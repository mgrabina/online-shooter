using System.Collections;
using System.Collections.Generic;
using custom;
using custom.Client;
using custom.Server;
using custom.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthSignal2 : MonoBehaviour
{
    private TextMesh mesh;
    public Material black, red, yellow, green;
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
        float lastHealth = cm.getCurrentHealth(id);
        if (lastHealth < 0.2f)
        {
            mesh.text = "(" + (int)(lastHealth*100) + ")";
            mesh.color = Color.black;
        } 
        else if (lastHealth < 0.4f)
        {
            mesh.text = "* (" + (int)(lastHealth*100) + ") *";
            mesh.color = Color.red;
        }
        else if (lastHealth < 0.7f)
        {
            mesh.text = "* * (" + (int)(lastHealth*100) + ") * *";
            mesh.color = Color.yellow;
        }
        else
        {
            mesh.text = "* * * (" + (int)(lastHealth*100) + ") * * *";
            mesh.color = Color.green;
        }
    }
}
