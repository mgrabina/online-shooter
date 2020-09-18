using custom.Client;
using custom.Server;
using UnityEngine;
using Random = UnityEngine.Random;

namespace custom
{
    public class Master : MonoBehaviour
    {
        public GameObject serverGameObject;
        public GameObject clientGameObject;
        private ServerMessenger server;
    
        private static float floorLimit = 4f;
        private static float minIntervalBetweenNewClients = 1;

        private float accumulatedTimeWithoutCreatingNewClients = 0;
    
        private void Start()
        {
            Instantiate(serverGameObject, new Vector3(0, 0.5f, 0), new Quaternion());
        }

        private void Update()
        {
            accumulatedTimeWithoutCreatingNewClients += Time.deltaTime;

            if (accumulatedTimeWithoutCreatingNewClients > minIntervalBetweenNewClients)
            {
                if (Input.GetKey(KeyCode.C))
                {
                    // Create new Client placed randomly
                    Instantiate(clientGameObject,
                        new Vector3(Random.Range(-floorLimit, floorLimit), Random.Range(1f, 3f),
                            Random.Range(-floorLimit, floorLimit)),
                        new Quaternion()).GetComponent<ClientMessenger>().id = generate_id();
                
                    accumulatedTimeWithoutCreatingNewClients = 0;
                }
            }
        }

        private int generate_id()
        {
            return Random.Range(0, int.MaxValue);
        }
    }
}
