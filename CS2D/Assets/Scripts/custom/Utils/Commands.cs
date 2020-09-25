
using custom.Network;
using UnityEngine;

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

        public static Vector3 generateForce(Commands commands)
        {
            Vector3 force = Vector3.zero;
            force += commands.space ? Vector3.up * 5 : Vector3.zero;
            force += commands.up ? Vector3.forward * 2 : Vector3.zero;
            force += commands.down ? Vector3.back * 2 : Vector3.zero;
            force += commands.left ? Vector3.left * 2 : Vector3.zero;
            force += commands.right ? Vector3.right * 2 : Vector3.zero;
            return force;
        }

        public bool notNull()
        {
            return down || up || right || left || space;
        }
    }
}