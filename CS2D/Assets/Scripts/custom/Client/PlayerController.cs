using lib;
using UnityEngine;

namespace custom.Client
{
	public class PlayerController : MonoBehaviour {

		PlayerInput playerInput = new PlayerInput();

		// Use this for initialization
		void Start () {
		
		}
	
		// Update is called once per frame
		void Update () {
			playerInput.x = UnityEngine.Input.GetAxis("Horizontal");
			playerInput.y = UnityEngine.Input.GetAxis("Vertical");
			playerInput.shoot = UnityEngine.Input.GetKey (KeyCode.Space);
		}

		public PlayerInput Input {
			get {
				return playerInput;
			}
		}
	}
}
