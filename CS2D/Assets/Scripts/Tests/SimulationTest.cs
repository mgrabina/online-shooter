using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class SimulationTest : MonoBehaviour
{

    private Channel channel;
    private bool online = true;

    [SerializeField] private GameObject serverGameObject;
    [SerializeField] private GameObject clientGameObject;
    
    private CubeEntity serverCubeEntity;
    private CubeEntity clientCubeEntity;
    
    // Start is called before the first frame update
    void Start() {
        channel = new Channel(9000);
        serverCubeEntity = new CubeEntity(serverGameObject);
        clientCubeEntity = new CubeEntity(clientGameObject);
    }

    private void OnDestroy() {
        channel.Disconnect();
    }

    // Update is called once per frame
    void Update() {
        //apply input
        if (Input.GetKeyDown(KeyCode.Space)) {
            serverGameObject.GetComponent<Rigidbody>().AddForceAtPosition(Vector3.up * 5, Vector3.zero, ForceMode.Impulse);
        }
        
        if (Input.GetKeyDown(KeyCode.D)) {
            online = !online;
        }

        if (online)
        {
            UpdateServer();
        }
        
        UpdateClient();
    }

    private void UpdateServer()
    {
        //serialize
        var packet = Packet.Obtain();
        serverCubeEntity.Serialize(packet.buffer);
        packet.buffer.Flush();

        string serverIP = "127.0.0.1";
        int port = 9000;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel.Send(packet, remoteEp);

        packet.Free();   
    }
    
    private void UpdateClient() {
        var packet = channel.GetPacket();

        if (packet == null) {
            // Interpolation
            return;
        }

        var buffer = packet.buffer;

        //deserialize
        clientCubeEntity.Deserialize(buffer);
    }
}
