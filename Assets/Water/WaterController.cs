using UnityEngine;

public class WaterController : MonoBehaviour
{
	Rigidbody2D tRigidBody;
	float Life;
	float MaxLife;
	void Start ()
	{
		tRigidBody = gameObject.GetComponent<Rigidbody2D> ();
	}

	void OnEnable ()
	{
		Life = 0;
		MaxLife = WaterPool.Instance.LifeTime * (Random.Range (0.8f, 1.2f));
	}

	void Update ()
	{
/*		int iRnd = Random.Range(0, 100);
		if (iRnd > 95)
			tRigidBody.AddTorque(Random.Range(-tRigidBody.mass, tRigidBody.mass), ForceMode2D.Impulse);
		else if (iRnd > 70)
			tRigidBody.AddForce(new Vector2(Random.Range(-tRigidBody.mass, tRigidBody.mass), 0), ForceMode2D.Impulse);
*/
		Life += Time.deltaTime;
		if (Life >= MaxLife) {
/*			BreakableBox tBox = BoxPool.GetBox();
			BreakableContainer tCont = ContainerPool.GetContainer();
			if (tBox != null) {
				tCont.FractureSize = 0.2f;
				tCont.FractureForce = 20;
				tCont.Density = 10;
				tBox.transform.position = transform.position;
				tBox.transform.localScale = transform.localScale / 1.8f;
				tBox.Init(tCont);
			} else if (tCont != null) {
				ContainerPool.Instance.PoolObject(tCont.gameObject);
			}
*/
			WaterPool.Instance.PoolObject (gameObject);
		}
	}
}
