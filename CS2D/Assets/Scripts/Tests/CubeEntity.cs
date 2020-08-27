using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeEntity
{
    private GameObject gameObject;
    
    public CubeEntity(GameObject go){
        this.gameObject = go;
    }

    public void Serialize(BitBuffer buffer) {
        Vector3 position = gameObject.transform.position;
        Quaternion rotation = gameObject.transform.rotation;
        buffer.PutFloat(position.x);
        buffer.PutFloat(position.y);
        buffer.PutFloat(position.z);
        buffer.PutFloat(rotation.w);
        buffer.PutFloat(rotation.x);
        buffer.PutFloat(rotation.y);
        buffer.PutFloat(rotation.z);
    }

    public void Deserialize(BitBuffer buffer) {
        Vector3 position = new Vector3();
        Quaternion rotation = new Quaternion();
        position.x = buffer.GetFloat();
        position.y = buffer.GetFloat();
        position.z = buffer.GetFloat();
        rotation.w = buffer.GetFloat();
        rotation.x = buffer.GetFloat();
        rotation.y = buffer.GetFloat();
        rotation.z = buffer.GetFloat();
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
    }
}
