using System.Runtime.CompilerServices;
using custom.Client;
using custom.Server;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace custom
{
    public class MasterBehavior : MonoBehaviour
    {
        public static class MasterData
        {
            public static Text serverTitle;
    
            private static float floorLimit = 4f;
            public static bool serverCreated = false;
            public static bool clientCreated = false;
            public static bool serverMode = false;

            public static void setServer(GameObject shadowServerGO)
            {
                SceneManager.LoadScene("Warzone");
                Instantiate(shadowServerGO,
                    new Vector3(Random.Range(-floorLimit, floorLimit), Random.Range(1f, 3f),
                        Random.Range(-floorLimit, floorLimit)),
                    new Quaternion());
                Debug.Log("Server Mode");
                    
                // serverTitle.text = "Server";
                serverMode = true;
                serverCreated = true;
            }

            public static void setClient(string address, GameObject shadowClientGO)
            {
                SceneManager.LoadScene("Warzone");
                Instantiate(shadowClientGO,
                    new Vector3(Random.Range(-floorLimit, floorLimit), Random.Range(1f, 3f),
                        Random.Range(-floorLimit, floorLimit)),
                    new Quaternion());
                Debug.Log("Client Mode");
                serverMode = false;
                clientCreated = true;
                // serverTitle.text = "SOLDIER";
            }
        }
    }
}
