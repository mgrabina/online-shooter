using custom.Utils;

namespace custom.Network
{
    public class InitStatusMessage : Message
    {
        public InitStatusMessage(int id = 0, Channel channel = null, string destinationIp = null, int destinationPort = 0) : base(Type.INIT_STATUS, id, channel, destinationIp, destinationPort)
        {
        }

        public InitStatusMessage(Type type, int id, Packet packet) : base(type, id, packet)
        {
        }

        public InitStatusMessage setArguments(Snapshot snapshot)
        {
            snapshot.Serialize(packet.buffer);
            return this;
        }
    }
}