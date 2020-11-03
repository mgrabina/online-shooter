using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using custom.Network;
using custom.Utils;
using TMPro;
using UnityEngine;
using Camera = UnityEngine.Camera;
using Random = UnityEngine.Random;

namespace custom.Client
{
    public class ClientMessenger : MonoBehaviour, Messenger
    {
    
        // Networking
        [SerializeField] private GameObject clientCubePrefab3p;
        [SerializeField] private GameObject clientCubePrefab1p;
        private Transform camera;
        private float health = 1f;
        private Animator _animator;
        private HashSet<int> playerIds = new HashSet<int>();
        private List<CubeEntity> clientCubes;
        private List<Commands> commands = new List<Commands>();
        private List<Snapshot> interpolationBuffer = new List<Snapshot>();
        private MessageBuilder mb;

        // State Params
        public int id;
        private bool clientResponding = false, registered = false, connected = true, initialized = false;
        private float clientTime = 0f, accumulatedTime_c2 = 0f;
        private int packetNumber = 0, lastCommandLocallyExcecuted = 0;
        private CharacterController myCharacterController;
        private GameObject concilliateGO;
        public GameObject concilliatePrefab;
        public int requiredSnapshots = 3;

        private void Start()
        {
            Destroy(GameObject.Find("ServerCamera"));
            id = generate_id();
            Debug.Log("me " + id);
            mb = new MessageBuilder(id, Constants.clients_base_port + id*10, Constants.server_base_port, MasterBehavior.MasterData.ip);
            clientCubes = new List<CubeEntity>();
            if (!registered)
            {
                register();
            }

            concilliateGO = Instantiate(concilliatePrefab, new Vector3(0, 1f, 0), Quaternion.identity);
        }

        private void Update()
        {
            clientTime += Time.deltaTime;

            getAndProcessMessage();

            if (Input.GetKeyDown(KeyCode.M))
            {
                connected = !connected;
            }
            
            if (registered && connected)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }
                updateServerVisualization();
                
