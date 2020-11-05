
using custom.Network;
using UnityEngine;

namespace custom.Utils
{
    public class Commands
    {
        public int number;
        public bool space;
        public float x, y;
        public float rotation;
        public float timestamp;

        private float lastRotation = 0f;

        public Commands(int number, float x, float y, bool space, float timestamp, float rotation)
        {
            this.number = number;
            this.x = x;
            this.y = y;
            this.space = space;
            this.timestamp = timestamp;
            this.rotation = rotation;
        }

        public Commands()
        {
        }
    

        public void Serialize(BitBuffer buffer)
        {
            buffer.PutInt(number);
            buffer.PutFloat(x);
            buffer.PutFloat(y);
            buffer.PutFloat(rotation);
            buffer.PutBit(space);
            
        }
    
        public void Deserialize(BitBuffer buffer)
        {
            number = buffer.GetInt();
            x = buffer.GetFloat();
            y = buffer.GetFloat();
            rotation = buffer.GetFloat();
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
            bool changed_rotation = rotation.Equals(lastRotation);
            lastRotation = rotation;
            return x != 0f || y != 0f || changed_rotation || space;
        }
    }
}