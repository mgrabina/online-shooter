using System.Net;
using custom.Server;
using JetBrains.Annotations;
using UnityEngine;

namespace custom.Network
{
    public class MessageBuilder
    {
        private readonly int _gameMemberId;
        private readonly string _destinationIP;
        
        private readonly Channel _channel;
        private readonly int _channelPortD;

        public MessageBuilder(int gameMemberId, int sourcePortBase, int destinationPortBase, string destinationIp)
        {
            Debug.Log("Connecting to " + destinationIp + " " + destinationPortBase);
            _channel = new Channel(sourcePortBase);
            this._gameMemberId = gameMemberId;
            this._channelPortD = destinationPortBase;
            this._destinationIP = destinationIp;
            Debug.Log(gameMemberId);
        }

        
        // Client Side
        
        public JoinGameMessage GenerateJoinGameMessage(int id)
        {
            Debug.Log("Sending join game to " + _destinationIP + " " + _channelPortD);
            return new JoinGameMessage(id, _channel, _destinationIP, _channelPortD);
        }

        public ClientUpdateMessage GenerateClientUpdateMessage()
        {
            return new ClientUpdateMessage(_gameMemberId, _channel, _destinationIP, _channelPortD);
        }
        
        // Server Side
        public PlayerJoinedMessage GeneratePlayerJoinedMessage(PlayerInfo player)
        {
            return new PlayerJoinedMessage(-1, _channel, player.EndPoint.Address.ToString(), Constants.clients_base_port + 10*player.Id);
        }

        public ServerUpdateMessage GenerateServerUpdateMessage(PlayerInfo player)
        {
            return new ServerUpdateMessage(-1, _channel, player.EndPoint.Address.ToString(), Constants.clients_base_port + 10*player.Id);
        }
        
        public InitStatusMessage GenerateInitStatusMessage(PlayerInfo player)
        {
            return new InitStatusMessage(-1, _channel, player.EndPoint.Address.ToString(), Constants.clients_base_port + 10*player.Id);
        }

        public ServerACKMessage GenerateServerAckMessage(PlayerInfo player)
        {
            return new ServerACKMessage(-1, _channel, player.EndPoint.Address.ToString(), Constants.clients_base_port + 10*player.Id);
        }
        
        // Read Messages from channels
        
        [CanBeNull]
        public Message GETChannelMessage()
        {
            Packet packet = _channel.GetPacket();
            if (packet != null)
            {
                Debug.Log(packet.fromEndPoint.Address.MapToIPv4().ToString());
            }
            return packet != null ? Message.getMessage(packet) : null;
        }

        private Message GETMessage(Packet incomePacket)
        {
            return Message.getMessage(incomePacket);
        }

        public void Disconnect()
        {
            _channel.Disconnect();
        }
    }
}