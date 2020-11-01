using System.Runtime.CompilerServices;
using custom.Client;
using custom.Server;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// 192.168.68.104


namespace custom
{
    public class MasterBehavior : MonoBehaviour
    {
        public static class MasterData
        {
            public static string ip = "";
            private static float floorLimit = 15f;

            private static GameObject client;
            private static GameObject server;
                
            public static bool serverMode = false;
                
            public static void setServer(GameObject shadowServerGO)
            {
                serverMode = true;
                server = shadowServerGO;
                SceneManager.LoadScene("Warzone");
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneWasLoadedServer;
                
            }

            public static void setClient(string address, GameObject shadowClientGO)
            {
                serverMode = false;
                client = shadowClientGO;
                ip = address;
                SceneManager.LoadScene("Warzone");
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneWasLoadedClient;
                // serverTitle.text = "SOLDIER";
            }
            
            public static void OnSceneWasLoadedServer (UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to) {
                Instantiate(server,
                    new Vector3(Random.Range(-floorLimit, floorLimit), Random.Range(1f, 3f),
                        Random.Range(-floorLimit, floorLimit)),
                    new Quaternion());
                Debug.Log("Server Mode");
            }
            
            public static void OnSceneWasLoadedClient (UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to) {
                Instantiate(client,
                    new Vector3(Random.Range(-floorLimit, floorLimit), Random.Range(1f, 3f),
                        Random.Range(-floorLimit, floorLimit)),
                    new Quaternion());
                Destroy(GameObject.Find("ServerCamera"));
                Debug.Log("Client Mode");
            }
        }

        
    }
}
