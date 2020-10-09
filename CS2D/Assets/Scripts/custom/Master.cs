using custom.Client;
using custom.Server;
using UnityEngine;
using UnityEngine.UI;

namespace custom
{
    public class Master : MonoBehaviour
    {
        public GameObject shadowClientGO;
        public GameObject shadowServerGO;
        public Text serverTitle;
    
        private static float floorLimit = 4f;
        private static float minIntervalBetweenNewClients = 1;
        private static bool serverCreated = false;
        private static bool clientCreated = false;
        public static bool serverMode = false;
        
    

        private void Update()
        {
            if (!serverCreated || !clientCreated) // TODO put &&
            {
                if (Input.GetKey(KeyCode.C) && !clientCreated)
                {
                    Instantiate(shadowClientGO,
                        new Vector3(Random.Range(-floorLimit, floorLimit), Random.Range(1f, 3f),
                            Random.Range(-floorLimit, floorLimit)),
                        new Quaternion());
                    Debug.Log("Client Mode");
                    serverMode = false;
                    clientCreated = true;
                    serverTitle.text = "SOLDIER";
                }
                if (Input.GetKey(KeyCode.X) && !serverCreated)
                {
                    Instantiate(shadowServerGO,
                        new Vector3(Random.Range(-floorLimit, floorLimit), Random.Range(1f, 3f),
                            Random.Range(-floorLimit, floorLimit)),
                        new Quaternion());
                    Debug.Log("Server Mode");
                    
                    serverTitle.text = "Server";
                    serverMode = true;
                    serverCreated = true;
                }
            }
        }

        
    }
}
