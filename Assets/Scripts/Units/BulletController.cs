using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour {
	public float		LifeTime = 0.5f;
	float				m_fActiveTime = 0;
	GameObject			m_tParent = null;
	GameObject			m_tTarget = null;
	Vector2				m_tTargetHitPont = Vector2.zero;
	float				m_fDamage = 0;
	bool				m_bCanRayCast = false;
	bool				m_bRayCast = false;

	void FixedUpdate() {
		m_fActiveTime += Time.fixedDeltaTime;
		if (m_fActiveTime > LifeTime) {
			if (m_tTarget != null && m_tTarget.activeSelf) {
				UnitController tTarget = m_tTarget.GetComponent<UnitController>();
				if (tTarget)
					tTarget.Damage(m_fDamage, m_tTargetHitPont, m_tParent);
			}
			Deactivate();
		}

		if (m_bCanRayCast) {
			if (enabled && m_fActiveTime > 0 && !m_bRayCast) {
				RaycastHit2D[] tHits = Physics2D.RaycastAll(transform.position, GetComponent<Rigidbody2D>().velocity.normalized, Mathf.Abs(GetComponent<Rigidbody2D>().velocity.magnitude * (LifeTime - m_fActiveTime))); // todo dest
				foreach (RaycastHit2D tHit in tHits) {
					if (tHit.transform.gameObject.GetComponent<BulletController>() == null && tHit.transform.gameObject != m_tParent) {
						m_tTarget = tHit.transform.gameObject;
						m_fActiveTime = LifeTime - (Mathf.Abs((transform.position - tHit.transform.position).magnitude) / Mathf.Abs(GetComponent<Rigidbody2D>().velocity.magnitude * (LifeTime - m_fActiveTime)) * 0.8f);
						m_tTargetHitPont = tHit.point;
						break;
					}
				}
				m_bRayCast = true;
			}
		} else {
			if (enabled)
				m_bCanRayCast = true;
		}
	}

	public GameObject GetParent() { return m_tParent; }

	public void Init(GameObject tParent, Vector3 tPos, Quaternion tRot, float fDamage, float fPrecision) {
		m_tTarget = null;
		m_bCanRayCast = false;
		m_bRayCast = false;
		m_fDamage = fDamage;
		m_fActiveTime = 0;
		m_tParent = tParent;
		transform.position = tPos;

		float fAngle = Random.Range(-fPrecision, fPrecision);
		Quaternion tNewRot = Quaternion.AngleAxis(fAngle, Vector3.up);
		transform.rotation = Quaternion.RotateTowards(tRot, tNewRot, fAngle);
		GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0, 1000));
	}

	public void Deactivate() {
		BulletPool.Instance.PoolObject(gameObject);
	}
}
