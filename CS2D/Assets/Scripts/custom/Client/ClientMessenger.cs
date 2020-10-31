using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using custom.Network;
using custom.Utils;
using UnityEngine;
using Camera = UnityEngine.Camera;
using Random = UnityEngine.Random;

namespace custom.Client
{
    public class ClientMessenger : MonoBehaviour
    {
    
        // Networking
        [SerializeField] private GameObject clientCubePrefab3p;
        [SerializeField] private GameObject clientCubePrefab1p;
        private Transform camera;
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
        private CharacterController myRigidbody;
        private Transform concilliate;
        
        private void Start()
        {
            id = generate_id();
            mb = new MessageBuilder(id, Constants.clients_base_port + id*10, Constants.server_base_port, MasterBehavior.MasterData.ip);
            clientCubes = new List<CubeEntity>();
            if (!registered)
            {
                register();
            }

            concilliate = new GameObject().transform;
        }

        private void Update()
        {
            
            getAndProcessMessage();

            if (Input.GetKeyDown(KeyCode.M))
            {
                connected = !connected;
            }
            
            if (registered && connected)
            {
                updateServerVisualization();
            }
        }

        private void FixedUpdate()
        {
            clientTime += Time.deltaTime;
            accumulatedTime_c2 += Time.deltaTime;

            if (clientResponding)
            {
                ReadInput();
                Predict();
                sendCommands();
                Interpolate();
                Concilliate();
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
                myRigidbody = clientCube.GetComponent<CharacterController>();
                setTransform(myRigidbody.transform.position, myRigidbody.transform.rotation, concilliate);
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
            if (interpolationBufferSize == 0
                || snapshot.GetPacketNumber() > interpolationBuffer[interpolationBufferSize - 1].GetPacketNumber())
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
            var previousTime = (interpolationBuffer[0]).GetPacketNumber() * (1f / Constants.pps);
            var nextTime = interpolationBuffer[1].GetPacketNumber() * (1f / Constants.pps);
            var period = (clientTime - previousTime) / (nextTime - previousTime);
            var interpolatedSnapshot =
                Snapshot.createInterpolationSnapshot(interpolationBuffer[0], interpolationBuffer[1], period, id, this);
            interpolatedSnapshot.applyChanges(id);
    
            if (clientTime > nextTime)
            {
                interpolationBuffer.RemoveAt(0);
            }
        }

        private void ReadInput()
        {
            var timeout = Time.time + 2;
            var command = new Commands(packetNumber++,
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"),
                Input.GetKeyDown(KeyCode.Space), 
                timeout,                
                Input.GetAxis("Mouse X")
            );
            if (command.notNull())
            {
                commands.Add(command);
            }
            else
            {
                packetNumber--;
            }
        }

        private void Predict()
        {
            foreach (Commands commands in commands)
            {
                if (commands.number > lastCommandLocallyExcecuted)
                {
                    lastCommandLocallyExcecuted = commands.number;
                    
                    myRigidbody.transform.Translate(
                        Commands.generateStraffe(commands), 0, Commands.generateTranslation(commands));
                    if (!_animator.GetCurrentAnimatorStateInfo(1).IsName("Walking"))
                    {
                        _animator.SetTrigger("Walk");   
                    }
                }
            }
        }

        private void Concilliate()
        {
            CubeEntity lastFromServer = interpolationBuffer.Last().getEntityById(id);
            setTransform(lastFromServer.AuxPosition, lastFromServer.AuxRotation, concilliate);
            int currentServerCommandExcecuted = lastFromServer.AuxLastCommandProcessed;
    
            foreach (var command in commands)    
            {
                if (currentServerCommandExcecuted < command.number)
                {
                    currentServerCommandExcecuted = command.number;
                    
                    concilliate.Translate(
                        Commands.generateStraffe(command), 0, Commands.generateTranslation(command));
                    
                }
            }

            myRigidbody.transform.position = concilliate.position;
            myRigidbody.transform.rotation = concilliate.rotation;
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
                Debug.Log(idJoined);
                clientCube = Instantiate(clientCubePrefab3p, new Vector3(0, 0.2f, 0), new Quaternion());
            }
            
            clientCubes.Add(new CubeEntity(clientCube, idJoined));
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
    }
}
