using System.Collections;
using System.Collections.Generic;
using custom;
using custom.Client;
using custom.Server;
using custom.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Latency : MonoBehaviour
{
    private TextMesh mesh;
    public Messenger cm;
    public int id;
    
    void Start()
    {
        mesh = this.gameObject.GetComponent<TextMesh>();
    }

    void Update()
    {
        if (cm == null)
        {
            return;
        }

        float latency = cm.getLatency();
        mesh.text = "LAT " + latency;
    }
}
