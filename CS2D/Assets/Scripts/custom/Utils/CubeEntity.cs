using System;
using System.Collections.Generic;
using custom.Client;
using custom.Network;
using UnityEngine;

namespace custom.Utils
{
    public class CubeEntity
    {
        private GameObject gameObject;
        private int id = -1;
        Vector3 aux_position = new Vector3();
        Quaternion aux_rotation = new Quaternion();
        private int lastCommandProcessed = -1, aux_lastCommandProcessed = -1, aux_id = -1;
        
        public CubeEntity(GameObject go, int id){
            this.gameObject = go;
            this.id = id;
        }

        public void Serialize(BitBuffer buffer) {
            aux_position = gameObject.transform.position;
            aux_rotation = gameObject.transform.rotation;
            buffer.PutInt(id);
            buffer.PutFloat(aux_position.x);
            buffer.PutFloat(aux_position.y);
            buffer.PutFloat(aux_position.z);
            buffer.PutFloat(aux_rotation.w);
            buffer.PutFloat(aux_rotation.x);
            buffer.PutFloat(aux_rotation.y);
            buffer.PutFloat(aux_rotation.z);
            buffer.PutInt(lastCommandProcessed);
        }

        public void Deserialize(BitBuffer buffer) {
            aux_position = new Vector3();
            aux_rotation = new Quaternion();
            aux_id = buffer.GetInt();
            aux_position.x = buffer.GetFloat();
            aux_position.y = buffer.GetFloat();
            aux_position.z = buffer.GetFloat();
            aux_rotation.w = buffer.GetFloat();
            aux_rotation.x = buffer.GetFloat();
            aux_rotation.y = buffer.GetFloat();
            aux_rotation.z = buffer.GetFloat();
            aux_lastCommandProcessed = buffer.GetInt();
        }
        
        public void DeserializeSpecific(BitBuffer buffer, List<CubeEntity> entities, ClientMessenger cm) {
            Deserialize(buffer);
            CubeEntity finded = null;
            foreach(var c in entities)
            {
                if (c.id == aux_id)
                {
                    finded = c;
                    break;
                }   
            }

            if (finded == null)
            {
                this.gameObject = cm.createClient(aux_id);
                this.id = aux_id;
            }
            else
            {
                this.gameObject = finded.gameObject;
                this.id = finded.id;
            }

        }

        public static CubeEntity createInterpolationEntity(CubeEntity previousEntity, CubeEntity nextEntity, float time)
        {
            var entity = new CubeEntity(previousEntity.gameObject, previousEntity.id);
            entity.aux_position = entity.aux_position + Vector3.Lerp(
                                                       previousEntity.aux_position, 
                                                       nextEntity.aux_position, time);
            var rotation1 = previousEntity.aux_rotation;
            var deltaRotation = Quaternion.Lerp(rotation1,
                nextEntity.aux_rotation, time);
            var rotation = new Quaternion();
            rotation.x = rotation1.x + deltaRotation.x;
            rotation.w = rotation1.w + deltaRotation.w;
            rotation.y = rotation1.y + deltaRotation.y;
            rotation.z = rotation1.z + deltaRotation.z;
            entity.aux_rotation = rotation;
            entity.aux_lastCommandProcessed = nextEntity.aux_lastCommandProcessed;
            return entity;
        }

        public void applyChanges()
        {
            if (aux_id != id && id != -1 && aux_id != -1)
            {
                Debug.Log("This should not happen.");
            }
            gameObject.transform.position = aux_position;
            gameObject.transform.rotation = aux_rotation;
            lastCommandProcessed = aux_lastCommandProcessed;
        }

        public int Id => id;

        public GameObject GameObject => gameObject;

        public Vector3 AuxPosition => aux_position;

        public Quaternion AuxRotation => aux_rotation;

        public int AuxLastCommandProcessed => aux_lastCommandProcessed;

        public int LastCommandProcessed
        {
            get { return lastCommandProcessed; }
            set { lastCommandProcessed = value; }
        }
    }
}
