using UnityEngine;

public class WaterController : MonoBehaviour
{
//	Rigidbody2D tRigidBody;
	float Life;
	public float MaxLife { get; set; }
	public bool Split { get; set; }
	void Start ()
	{
	}

	void OnEnable ()
	{
		Split = true;
		Life = 0;
		MaxLife = WaterPool.Instance.LifeTime * (Random.Range (0.8f, 1.2f));
	}

	void Update ()
	{
		Life += Time.deltaTime;
		if (Life >= MaxLife * 0.25f && Life < MaxLife * 0.75f && Split) {
			if (Random.Range (0, 100) > 95) {
				for (int i=0; i<3; ++i) {
					var tObj = WaterPool.Instance.GetObject ();
					if (tObj) {
						var tNewObj = tObj.GetComponent<WaterController> ();
						tNewObj.transform.SetParent (transform.parent);
						tNewObj.transform.localPosition = transform.localPosition;
						tNewObj.transform.localRotation = transform.localRotation;
						Vector2 tVel = GetComponent<Rigidbody2D> ().velocity;
						tVel.x *= Random.Range (0.8f, 1.2f);
						tVel.y *= Random.Range (0.8f, 1.2f);
						tNewObj.GetComponent<Rigidbody2D> ().velocity = tVel;
						tNewObj.MaxLife = (MaxLife - Life) * Random.Range (0.8f, 1.2f);
						tNewObj.Split = false;
					}
				}
			}
		} else if (Life >= MaxLife) {
			WaterPool.Instance.PoolObject (gameObject);
		}
	}
}