                while (interpolationBuffer.Count >= requiredSnapshots) {
                    Interpolate();
                    Concilliate();
                }
            }
        }

        private void FixedUpdate()
        {
            accumulatedTime_c2 += Time.deltaTime;

            if (clientResponding)
            {
                ReadInput();
                Predict();
                sendCommands();
            }
        }

        private void getAndProcessMessage()
        {
            Message message;
            while ((message = mb.GETChannelMessage()) != null)
            {
                switch (message.GetType)
                {
                    case Message.Type.PLAYER_JOINED: processPlayerJoined((PlayerJoinedMessage) message); break;
                    case Message.Type.INIT_STATUS:
                        if (initialized)
                        {
                            return;
                        }
                        processInitStatus((InitStatusMessage) message); break;
                    case Message.Type.GAME_STATE_UPDATE:
                        if (!registered || !connected)
                        {
                            return;
                        }
                        processServerUpdate((ServerUpdateMessage) message); break;
                    case Message.Type.CLIENT_UPDATE_ACK: 
                        if (!registered || !connected)
                        {
                            return;
                        }
                        processServerACK((ServerACKMessage) message); break;
                }
            }
        }

        private void register()
        {
            mb.GenerateJoinGameMessage(id).Send();
        }

        private void processInitStatus(InitStatusMessage message)
        {
            initialized = !initialized;
            var snapshot = new Snapshot(-1, clientCubes);
            var buffer = message.Packet.buffer;
            snapshot.Deserialize(buffer, this);
            
            Snapshot.setUniqueSnapshot(snapshot).applyChanges(id);
            setCurrentHealths(snapshot, clientCubes);

        }
        
        private void processPlayerJoined(PlayerJoinedMessage message)
        {
            int idJoined = (message).IdJoined();
            if (playerIds.Contains(idJoined))
            {
                return;
            }
            GameObject clientCube = createClient(idJoined, idJoined == this.id);
            if (idJoined == this.id)
            {
                registered = true;
                myCharacterController = clientCube.GetComponent<CharacterController>();
                this._animator = clientCube.GetComponent<Animator>();
                clientCube.GetComponentInChildren<Camera>().tag = "MasterClient";
            }
            else
            {
                clientCube.GetComponentInChildren<Camera>().tag = "SecondaryClient";
            }
        }

        private void processServerUpdate(ServerUpdateMessage message)
        {
            // Recieved
            var snapshot = new Snapshot(-1, clientCubes);
            var buffer = message.Packet.buffer;
            snapshot.Deserialize(buffer, this);

            int interpolationBufferSize = interpolationBuffer.Count;
            if ((interpolationBufferSize == 0
                || snapshot.GetPacketNumber() > interpolationBuffer[interpolationBufferSize - 1].GetPacketNumber()) 
                && interpolationBufferSize < requiredSnapshots + 1)
            {
                interpolationBuffer.Add(snapshot);
            }
        }
        
        private void updateServerVisualization()
        {
            // Interpolation
            if (interpolationBuffer.Count >= Constants.requiredSnapshots)
            {
                clientResponding = true;
            }
            else if (interpolationBuffer.Count <= 1)
            {
                clientResponding = false;
            }
        }

        private void sendCommands()
        {
            if (accumulatedTime_c2 >= Constants.sendRate)
            {
                mb.GenerateClientUpdateMessage().setArguments(commands).Send();
                accumulatedTime_c2 -= Constants.sendRate;
            }
        }

        private void processServerACK(ServerACKMessage message)
        {
            var toDelete = message.getNumber();
            while (commands.Count != 0)
            {
                if (commands[0].number <= toDelete || commands[0].timestamp < Time.time)
                {
                    commands.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
        }
        private void Interpolate()
        {
            if (interpolationBuffer.Count < 2)
            {
                return;
            }
            
            var previousTime = (interpolationBuffer[0]).GetPacketNumber() * (1f / Constants.pps);
            var nextTime = interpolationBuffer[1].GetPacketNumber() * (1f / Constants.pps);
            var period = (clientTime - previousTime) / (nextTime - previousTime);
            var interpolatedSnapshot =
                Snapshot.createInterpolationSnapshot(interpolationBuffer[0], interpolationBuffer[1], period, id, this);
            interpolatedSnapshot.applyChanges(id);
            setCurrentHealths(interpolatedSnapshot, clientCubes);
            
            if (clientTime > nextTime)
            {
                interpolationBuffer.RemoveAt(0);
            }
        }

        private void setCurrentHealths(Snapshot snap, List<CubeEntity> cubes)
        {
            foreach (var cube in cubes)
            {
                CubeEntity other = snap.getEntityById(cube.Id);
                if (other == null)
                {
                    //me
                }
                else
                {
                    cube.Health = other.Health;
                }
            }   
        }

        private void ReadInput()
        {
            var timeout = Time.time + 2;
            var command = new Commands(packetNumber + 1,
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"),
                Input.GetKeyDown(KeyCode.Space), 
                timeout,                
                Input.GetAxis("Mouse X")
            );

            if (command.notNull())
            {
                commands.Add(command);
                packetNumber++;
            }
        }

        private void Predict()
        {
            foreach (Commands commands in commands)
            {
                if (commands.number > lastCommandLocallyExcecuted)
                {
                    lastCommandLocallyExcecuted = commands.number;
                    
                    Vector3 move = myCharacterController.gameObject.transform.forward * commands.y 
                                   + myCharacterController.gameObject.transform.right * commands.x;
                    myCharacterController.
                        Move(Constants.speed * Time.deltaTime * move);
                    myCharacterController.gameObject.transform.Rotate(0, commands.mouse_x * Constants.mouseSensibility, 0);
            
                    myCharacterController.transform.Find("Main Camera").transform.rotation = myCharacterController.transform.rotation;
                }
            }
        }

        private void Shoot()
        {
            Ray ray = myCharacterController.transform.Find("Main Camera")
                .gameObject.GetComponent<UnityEngine.Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit[] allhHit = Physics.RaycastAll(ray).OrderBy(h=>h.distance).ToArray();
            foreach (var hit in allhHit)
            {
                if (hit.transform.name.Contains("soldier"))
                {
                    int id = int.Parse(hit.transform.name.Split(' ')[1]);
                    mb.GenerateHitEnemyMessage(id).Send();
                }
            }
        }
        private void Concilliate()
        {
            var auxClient = interpolationBuffer[interpolationBuffer.Count - 1].getEntityById(id);
            int currentServerCommandExcecuted = auxClient.AuxLastCommandProcessed;

            concilliateGO.transform.position = auxClient.AuxPosition;
            concilliateGO.transform.rotation = auxClient.AuxRotation;
            
            foreach (var auxCommand in commands)
            {
                if (currentServerCommandExcecuted < auxCommand.number)
                {
                    currentServerCommandExcecuted = auxCommand.number;

                    Vector3 move = concilliateGO.transform.forward * auxCommand.y + concilliateGO.transform.right * auxCommand.x;
                    concilliateGO.GetComponent<CharacterController>().Move(Constants.speed * Time.deltaTime * move);
                    // concilliateGO.transform.Rotate(0, auxCommand.mouse_x * Constants.mouseSensibility, 0);
                }

            }

            myCharacterController.gameObject.transform.position = concilliateGO.transform.position;

            myCharacterController.gameObject.transform.rotation = concilliateGO.transform.rotation;
            
            
            this.health = auxClient.Health;
        }

        private static int generate_id()
        {
            return Random.Range(0, 100);
        }

        public GameObject createClient(int idJoined, bool me)
        {
            playerIds.Add(idJoined);
            GameObject clientCube;
            if (me)
            {
                clientCube = Instantiate(clientCubePrefab1p, new Vector3(0, 0.2f, 0), new Quaternion());
                camera = clientCube.transform.Find("Main Camera");
            }
            else
            {
                clientCube = Instantiate(clientCubePrefab3p, new Vector3(0, 0.2f, 0), new Quaternion());
                clientCube.name = "soldier " + idJoined;
            }

            CubeEntity newCubeEntity = new CubeEntity(clientCube, idJoined);

            if (!me)
            {
                clientCube.transform.Find("Cube").GetComponent<HealthSignal>().id = idJoined;
                clientCube.transform.Find("Cube").GetComponent<HealthSignal>().cm = this;
            }
            clientCubes.Add(newCubeEntity);
            return clientCube;
        }

        public bool isIdRegistered(int id)
        {
            return playerIds.Contains(id);
        }
        
        public void OnDestroy() {
            mb.Disconnect();
        }

        private void setTransform(Vector3 position, Quaternion rotation, Transform transform)
        {
            transform.position = new Vector3(position.x, position.y, position.z);
            transform.rotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
        }

        public float getCurrentHealth(int id)
        {
            foreach (var cube in clientCubes)
            {
                if (cube.Id.Equals(id))
                {
                    return cube.Health;
                }   
            }

            return -1f;
        }

        public void deletePlayer(int id)
        {
            playerIds.Remove(id);
            foreach (var cube in clientCubes)
            {
                if (cube.Id.Equals(id))
                {
                    clientCubes.Remove(cube);
                    Destroy(cube.GameObject);
                }
            }
        }
    }
}
