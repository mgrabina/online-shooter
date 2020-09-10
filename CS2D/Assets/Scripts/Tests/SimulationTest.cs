using System.Collections;
using System.Collections.Generic;
using System.Net;
using Tests;
using UnityEngine;

public class SimulationTest : MonoBehaviour
{

    // Networking
    private Channel visualizationChannel;
    private Channel clientInputChannel;
    private Channel serverACKChannel;

    private static string serverIP = "127.0.0.1";
    private static int visualizationChannelPort = 9001;
    private static int clientInputChannelPort = 9002;
    private static int serverACKChannelPort = 9003;

    
    private bool online = true;

    [SerializeField] private GameObject serverGameObject;
    [SerializeField] private GameObject clientGameObject;
    private CubeEntity serverCubeEntity;
    private CubeEntity clientCubeEntity;
    private Rigidbody serverRigidBody;
    
    List<Commands> commands = new List<Commands>();
    List<Snapshot> interpolationBuffer = new List<Snapshot>();
    
    // Parameters
    public static int pps = 10;
    public static int requiredSnapshots = 3;
    private static float sendRate = 1f / pps;


    private float accumulatedTime_c1 = 0f;
    private float accumulatedTime_c2 = 0f;
    private float clientTime = 0f;
    private int packetNumber = 0;
    private bool clientResponding = false;


    // Start is called before the first frame update
    void Start() {
        visualizationChannel = new Channel(visualizationChannelPort);
        clientInputChannel = new Channel(clientInputChannelPort);
        serverACKChannel = new Channel(serverACKChannelPort);
        
        serverCubeEntity = new CubeEntity(serverGameObject);
        clientCubeEntity = new CubeEntity(clientGameObject);
        serverRigidBody = serverGameObject.GetComponent<Rigidbody>();
    }

    private void OnDestroy() {
        visualizationChannel.Disconnect();
        clientInputChannel.Disconnect();
        serverACKChannel.Disconnect();
    }

    // Update is called once per frame
    void Update()
    {
        accumulatedTime_c1 += Time.deltaTime;
        accumulatedTime_c2 += Time.deltaTime;
        
        //apply input
        if (Input.GetKeyDown(KeyCode.Space)) {
            serverRigidBody.AddForceAtPosition(Vector3.up * 5, Vector3.zero, ForceMode.Impulse);
        }
        
        if (Input.GetKeyDown(KeyCode.D)) {
            online = !online;
        }

        if (online)
        {
            UpdateServer();
        }
        
        UpdateClient();
    }

    private void UpdateServer()
    {
        // Send New Position - Chanel 1
        if (this.accumulatedTime_c1 >= sendRate)
        {
            // Serialize
            var packet = Packet.Obtain();
            var snapshot = new Snapshot(this.packetNumber++, this.serverCubeEntity);
            snapshot.Serialize(packet.buffer);
            packet.buffer.Flush();

            var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), visualizationChannelPort);
            visualizationChannel.Send(packet, remoteEp);
            packet.Free();
            accumulatedTime_c1 -= sendRate;
        }
        
        // Recieve Input - Channel 2
        Packet inputPacket;
        while ((inputPacket = clientInputChannel.GetPacket()) != null)
        {
            int number = 0, limit = inputPacket.buffer.GetInt();
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
            
            // Send ACK - Channel 3
            var ackPacket = Packet.Obtain();
            ackPacket.buffer.PutInt(number);
            ackPacket.buffer.Flush();
            var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), serverACKChannelPort);
            serverACKChannel.Send(ackPacket, remoteEp);
            ackPacket.Free();
        }
    }
    
    private void UpdateClient() {
        
        
        // Update List - Channel 3
        Packet ack_packet;
        while ((ack_packet = serverACKChannel.GetPacket()) != null)
        {
            var toDelete = ack_packet.buffer.GetInt();
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
        
        // Send Input - Channel 2
        if (accumulatedTime_c2 >= sendRate)
        {
            ReadInput();
            var inputPacket = Packet.Obtain();
            inputPacket.buffer.PutInt(commands.Count);
            foreach (var command in commands)
            {
                command.Serialize((inputPacket.buffer));
            }

            inputPacket.buffer.Flush();
            var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), clientInputChannelPort);
            clientInputChannel.Send(inputPacket, remoteEp);
            inputPacket.Free();
            accumulatedTime_c2 -= sendRate;
        }
        
        // Recieve packet for visualization - Channel 1 
        var visualization_packet = visualizationChannel.GetPacket();
        if (visualization_packet != null)
        {
            // Recieved
            var snapshot = new Snapshot(-1, clientCubeEntity);
            var buffer = visualization_packet.buffer;
        
            //Deserialization
            clientCubeEntity.Deserialize(buffer);

            int interpolationBufferSize = interpolationBuffer.Count;
            if (interpolationBufferSize == 0 
                || snapshot.GetPacketNumber() > interpolationBuffer[interpolationBufferSize - 1].GetPacketNumber())
            {
                interpolationBuffer.Add(snapshot);    
            }
        }

        // Interpolation
        if (interpolationBuffer.Count >= requiredSnapshots)
        {
            clientResponding = true;
        } else if (interpolationBuffer.Count <= 1)
        {
            clientResponding = false;
        }

        if (clientResponding)
        {
            clientTime += Time.deltaTime;
            Interpolate();
        }
    }

    private void Interpolate()
    {
        var previousTime = (interpolationBuffer[0]).GetPacketNumber() * (1f / pps);
        var nextTime = interpolationBuffer[1].GetPacketNumber() * (1f / pps);
        var period = (clientTime - previousTime) / (nextTime - previousTime);
        var interpolatedSnapshot =
            Snapshot.createInterpolationSnapshot(interpolationBuffer[0], interpolationBuffer[1], period);
        interpolatedSnapshot.applyChanges();
        
        if (clientTime > nextTime)
        {
            interpolationBuffer.RemoveAt(0);
        }
    }
    
    private void ReadInput()
    {
        var timeout = Time.time + 2;
        var command = new Commands(packetNumber, 
            Input.GetKeyDown(KeyCode.UpArrow), 
            Input.GetKeyDown(KeyCode.DownArrow),
            Input.GetKeyDown(KeyCode.Space), timeout);
        commands.Add(command);
        if (Input.GetKeyDown(KeyCode.D))
        {
            clientInputChannel.Disconnect();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            clientInputChannel = new Channel(clientInputChannelPort);
        }
    }
}
