namespace custom.Network
{
    public class GoodbyeMessage : Message
    {
        public GoodbyeMessage(int id = 0, Channel channel = null, string destinationIp = null, int destinationPort = 0) : base(Type.GOODBYE, id, channel, destinationIp, destinationPort)
        {
        }

        public GoodbyeMessage(Type type, int id, Packet packet) : base(Type.GOODBYE, id, packet)
        {
        }
    }
}