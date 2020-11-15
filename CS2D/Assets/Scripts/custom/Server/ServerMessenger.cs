using System;
using System.Collections.Generic;
using System.Net;
using custom.Client;
using custom.Network;
using custom.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace custom.Server
{
    public class ServerMessenger : MonoBehaviour, Messenger
    {
        private HashSet<PlayerInfo> players = new HashSet<PlayerInfo>();
        private List<CubeEntity> serverCubes;
        private Dictionary<int, int> kills;
        
        private float accumulatedTime_c1 = 0f;
        private int packetNumber = 0;

        private bool online = false;

        private MessageBuilder mb;
        public GameObject serverGameObject;

        private Dictionary<int, int> lastSnapshot;
        
        private void Awake()
        {
            mb = new MessageBuilder(-1, Constants.server_base_port, Constants.clients_base_port, Constants.serverIP);
            online = true;
            serverCubes = new List<CubeEntity>();
            lastSnapshot = new Dictionary<int, int>();
            kills = new Dictionary<int, int>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.D)) {
                online = !online;
            }

            if (online)
            {
                getAndProcessMessage();
            }
        }

        public void FixedUpdate()
        {
            accumulatedTime_c1 += Time.deltaTime;
            
            if (online && players.Count > 0)
            {
                SendUpdates();
            }
            
            regenerateHealth();
        }

        private void getAndProcessMessage()
        {
            Message message;
            while ((message = mb.GETChannelMessage()) != null)
            {
                if (!online)
                {
                    return;
                }
                switch (message.GetType)
                {
                    case Message.Type.HIT_ENEMY_MESSAGE: newHittedPlayer((HitEnemyMessage) message); break;
                    case Message.Type.JOIN_GAME: processJoinGame((JoinGameMessage) message); break;
                    case Message.Type.CLIENT_UPDATE: processClientInput((ClientUpdateMessage) message); break;
                    case Message.Type.GOODBYE: processGoodbye((GoodbyeMessage) message); break;
                }
            }
        }

        private void newHittedPlayer(HitEnemyMessage message)
        {
            int fromId = message.FromId;
            int toId = message.ToId;
            foreach (CubeEntity player in serverCubes)
            {
                if (player.Id.Equals(toId))
                {
                    player.decrementHealth();
                    if (!player.isAlive())
                    {
                        registerKill(fromId);

                        foreach (var p in players)
                        {
                            if (p.Id.Equals(player.Id))
                            {
                                mb.GenerateGoodbye(p).Send();
                                break;
                            }   
                        }
                        
                        Destroy(player.GameObject);
                        serverCubes.Remove(player);
                        players.Remove(new PlayerInfo(toId, null));
                    }
                }
            }
        }

        public void processGoodbye(GoodbyeMessage message)
        {
            int id = message.GetId;

            foreach (CubeEntity player in serverCubes)
            {
                if (player.Id.Equals(id))
                {
                    Destroy(player.GameObject);
                    serverCubes.Remove(player);
                    players.Remove(new PlayerInfo(id, null));
                }
            } 
        }
        
        public void processJoinGame(JoinGameMessage message)
        {
            int id = message.GetId;
            IPEndPoint endPoint = message.Packet.fromEndPoint;
            if (!players.Contains(new PlayerInfo(id, endPoint)))
            {
                lastSnapshot.Add(id, 0);
                kills.Add(id, 0);
                players.Add(new PlayerInfo(id, endPoint));
                var serverCube = Instantiate(serverGameObject, 
                    new Vector3(Random.Range(-20, 20), 0.5f, Random.Range(-20,20)), Quaternion.identity);
                // serverCube.layer = 8; // Server Layer
                SetLayerRecursively(serverCube, 8);
                CubeEntity newcube = new CubeEntity(serverCube, id);
                serverCube.transform.Find("Cube").GetComponent<HealthSignal>().cm = this;
                serverCube.transform.Find("Cube").GetComponent<HealthSignal>().id = id;
                serverCubes.Add( newcube );
                SendPlayerJoined(id);
                SendInitStatus(id);
            }
            
        }
        void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (null == obj)
            {
                return;
            }
       
            obj.layer = newLayer;
       
            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        
        public void SendInitStatus(int id)
        {
            PlayerInfo pi = GetPlayerById(id);
            if (pi == null)
            {
                throw new Exception("Invalid ID");
            }
            mb.GenerateInitStatusMessage(pi).setArguments(new Snapshot(lastSnapshot[id]++, serverCubes)).Send();
        }
        
        public void SendPlayerJoined(int id)
        {
            foreach (var player in players)
            {
                mb.GeneratePlayerJoinedMessage(player).setArguments(id).Send();
            }
        }
    
        public void SendUpdates()
        {
            if (this.accumulatedTime_c1 >= Constants.sendRate)
            {
                foreach (var player in players)
                {
                    mb.GenerateServerUpdateMessage(player)
                        .setArguments(new Snapshot(lastSnapshot[player.Id]++, serverCubes)).Send();
                    accumulatedTime_c1 -= Constants.sendRate;
                    packetNumber++;
                }
            }   
        }

        public void processClientInput(ClientUpdateMessage message)
        {
            int n = -1;
            foreach (Commands commands in (message).Commands)
            {
                foreach (var cube in serverCubes)
                {
                    if (cube.Id.Equals(message.GetId))
                    {
                        cube.GameObject.transform.rotation = Quaternion.Euler(commands.rotation_x, commands.rotation_y,commands.rotation_z);
                        Vector3 move = cube.GameObject.transform.forward * commands.y 
                                       + cube.GameObject.transform.right * commands.x;
                        cube.GameObject.GetComponent<CharacterController>().
                            Move(Constants.speed * Time.deltaTime * move);

                        cube.LastCommandProcessed = commands.number;
                        break;
                    }
                }

                n = commands.number;
            }
            sendClientCommandACK(n, message.GetId);
        }

        public void sendClientCommandACK(int number, int id)
        {
            PlayerInfo pi = GetPlayerById(id);
            if (pi == null)
            {
                throw new Exception("Invalid ID");
            }
            
            mb.GenerateServerAckMessage(pi).setArguments(number).Send();
        }
    
        public void OnDestroy() {
            foreach (var player in players)
            {
                mb.GenerateGoodbye(player).Send();
            }
            mb.Disconnect();
        }

        public PlayerInfo GetPlayerById(int id)
        {
            foreach (var player in players)
            {
                if (player.Id.Equals(id))
                {
                    return player;
                }
            }
            return null;
        }

        public void registerKill(int id)
        {
            kills[id] = kills[id] + 1;
            foreach (CubeEntity player in serverCubes)
            {
                player.incrementKills();
            }
        }
        
        public void regenerateHealth()
        {
            foreach (CubeEntity player in serverCubes)
            {
                player.incrementHealth();
            }
        }
        
        public float getCurrentHealth(int id)
        {
            foreach (var cube in serverCubes)
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
            return -1;
        }

        public float getLatency()
        {
            return 0f;
        }
    }
}
