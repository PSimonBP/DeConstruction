using UnityEngine;
using System.Collections;

public class DebrisController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	public void Init() {
		gameObject.layer = 8;
	}
	// Update is called once per frame
	void Update () {
		if (gameObject.GetComponent<Rigidbody2D>().IsSleeping())
			gameObject.GetComponent<BreakController>().Deactivate();
	}
}
