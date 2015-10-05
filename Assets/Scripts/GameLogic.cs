using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class GameLogic : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.T) || (CrossPlatformInputManager.ButtonExists("SlowMotion") && CrossPlatformInputManager.GetButtonDown("SlowMotion"))) {
			if (System.Math.Abs(Time.timeScale - 1) < Mathf.Epsilon) {
				Time.timeScale = 0.2f;
			} else {
				Time.timeScale = 1;
			}
		}
		if (Input.GetKeyDown(KeyCode.R) || (CrossPlatformInputManager.ButtonExists("SlowMotion") && CrossPlatformInputManager.GetButtonDown("Reset"))) {
			Application.LoadLevel("MainScene");
		}
	}
}
