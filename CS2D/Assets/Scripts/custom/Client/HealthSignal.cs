using System.Collections;
using System.Collections.Generic;
using custom.Utils;
using UnityEngine;

public class HealthSignal : MonoBehaviour
{
    public CubeEntity CubeEntity;
    private MeshRenderer mesh;
    public Material black, red, yellow, green;

    void Start()
    {
        mesh = this.gameObject.GetComponent<MeshRenderer>();
    }

    void Update()
    {
        float lastHealth = CubeEntity.Health;
        Debug.Log(CubeEntity.Id + " " + CubeEntity.Health);
        if (lastHealth < 0.2f)
        {
            mesh.material = black;
        } 
        else if (lastHealth < 0.4f)
        {
            mesh.material = red;
        }
        else if (lastHealth < 0.7f)
        {
            mesh.material = yellow;
        }
        else
        {
            mesh.material = green;
        }
    }
}
