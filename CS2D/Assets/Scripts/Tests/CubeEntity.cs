using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeEntity
{
    private GameObject gameObject;
    Vector3 aux_position = new Vector3();
    Quaternion aux_rotation = new Quaternion();

    public CubeEntity(GameObject go){
        this.gameObject = go;
    }

    public void Serialize(BitBuffer buffer) {
        aux_position = gameObject.transform.position;
        aux_rotation = gameObject.transform.rotation;
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
        aux_position.x = buffer.GetFloat();
        aux_position.y = buffer.GetFloat();
        aux_position.z = buffer.GetFloat();
        aux_rotation.w = buffer.GetFloat();
        aux_rotation.x = buffer.GetFloat();
        aux_rotation.y = buffer.GetFloat();
        aux_rotation.z = buffer.GetFloat();
        gameObject.transform.position = aux_position;
        gameObject.transform.rotation = aux_rotation;
    }

    public static CubeEntity createInterpolationEntity(CubeEntity previousEntity, CubeEntity nextEntity, float time)
    {
        var entity = new CubeEntity(previousEntity.gameObject);
        entity.gameObject.transform.position = entity.gameObject.transform.position + Vector3.Lerp(
                                                   previousEntity.gameObject.transform.position, 
                                                   nextEntity.gameObject.transform.position, time);
        var rotation1 = previousEntity.gameObject.transform.rotation;
        var deltaRotation = Quaternion.Lerp(rotation1,
            nextEntity.gameObject.transform.rotation, time);
        var rotation = new Quaternion();
        rotation.x = rotation1.x + deltaRotation.x;
        rotation.w = rotation1.w + deltaRotation.w;
        rotation.y = rotation1.y + deltaRotation.y;
        rotation.z = rotation1.z + deltaRotation.z;
        entity.gameObject.transform.rotation = rotation;
        return entity;
    }

    public void applyChanges()
    {
        gameObject.transform.position = aux_position;
        gameObject.transform.rotation = aux_rotation;
    }
}
