namespace custom.Network
{
    public class PlayerJoinedMessage : Message
    {
        private int idJoined;
        
        public PlayerJoinedMessage(int id = 0, Channel channel = null, string destinationIp = null, int destinationPort = 0) : base(Type.PLAYER_JOINED, id, channel, destinationIp, destinationPort)
        {
        }

        public PlayerJoinedMessage(Type type, int id, Packet packet) : base(type, id, packet)
        {
        }

        public PlayerJoinedMessage setArguments(int id)
        {
            idJoined = id;
            packet.buffer.PutInt(id);
            return this;
        }

        public int IdJoined()
        {
            idJoined = packet.buffer.GetInt();
            return idJoined;
        }
        
    }
}