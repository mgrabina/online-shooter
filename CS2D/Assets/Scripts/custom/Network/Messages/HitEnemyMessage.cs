namespace custom.Network
{
    public class HitEnemyMessage : Message
    {
        public HitEnemyMessage(int id = 0, Channel channel = null, string destinationIp = null, int destinationPort = 0) : base(Type.HIT_ENEMY_MESSAGE, id, channel, destinationIp, destinationPort)
        {
        }

        public HitEnemyMessage(Type type, int id, Packet packet) : base(Type.JOIN_GAME, id, packet)
        {
        }
    }
}