using UnityEngine;

public class WeaponController : MonoBehaviour
{
/*	public enum EWeaponType
	{
		WT_BULLET,
		WT_ROCKET,
		WT_LASER,
		WT_FLAME
	}
*/
	public float		RateOfFire = 1;
	public int			Strength = 1;
	public int          Precision = 20;
//	public EWeaponType	WeaponType = EWeaponType.WT_BULLET;

	UnitController		m_tController;
	BodyController		m_tBody;
	float				m_fTimeToReady;
	float				m_fEffectTime = float.MaxValue;

	Renderer			m_tFireEffect1;
	Renderer			m_tFireEffect2;

	void Start()
	{
		m_tController = gameObject.GetComponentInParent<UnitController>();
		m_tBody = gameObject.GetComponentInParent<BodyController>();
		Renderer[] tFireEffects = gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer tEffect in tFireEffects) {
			if (tEffect.name == "Fire 1")
				m_tFireEffect1 = tEffect;
			else if (tEffect.name == "Fire 2")
				m_tFireEffect2 = tEffect;
		}
	}
	
	void Update()
	{
		if (m_tController.IsAttacking()) {
			if (m_fTimeToReady <= 0) {
				for (int i=0; i<Strength; ++i) {
					var tDrop = FirePool.GetFire();
					if (tDrop != null) {
						tDrop.MaxLife = Random.Range(0.8f, 1.2f);

						Vector3 tPosition = transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
						tPosition.z = 0;
						tDrop.transform.position = tPosition;
						tDrop.transform.SetParent(FirePool.Instance.transform);
						tDrop.Init();
						float fAngle = Random.Range(-Precision, Precision);
						Quaternion tNewRot = Quaternion.AngleAxis(fAngle, Vector3.up);
						tDrop.transform.rotation = Quaternion.RotateTowards(m_tBody.transform.rotation, tNewRot, fAngle);
						tDrop.RigidBody.AddRelativeForce(new Vector2(0, 1000 * tDrop.RigidBody.mass));
					}
				}

/*				var tBullet = BulletPool.Instance.GetObject();
				BulletController tBC = tBullet.GetComponent<BulletController>();
				tBC.Init(m_tController.gameObject, transform.position, m_tBody.transform.rotation, Strength, 1.0f / RateOfFire);
*/
				m_fTimeToReady = RateOfFire;
				m_fEffectTime = 0;
			}
		}

		if (m_fTimeToReady > 0)
			m_fTimeToReady -= Time.deltaTime;
		float fEffectPartTime = 0.02f;
		if (m_fEffectTime <= fEffectPartTime * 5) {
			if (m_fEffectTime > fEffectPartTime && m_fEffectTime < fEffectPartTime * 4) {
				m_tFireEffect1.GetComponent<Renderer>().enabled = (m_fEffectTime > fEffectPartTime * 2 && m_fEffectTime < fEffectPartTime * 3);
				m_tFireEffect2.GetComponent<Renderer>().enabled = true;
			} else {
				m_tFireEffect1.GetComponent<Renderer>().enabled = true;
				m_tFireEffect2.GetComponent<Renderer>().enabled = false;
			}
			m_fEffectTime += Time.deltaTime;
		} else {
			m_tFireEffect1.GetComponent<Renderer>().enabled = false;
			m_tFireEffect2.GetComponent<Renderer>().enabled = false;
		}
	}
}
