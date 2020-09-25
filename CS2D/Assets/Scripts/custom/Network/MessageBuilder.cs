using System.Net;
using custom.Server;
using JetBrains.Annotations;

namespace custom.Network
{
    public class MessageBuilder
    {
        private int gameMemberId;
        private string destination_ip;
        
        private Channel registrationChannel;
        private int registrationChannelPort_s;
        private int registrationChannelPort_d;
        
        private Channel playerJoinedChannel;
        private int playerJoinedChannelPort_s;
        private int playerJoinedChannelPort_d;
        
        private Channel serverBroadcastChannel;
        private int serverBroadcastChannelPort_s;
        private int serverBroadcastChannelPort_d;

        private Channel clientInputChannel;
        private int clientInputChannelPort_s;
        private int clientInputChannelPort_d;

        private Channel serverACKChannel;
        private int serverACKChannelPort_s;
        private int serverACKChannelPort_d;


        public MessageBuilder(int gameMemberId, int sourcePortBase, int destinationPortBase, string destinationIp)
        {
            registrationChannel = new Channel(null, registrationChannelPort_s = sourcePortBase++, registrationChannelPort_d = destinationPortBase++);
            playerJoinedChannel = new Channel(null, playerJoinedChannelPort_s = sourcePortBase++, playerJoinedChannelPort_d = destinationPortBase++);
            serverBroadcastChannel = new Channel(null, serverBroadcastChannelPort_s = sourcePortBase++, serverBroadcastChannelPort_d = destinationPortBase++);
            clientInputChannel = new Channel(null, clientInputChannelPort_s = sourcePortBase++, clientInputChannelPort_d = destinationPortBase++);
            serverACKChannel = new Channel(null, serverACKChannelPort_s = sourcePortBase++, serverACKChannelPort_d = destinationPortBase++);
            this.gameMemberId = gameMemberId;
            this.destination_ip = destinationIp;
        }

        
        // Client Side
        
        public JoinGameMessage generateJoinGameMessage(int id)
        {
            return new JoinGameMessage(id, registrationChannel, destination_ip, registrationChannelPort_d);
        }

        public ClientUpdateMessage generateClientUpdateMessage()
        {
            return new ClientUpdateMessage(gameMemberId, clientInputChannel, destination_ip, clientInputChannelPort_d);
        }
        
        // Server Side
        public PlayerJoinedMessage generatePlayerJoinedMessage(PlayerInfo player)
        {
            return new PlayerJoinedMessage(-1, playerJoinedChannel, player.EndPoint.Address.ToString(), Constants.clients_base_port + 10*player.Id + 1);
        }

        public ServerUpdateMessage generateServerUpdateMessage(PlayerInfo player)
        {
            return new ServerUpdateMessage(-1, serverBroadcastChannel, player.EndPoint.Address.ToString(), Constants.clients_base_port + 10*player.Id + 2);
        }

        public ServerACKMessage generateServerACKMessage(PlayerInfo player)
        {
            return new ServerACKMessage(-1, serverACKChannel, player.EndPoint.Address.ToString(), Constants.clients_base_port + 10*player.Id + 4);
        }
        
        // Read Messages from channels
        
        [CanBeNull]
        public Message getRegistrationChannelMessage()
        {
            Packet packet = registrationChannel.GetPacket();
            return packet != null ? Message.getMessage(packet) : null;
        }
        
        [CanBeNull]
        public Message getPlayerJoinedChannelMessage()
        {
            Packet packet = playerJoinedChannel.GetPacket();
            return packet != null ? Message.getMessage(packet) : null;
        }
        
        [CanBeNull]
        public Message getClientInputChannelMessage()
        {
            Packet packet = clientInputChannel.GetPacket();
            return packet != null ? Message.getMessage(packet) : null;
        }
        
        [CanBeNull]
        public Message getServerBroadcastChannelMessage()
        {
            Packet packet = serverBroadcastChannel.GetPacket();
            return packet != null ? Message.getMessage(packet) : null;
        }
        
        [CanBeNull]
        public Message getServerACKChannelMessage()
        {
            Packet packet = serverACKChannel.GetPacket();
            return packet != null ? Message.getMessage(packet) : null;
        }
        
        
        private Message getMessage(Packet incomePacket)
        {
            return Message.getMessage(incomePacket);
        }

        public void disconnect()
        {
            registrationChannel.Disconnect();
            clientInputChannel.Disconnect();
            playerJoinedChannel.Disconnect();
            serverBroadcastChannel.Disconnect();
            serverACKChannel.Disconnect();
        }
    }
}