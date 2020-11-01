
using custom.Network;
using UnityEngine;

namespace custom.Utils
{
    public class Commands
    {
        public int number;
        public bool space;
        public float x, y;
        public float mouse_x;
        public float timestamp;


        public Commands(int number, float x, float y, bool space, float timestamp, float mouseX)
        {
            this.number = number;
            this.x = x;
            this.y = y;
            this.space = space;
            this.timestamp = timestamp;
            this.mouse_x = mouseX;
        }

        public Commands()
        {
        }
    

        public void Serialize(BitBuffer buffer)
        {
            buffer.PutInt(number);
            buffer.PutFloat(x);
            buffer.PutFloat(y);
            buffer.PutFloat(mouse_x);
            buffer.PutBit(space);
            
        }
    
        public void Deserialize(BitBuffer buffer)
        {
            number = buffer.GetInt();
            x = buffer.GetFloat();
            y = buffer.GetFloat();
            mouse_x = buffer.GetFloat();
            space = buffer.GetBit();
        }

        public static float generateTranslation(Commands commands)
        {
            return commands.y * Constants.speed * Time.deltaTime;
        }
        
        public static float generateStraffe(Commands commands)
        {
            return commands.x * Constants.speed * Time.deltaTime;
        }


        public bool notNull()
        {
            return x != 0f || y != 0f || mouse_x != 0f || space;
        }
    }
}