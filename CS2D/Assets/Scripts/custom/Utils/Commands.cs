using lib.Network;

namespace custom.Utils
{
    public class Commands
    {
        public int number;
        public bool up;
        public bool down;
        public bool space;
        public float timestamp;

        public Commands(int number, bool up, bool down, bool space)
        {
            this.number = number;
            this.up = up;
            this.down = down;
            this.space = space;
        }
    
        public Commands(int number, bool up, bool down, bool space, float timestamp)
        {
            this.number = number;
            this.up = up;
            this.down = down;
            this.space = space;
            this.timestamp = timestamp;
        }

        public Commands()
        {
        }
    

        public void Serialize(BitBuffer buffer)
        {
            buffer.PutInt(number);
            buffer.PutBit(up);
            buffer.PutBit(down);
            buffer.PutBit(space);
        }
    
        public void Deserialize(BitBuffer buffer)
        {
        
            number = buffer.GetInt();
            up = buffer.GetBit();
            down = buffer.GetBit();
            space = buffer.GetBit();
        }
    }
}