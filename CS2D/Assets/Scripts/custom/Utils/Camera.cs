using UnityEngine;

namespace custom.Utils
{
    public class Camera : MonoBehaviour
    {
        [SerializeField]
        public float sensitivity = 5.0f;
        [SerializeField]
        public float smoothing = 2.0f;
        // the chacter is the capsule
        public GameObject character;
        // get the incremental value of mouse moving
        private Vector2 mouseLook;
        // smooth the mouse moving
        private Vector2 smoothV;

        public GameObject main;

        private bool following = true;
        private bool camera_cheked = false;
        
        
        // Use this for initialization
        void Start () {
            character = this.transform.parent.gameObject;
            this.GetComponent<UnityEngine.Camera>().enabled = false;
        }
	
        // Update is called once per frame
        void Update () {
            if (this.tag.Equals("MasterClient") && !camera_cheked)
            {
                // this.GetComponent<UnityEngine.Camera>().enabled = true;
                this.GetComponent<UnityEngine.Camera>().enabled = false;
            }
            else if (!camera_cheked)
            {
                this.GetComponent<UnityEngine.Camera>().enabled = false;
            }

            if (!camera_cheked && !this.tag.Equals("Untagged"))
            {
                camera_cheked = true;
            }
            
            if (Input.GetKey(KeyCode.F))
            {
                following = !following;
            }

            if (following)
            {
                // md is mosue delta
                var md = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                md = Vector2.Scale(md, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
                // the interpolated float result between the two float values
                smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / smoothing);
                smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / smoothing);
                // incrementally add to the camera look
                mouseLook += smoothV;

                // vector3.right means the x-axis
                transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
                character.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, character.transform.up);   
            }
        }
    }
}