using UnityEngine;

public class WaterController : MonoBehaviour {
	Rigidbody2D tRigidBody;
	void Start () {
		tRigidBody = gameObject.GetComponent<Rigidbody2D>();
	}

	void Update () {
		if (Random.Range(0, 100) > 50) {
			tRigidBody.AddForce(new Vector2(Random.Range(-tRigidBody.mass, tRigidBody.mass), 0), ForceMode2D.Impulse);
//			tRigidBody.AddTorque(Random.Range(-tRigidBody.mass, tRigidBody.mass), ForceMode2D.Impulse);
		}
	}
}
