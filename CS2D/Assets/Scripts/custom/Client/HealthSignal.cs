using System.Collections;
using System.Collections.Generic;
using custom.Utils;
using UnityEngine;

public class HealthSignal : MonoBehaviour
{
    public CubeEntity CubeEntity;
    private Color colorPicker;
    
    // Start is called before the first frame update
    void Start()
    {
        colorPicker = this.gameObject.GetComponent<MeshRenderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (CubeEntity.Health < 0.2)
        {
            colorPicker = Color.black;
        } 
        else if (CubeEntity.Health < 0.4)
        {
            colorPicker = Color.red;
        }
        else if (CubeEntity.Health < 0.7)
        {
            colorPicker = Color.yellow;
        }
        else
        {
            colorPicker = Color.green;
        }
    }
}
