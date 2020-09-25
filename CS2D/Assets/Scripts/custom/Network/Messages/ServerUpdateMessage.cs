using custom.Utils;

namespace custom.Network
{
    public class ServerUpdateMessage : Message
    {
        public ServerUpdateMessage(int id = 0, Channel channel = null, string destinationIp = null, int destinationPort = 0) : base(Type.GAME_STATE_UPDATE, id, channel, destinationIp, destinationPort)
        {
        }

        public ServerUpdateMessage(Type type, int id, Packet packet) : base(type, id, packet)
        {
        }

        public ServerUpdateMessage setArguments(Snapshot snapshot)
        {
            snapshot.Serialize(packet.buffer);
            return this;
        }
    }
}