
using custom.Network;
using UnityEngine;

namespace custom.Utils
{
    public class Commands
    {
        public int number;
        public bool space;
        public float x, y;
        public float rotation_x;
        public float rotation_y;
        public float rotation_z;
        public float timestamp;
        
        public Commands(int number, float x, float y, bool space, float timestamp, float rotation_x, float rotation_y, float rotation_z)
        {
            this.number = number;
            this.x = x;
            this.y = y;
            this.space = space;
            this.timestamp = timestamp;
            this.rotation_x = rotation_x;
            this.rotation_y = rotation_y;
            this.rotation_z = rotation_z;
        }

        public Commands()
        {
        }
    

        public void Serialize(BitBuffer buffer)
        {
            buffer.PutInt(number);
            buffer.PutFloat(x);
            buffer.PutFloat(y);
            buffer.PutFloat(rotation_x);
            buffer.PutFloat(rotation_y);
            buffer.PutFloat(rotation_z);
            buffer.PutBit(space);
            
        }
    
        public void Deserialize(BitBuffer buffer)
        {
            number = buffer.GetInt();
            x = buffer.GetFloat();
            y = buffer.GetFloat();
            rotation_x = buffer.GetFloat();
            rotation_y = buffer.GetFloat();
            rotation_z = buffer.GetFloat();
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
            return x != 0f || y != 0f || space;
        }
    }
}