using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using custom.Network;
using custom.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        public float latency = 0f;
        
        // State Params
        public int id;
        private bool clientResponding = false, registered = false, connected = true, initialized = false;
        private float clientTime = 0f, accumulatedTime_c2 = 0f;
        private int packetNumber = 0, lastCommandLocallyExcecuted = 0;
        private CharacterController myCharacterController;
        private GameObject concilliateGO;
        public GameObject concilliatePrefab;
        public int requiredSnapshots = 3;
        private int kills;

        private float mintime_between_inputs = 0.01f;
        private float current_time_between_inputs = 1f;
        
        private void Start()
        {
            Destroy(GameObject.Find("ServerCamera"));
            id = generate_id();
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
            
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                latency = 0.0f;
            } else if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                latency = 0.1f;
            } else if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                latency = 0.2f;
            } else if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                latency = 0.3f;
            } else if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                latency = 0.4f;
            } else if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                latency = 0.5f;
            }
            
            if (registered && connected)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }
                updateServerVisualization();

                int safe_check_limit = 10, currentIter = 0;
                
                while (interpolationBuffer.Count >= requiredSnapshots && currentIter++ < safe_check_limit) {
                    Interpolate();
                    Concilliate();
                }
            }
        }

        private void FixedUpdate()
        {
            accumulatedTime_c2 += Time.deltaTime;
            current_time_between_inputs += Time.deltaTime;

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
                    case Message.Type.GOODBYE: 
                        Application.Quit(); //Server shut down
                        break;
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
                StartCoroutine(sendMessage(mb.GenerateClientUpdateMessage().setArguments(commands)));
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
            var rotation = this.myCharacterController.gameObject.transform.rotation;
            var command = new Commands(packetNumber + 1,
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"),
                Input.GetKeyDown(KeyCode.Space), 
                timeout,                
                 rotation.eulerAngles.x,
                 rotation.eulerAngles.y,
                 rotation.eulerAngles.z
            );

            if (command.notNull())
            {
                current_time_between_inputs = 0f;
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
                    int toId = int.Parse(hit.transform.name.Split(' ')[1]);
                    StartCoroutine(sendMessage(mb.GenerateHitEnemyMessage(this.id, toId)));
                }
            }
        }
        private void Concilliate()
        {
            var auxClient = interpolationBuffer[interpolationBuffer.Count - 1].getEntityById(id);
            int currentServerCommandExcecuted = auxClient.AuxLastCommandProcessed;

            concilliateGO.transform.position = auxClient.AuxPosition;
            // concilliateGO.transform.rotation = auxClient.AuxRotation;
            
            foreach (var auxCommand in commands)
            {
                if (currentServerCommandExcecuted < auxCommand.number)
                {
                    currentServerCommandExcecuted = auxCommand.number;

                    Vector3 move = concilliateGO.transform.forward * auxCommand.y + concilliateGO.transform.right * auxCommand.x;
                    concilliateGO.GetComponent<CharacterController>().Move(Constants.speed * Time.deltaTime * move);
                }

            }

            myCharacterController.gameObject.transform.position = concilliateGO.transform.position;
            
            
            this.health = auxClient.Health;
            this.kills = auxClient.kills;

            if (this.health < Constants.min_health_alive)
            {
                // DIED
                
                SceneManager.LoadScene("Menu");
            }
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
                clientCube = Instantiate(clientCubePrefab3p, new Vector3(0, 0.05f, 0), new Quaternion());
                clientCube.name = "soldier " + idJoined;
            }

            CubeEntity newCubeEntity = new CubeEntity(clientCube, idJoined);

            if (!me)
            {
                clientCube.transform.Find("Cube").GetComponent<HealthSignal>().id = idJoined;
                clientCube.transform.Find("Cube").GetComponent<HealthSignal>().cm = this;
            }
            else
            {
                clientCube.transform.Find("Main Camera").transform.Find("HUD_Life").GetComponent<HealthSignal2>().id = idJoined;
                clientCube.transform.Find("Main Camera").transform.Find("HUD_Life").GetComponent<HealthSignal2>().cm = this;
                clientCube.transform.Find("Main Camera").transform.Find("Kills").GetComponent<Kills>().id = idJoined;
                clientCube.transform.Find("Main Camera").transform.Find("Kills").GetComponent<Kills>().cm = this;
                clientCube.transform.Find("Main Camera").transform.Find("Latency").GetComponent<Latency>().cm = this;

            }
            clientCubes.Add(newCubeEntity);
            return clientCube;
        }

        public bool isIdRegistered(int id)
        {
            return playerIds.Contains(id);
        }
        
        public void OnDestroy()
        {
            mb.GenerateGoodbye(id).Send();
            mb.Disconnect();
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
        
        public int getKills()
        {
            return this.kills;
        }

        public void deletePlayer(int id)
        {
            playerIds.Remove(id);
            foreach (var cube in clientCubes)
            {
                if (cube.Id.Equals(id))
                {
                    clientCubes.Remove(cube);
                    cube.GameObject.GetComponent<Animator>().SetTrigger("Death");
                    StartCoroutine(removeObject(cube));
                    return;
                }
            }
        }

        private IEnumerator removeObject(CubeEntity cube)
        {
            yield return new WaitForSeconds(5f);
            Destroy(cube.GameObject);
        }

        private IEnumerator sendMessage(Message message)
        {
            yield return new WaitForSeconds(latency);
            message.Send();
        }

        public float getLatency()
        {
            return latency;
        }
    }
}
