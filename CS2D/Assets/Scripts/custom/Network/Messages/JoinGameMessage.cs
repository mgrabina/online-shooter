namespace custom.Network
{
    public class JoinGameMessage : Message
    {
        public JoinGameMessage(int id = 0, Channel channel = null, string destinationIp = null, int destinationPort = 0) : base(Type.JOIN_GAME, id, channel, destinationIp, destinationPort)
        {
        }

        public JoinGameMessage(Type type, int id, Packet packet) : base(Type.JOIN_GAME, id, packet)
        {
        }
    }
}