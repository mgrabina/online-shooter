using System;
using System.Collections.Generic;
using System.Net;
using Tests;
using UnityEngine;

public class ServerMessenger : MonoBehaviour
{
    
    // Networking
    private Channel registrationChannel;
    private Channel visualizationChannel;
    private Channel clientInputChannel;
    private Channel serverACKChannel;

    
    private CubeEntity serverCubeEntity;
    private Rigidbody serverRigidBody;

    private float accumulatedTime_c1 = 0f;
    private int packetNumber = 0;

    private bool online = true;

    private HashSet<PlayerInfo> players = new HashSet<PlayerInfo>();
    
    private void Start()
    {
        registrationChannel = new Channel(null, Constants.registrationChannelPort, Constants.registrationChannelPort);
        visualizationChannel = new Channel(null, Constants.visualizationChannelPort, Constants.visualizationChannelPort);
        clientInputChannel = new Channel(null, Constants.clientInputChannelPort, Constants.clientInputChannelPort);
        serverACKChannel = new Channel(null, Constants.serverACKChannelPort, Constants.serverACKChannelPort);
        serverCubeEntity = new CubeEntity(gameObject);
        serverRigidBody = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        accumulatedTime_c1 += Time.deltaTime;
    
        //apply input
        if (Input.GetKeyDown(KeyCode.Space)) {
            serverRigidBody.AddForceAtPosition(Vector3.up * 5, Vector3.zero, ForceMode.Impulse);
        }
        
        if (Input.GetKeyDown(KeyCode.D)) {
            online = !online;
        }

        if (online)
        {
            ListenForNewConnections();
            SendNewPosition();
            recieveClientCommands();
        }
    }

    public void ListenForNewConnections()
    {
        Packet inputPacket;
        while ((inputPacket = registrationChannel.GetPacket()) != null)
        {
            int id = inputPacket.buffer.GetInt();
            IPEndPoint endPoint = inputPacket.fromEndPoint;
            players.Add(new PlayerInfo(id, endPoint));
        }
    }
    
    public void SendNewPosition()
    {
        if (this.accumulatedTime_c1 >= Constants.sendRate)
        {
            foreach (var player in players)
            {
                // Serialize
                var packet = Packet.Obtain();
                var snapshot = new Snapshot(this.packetNumber++, this.serverCubeEntity);
                snapshot.Serialize(packet.buffer);
                packet.buffer.Flush();

                visualizationChannel.Send(packet, player.EndPoint);
                packet.Free();
                accumulatedTime_c1 -= Constants.sendRate;
            }
        }   
    }

    public void recieveClientCommands()
    {
        Packet inputPacket;
        while ((inputPacket = clientInputChannel.GetPacket()) != null)
        {
            int number = 0, id = inputPacket.buffer.GetInt(), limit = inputPacket.buffer.GetInt();
            for (int i = 0; i < limit; i++)
            {
                var commands = new Commands();
                commands.Deserialize(inputPacket.buffer);
                if (commands.space)
                {
                    serverRigidBody.AddForceAtPosition(Vector3.up * 2, Vector3.zero, ForceMode.Impulse);
                }
                if (commands.up)
                {
                    serverRigidBody.AddForceAtPosition(Vector3.up * 10, Vector3.zero, ForceMode.Impulse);
                }

                number = commands.number;
            }
            sendClientCommandACK(number, id);
        }
    }

    public void sendClientCommandACK(int number, int id)
    {
        PlayerInfo pi = GetPlayerById(id);
        if (pi == null)
        {
            throw new Exception("Invalid ID");
        }
        var ackPacket = Packet.Obtain();
        ackPacket.buffer.PutInt(number);
        ackPacket.buffer.Flush();
        serverACKChannel.Send(ackPacket, pi.EndPoint);
        ackPacket.Free();   
    }
    
    public void OnDestroy() {
        visualizationChannel.Disconnect();
        clientInputChannel.Disconnect();
        serverACKChannel.Disconnect();
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
