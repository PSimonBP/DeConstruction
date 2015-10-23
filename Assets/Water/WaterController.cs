using UnityEngine;

public class WaterController : MonoBehaviour
{
//	Rigidbody2D tRigidBody;
	float Life;
	public float MaxLife { get; set; }
	public bool Split { get; set; }

//	Component m_tLight;
//	Color m_tStartColor;
//	Color m_tEndColor = new Color (1, 0.266f, 0, 1);
	void Start ()
	{
//		m_tLight = GetComponent ("Halo");
//		m_tStartColor = m_tLight.color;
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
//		float fRatio = Life / MaxLife;
//		m_tLight.color = new Color ((m_tStartColor.r * 1 - fRatio) + (m_tEndColor.r * fRatio), (m_tStartColor.g * 1 - fRatio) + (m_tEndColor.g * fRatio), (m_tStartColor.b * 1 - fRatio) + (m_tEndColor.b * fRatio), 255);
		if (Life >= MaxLife * 0.05f && Life < MaxLife * 0.95f && Split) {
			if (Random.Range (0, 100) > 95) {
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
		} else if (Life >= MaxLife) {
			WaterPool.Instance.PoolObject (gameObject);
		}
	}
}
