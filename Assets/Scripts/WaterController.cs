using UnityEngine;
using System.Collections;

public class WaterController : MonoBehaviour {
	private Rigidbody2D tRigidBody;
	// Use this for initialization
	void Start () {
		tRigidBody = gameObject.GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update () {
		if (Random.Range(0, 100) > 90)
//			tRigidBody.AddForce(new Vector2(Random.Range(-0.1f, 0.1f), 0), ForceMode2D.Impulse);
			tRigidBody.AddTorque(Random.Range(-0.1f, 0.1f), ForceMode2D.Impulse);
	}
}
