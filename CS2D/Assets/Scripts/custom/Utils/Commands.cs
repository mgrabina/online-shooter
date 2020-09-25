
using custom.Network;

namespace custom.Utils
{
    public class Commands
    {
        public int number;
        public bool up, down, left, right, space;
        public float timestamp;


        public Commands(int number, bool up, bool down, bool left, bool right, bool space, float timestamp)
        {
            this.number = number;
            this.up = up;
            this.down = down;
            this.left = left;
            this.right = right;
            this.space = space;
            this.timestamp = timestamp;
        }

        public Commands(int number, bool up, bool down, bool left, bool right, bool space)
        {
            this.number = number;
            this.up = up;
            this.down = down;
            this.left = left;
            this.right = right;
            this.space = space;
        }
        
        public Commands()
        {
        }
    

        public void Serialize(BitBuffer buffer)
        {
            buffer.PutInt(number);
            buffer.PutBit(up);
            buffer.PutBit(down);
            buffer.PutBit(left);
            buffer.PutBit(right);
            buffer.PutBit(space);
            
        }
    
        public void Deserialize(BitBuffer buffer)
        {
            number = buffer.GetInt();
            up = buffer.GetBit();
            down = buffer.GetBit();
            left = buffer.GetBit();
            right = buffer.GetBit();
            space = buffer.GetBit();
        }
    }
}