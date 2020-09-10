namespace Tests
{
    public class Snapshot
    {
        private CubeEntity entity;
        private int packetNumber;

        public Snapshot(int packetNumber, CubeEntity entity)
        {
            this.entity = entity;
            this.packetNumber = packetNumber;
        }

        public void Serialize(BitBuffer buffer)
        {
            buffer.PutInt(packetNumber);
            entity.Serialize(buffer);
        }

        public void Deserialize(BitBuffer buffer)
        {
            packetNumber = buffer.GetInt();
            entity.Deserialize(buffer);
        }

        public static Snapshot createInterpolationSnapshot(Snapshot previous, Snapshot next, float time)
        {
            var newEntity = CubeEntity.createInterpolationEntity(previous.entity, next.entity, time);
            return new Snapshot(-1, newEntity);
        }

        public int GetPacketNumber()
        {
            return packetNumber;
        }

        public void applyChanges()
        {    
            this.entity.applyChanges();
        }
    }
}