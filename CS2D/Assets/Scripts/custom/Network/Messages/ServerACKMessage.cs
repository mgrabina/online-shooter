namespace custom.Network
{
    public class ServerACKMessage : Message
    {
        private int number;
        
        public ServerACKMessage(int id = 0, Channel channel = null, string destinationIp = null, int destinationPort = 0) : base(Type.CLIENT_UPDATE_ACK, id, channel, destinationIp, destinationPort)
        {
        }

        public ServerACKMessage(Type type, int id, Packet packet) : base(type, id, packet)
        {
        }

        public ServerACKMessage setArguments(int number)
        {
            this.number = number;
            packet.buffer.PutInt(number);
            return this;
        }

        public int getNumber()
        {
            number = packet.buffer.GetInt();
            return number;
        }
    }
}