using UnityEngine;

public class WaterController : MonoBehaviour
{
	Rigidbody2D tRigidBody;
	void Start ()
	{
		tRigidBody = gameObject.GetComponent<Rigidbody2D> ();
	}

	void Update ()
	{
		int iRnd = Random.Range (0, 100);
		if (iRnd > 95)
			tRigidBody.AddTorque (Random.Range (-tRigidBody.mass, tRigidBody.mass), ForceMode2D.Impulse);
		else if (iRnd > 70)
			tRigidBody.AddForce (new Vector2 (Random.Range (-tRigidBody.mass, tRigidBody.mass), 0), ForceMode2D.Impulse);
	}
}
