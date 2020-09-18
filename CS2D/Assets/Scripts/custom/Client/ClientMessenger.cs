using System.Collections.Generic;
using System.Net;
using custom.Utils;
using lib.Network;
using UnityEngine;

namespace custom.Client
{
    public class ClientMessenger : MonoBehaviour
    {
    
        // Networking
        private Channel registrationChannel;
        private Channel visualizationChannel;
        private Channel clientInputChannel;
        private Channel serverACKChannel;
    
        private CubeEntity clientCubeEntity;
    
        private List<Commands> commands = new List<Commands>();
        private List<Snapshot> interpolationBuffer = new List<Snapshot>();

        private float accumulatedTime_c2 = 0f;
        private float clientTime = 0f;
        private bool clientResponding = false;
        private int packetNumber = 0;

        public int id;
    
        private void Start()
        {
            registrationChannel = new Channel(null, Constants.client_registrationChannelPort, Constants.server_registrationChannelPort);
            visualizationChannel = new Channel(null, Constants.client_visualizationChannelPort, Constants.server_visualizationChannelPort);
            clientInputChannel = new Channel(null, Constants.client_clientInputChannelPort, Constants.server_clientInputChannelPort);
            serverACKChannel = new Channel(null, Constants.client_serverACKChannelPort, Constants.server_serverACKChannelPort);

            clientCubeEntity = new CubeEntity(gameObject);

            register();
        }

        private void Update()
        {
            accumulatedTime_c2 += Time.deltaTime;
        
            recieveCommandsACK();
        
            sendCommands();
        
            updateServerVisualization();
        }

        private void register()
        {
            var registerPaket = Packet.Obtain();
            registerPaket.buffer.PutInt(id);
            registerPaket.buffer.Flush();
            var remoteEp = new IPEndPoint(IPAddress.Parse(Constants.serverIP), Constants.server_registrationChannelPort);
            registrationChannel.Send(registerPaket, remoteEp);
            registerPaket.Free();
        }

        private void updateServerVisualization()
        {
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
            if (interpolationBuffer.Count >= Constants.requiredSnapshots)
            {
                clientResponding = true;
            }
            else if (interpolationBuffer.Count <= 1)
            {
                clientResponding = false;
            }

            if (clientResponding)
            {
                clientTime += Time.deltaTime;
                Interpolate();
            }
        }

        private void sendCommands()
        {
            if (accumulatedTime_c2 >= Constants.sendRate)
            {
                ReadInput();
                var inputPacket = Packet.Obtain();
                inputPacket.buffer.PutInt(id);
                inputPacket.buffer.PutInt(commands.Count);
                foreach (var command in commands)
                {
                    command.Serialize((inputPacket.buffer));
                }

                inputPacket.buffer.Flush();
                var remoteEp = new IPEndPoint(IPAddress.Parse(Constants.serverIP), Constants.server_clientInputChannelPort);
                clientInputChannel.Send(inputPacket, remoteEp);
                inputPacket.Free();
                accumulatedTime_c2 -= Constants.sendRate;
            }
        }

        private void recieveCommandsACK()
        {
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
        }
        private void Interpolate()
        {
            var previousTime = (interpolationBuffer[0]).GetPacketNumber() * (1f / Constants.pps);
            var nextTime = interpolationBuffer[1].GetPacketNumber() * (1f / Constants.pps);
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
            var command = new Commands(packetNumber++, 
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
                clientInputChannel = new Channel(Constants.server_clientInputChannelPort);
            }
        }
    
    }
}
