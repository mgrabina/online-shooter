using custom.Client;
using custom.Server;
using UnityEngine;

namespace custom
{
    public class Master : MonoBehaviour
    {
        public GameObject shadowClientGO;
    
        private static float floorLimit = 4f;
        private static float minIntervalBetweenNewClients = 1;

        private float accumulatedTimeWithoutCreatingNewClients = 0;
    
        private void Start()
        {
            
        }

        private void Update()
        {
            accumulatedTimeWithoutCreatingNewClients += Time.deltaTime;

            if (accumulatedTimeWithoutCreatingNewClients > minIntervalBetweenNewClients)
            {
                if (Input.GetKey(KeyCode.C))
                {
                    Instantiate(shadowClientGO,
                        new Vector3(Random.Range(-floorLimit, floorLimit), Random.Range(1f, 3f),
                            Random.Range(-floorLimit, floorLimit)),
                        new Quaternion());
                    
                    accumulatedTimeWithoutCreatingNewClients = 0;
                }
            }
        }

        
    }
}
