using custom.Network;
using UnityEngine;

namespace custom.Utils
{
    public class CubeEntity
    {
        private GameObject gameObject;
        private int id;
        Vector3 aux_position = new Vector3();
        Quaternion aux_rotation = new Quaternion();

        public CubeEntity(GameObject go, int id){
            this.gameObject = go;
            this.id = id;
        }

        public CubeEntity(Vector3 position, Quaternion rotation, GameObject gameObject)
        {
            this.aux_position = position;
            this.aux_rotation = rotation;
            this.gameObject = gameObject;
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
        
        }

        public void Deserialize(BitBuffer buffer) {
            aux_position = new Vector3();
            aux_rotation = new Quaternion();
            id = buffer.GetInt();
            aux_position.x = buffer.GetFloat();
            aux_position.y = buffer.GetFloat();
            aux_position.z = buffer.GetFloat();
            aux_rotation.w = buffer.GetFloat();
            aux_rotation.x = buffer.GetFloat();
            aux_rotation.y = buffer.GetFloat();
            aux_rotation.z = buffer.GetFloat();
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
            return entity;
        }

        public void applyChanges()
        {
            gameObject.transform.position = aux_position;
            gameObject.transform.rotation = aux_rotation;
        }

        public int Id => id;

        public GameObject GameObject => gameObject;
    }
}
