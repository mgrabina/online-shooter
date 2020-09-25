using System;
using System.Collections.Generic;
using System.Net;
using custom.Client;
using custom.Network;
using custom.Utils;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace custom.Server
{
    public class ServerMessenger : MonoBehaviour
    {
        private HashSet<PlayerInfo> players = new HashSet<PlayerInfo>();
        private List<CubeEntity> serverCubes;
        
        
        private float accumulatedTime_c1 = 0f;
        private int packetNumber = 0;

        private bool online = true;
        
        private MessageBuilder mb;
        public GameObject serverGameObject;

        
        private void Start()
        {
            mb = new MessageBuilder(-1, Constants.server_base_port, Constants.clients_base_port,null);
            serverCubes = new List<CubeEntity>();

        }

        private void Update()
        {
            accumulatedTime_c1 += Time.deltaTime;
            
            if (Input.GetKeyDown(KeyCode.D)) {
                online = !online;
            }

            if (online)
            {
                ListenForNewConnections();
                SendUpdates();
                recieveClientCommands();
            }
        }

        public void ListenForNewConnections()
        {
            Message newMessage;
            while ((newMessage = mb.getRegistrationChannelMessage()) != null)
            {
                int id = newMessage.GetId;
                IPEndPoint endPoint = newMessage.Packet.fromEndPoint;
                if (!players.Contains(new PlayerInfo(id, endPoint)))
                {
                    players.Add(new PlayerInfo(id, endPoint));
                    var serverCube = Instantiate(serverGameObject, new Vector3(Random.Range(-4, 4), 1, Random.Range(-4,4)), Quaternion.identity);
                    serverCubes.Add( new CubeEntity(serverCube, id) );
                    SendPlayerJoined(id);
                }
            }
        }

        public void SendPlayerJoined(int id)
        {
            foreach (var player in players)
            {
                mb.generatePlayerJoinedMessage(player).setArguments(id).Send();
            }
        }
    
        public void SendUpdates()
        {
            if (this.accumulatedTime_c1 >= Constants.sendRate)
            {
                foreach (var player in players)
                {
                    mb.generateServerUpdateMessage(player).setArguments(new Snapshot(this.packetNumber++, serverCubes)).Send();
                    accumulatedTime_c1 -= Constants.sendRate;
                }
            }   
        }

        public void recieveClientCommands()
        {
            Message recievedMessage;
            while ((recievedMessage = mb.getClientInputChannelMessage()) != null)
            {
                if (recievedMessage.GetType == Message.Type.CLIENT_UPDATE)
                {
                    int n = -1;
                    foreach (Commands commands in ((ClientUpdateMessage)recievedMessage).Commands)
                    {
                        Vector3 force = Commands.generateForce(commands);                        

                        foreach (var cube in serverCubes)
                        {
                            if (cube.Id.Equals(recievedMessage.GetId))
                            {
                                cube.GameObject.GetComponent<Rigidbody>().AddForceAtPosition(force, Vector3.zero, ForceMode.Impulse);
                                cube.LastCommandProcessed = commands.number;
                                break;
                            }
                        }

                        n = commands.number;
                    }
                    sendClientCommandACK(n, recievedMessage.GetId);

                }
                
            }
        }

        public void sendClientCommandACK(int number, int id)
        {
            PlayerInfo pi = GetPlayerById(id);
            if (pi == null)
            {
                throw new Exception("Invalid ID");
            }
            
            mb.generateServerACKMessage(pi).setArguments(number).Send();
        }
    
        public void OnDestroy() {
            mb.disconnect();
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
    }
}
