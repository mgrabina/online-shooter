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
            public static string ip = "";
            private static float floorLimit = 4f;
            public static bool serverCreated = false;
            public static bool clientCreated = false;
            public static bool serverMode = false;

            private static GameObject client;
            private static GameObject server;
                
            public static void setServer(GameObject shadowServerGO)
            {
                server = shadowServerGO;
                SceneManager.LoadScene("Warzone");
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneWasLoadedServer;
            }

            public static void setClient(string address, GameObject shadowClientGO)
            {
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
                    
                // serverTitle.text = "Server";
                serverMode = true;
                serverCreated = true;
            }
            
            public static void OnSceneWasLoadedClient (UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to) {
                Instantiate(client,
                    new Vector3(Random.Range(-floorLimit, floorLimit), Random.Range(1f, 3f),
                        Random.Range(-floorLimit, floorLimit)),
                    new Quaternion());
                Debug.Log("Client Mode");
                serverMode = false;
                clientCreated = true;
            }
        }

        
    }
}
