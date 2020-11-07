namespace custom.Network
{
    public class HitEnemyMessage : Message
    {
        private int fromId;

        public int FromId => fromId;

        public int ToId => toId;

        private int toId;
        
        public HitEnemyMessage(int fromId, int toId, Channel channel = null, string destinationIp = null, int destinationPort = 0) : base(Type.HIT_ENEMY_MESSAGE, fromId, channel, destinationIp, destinationPort)
        {
            packet.buffer.PutInt(fromId);
            packet.buffer.PutInt(toId);

            this.fromId = fromId;
            this.toId = toId;
        }

        public HitEnemyMessage(Type type, int id, Packet packet) : base(Type.HIT_ENEMY_MESSAGE, id, packet)
        {
            this.fromId = packet.buffer.GetInt();
            this.toId = packet.buffer.GetInt();
        }
    }
}