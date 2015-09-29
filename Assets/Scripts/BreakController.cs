using UnityEngine;
using System.Collections;

public class BreakController : MonoBehaviour
{
	public float	FractureSize = 0.1f;
	public float	FractureForce = 1.0f;
	public float	DelayStart = 10;
	public Vector3	InitialVelocity = Vector3.zero;
	public float InitialAngularVelocity = 0;
	float fDelayStart = float.MaxValue;
	protected Vector3 m_tVelocity;
	bool bInitialized = false;
	protected Rigidbody2D	tRigidBody = null;
	protected BoxCollider2D	tCollider = null;

	void Start()
	{
		if (!bInitialized) {
			Init();
			if (InitialVelocity != Vector3.zero || InitialAngularVelocity != 0)
				fDelayStart = DelayStart;
		}
	}

	void Init()
	{
		gameObject.layer = 0;
		tRigidBody = gameObject.GetComponent<Rigidbody2D>();
		tCollider = gameObject.GetComponent<BoxCollider2D>();
		bInitialized = true;
	}

	public bool Break(Collider2D tCol = null)
	{
		int iNeededObjects = 1;
		float iBreakX = 1;
		float iBreakY = 1;
		if (CanBreakX()) {
			iBreakX = 2;
			if (transform.localScale.x * 2 > transform.localScale.y)	iBreakX = (int)Mathf.Max(iBreakX, transform.localScale.y / transform.localScale.x);
		}
		if (CanBreakY()) {
			iBreakY = 2;
			if (transform.localScale.y * 2 > transform.localScale.x)	iBreakY = (int)Mathf.Max(iBreakY, transform.localScale.x / transform.localScale.y);
		}

		iBreakX = (int)iBreakX;	iBreakY = (int)iBreakY;
		iNeededObjects *= (int)iBreakX;	iNeededObjects *= (int)iBreakY;	iNeededObjects -= 1;

		if (iNeededObjects <= BoxPool.Instance.GetFreePoolSize()) {
			for (float x = 0; x < iBreakX; ++x)
				for (float y = 0; y < iBreakY; ++y) {
						if (x == 0 && y == 0) {
							Vector3 tS = new Vector3(transform.localScale.x / iBreakX, transform.localScale.y / iBreakY, 0);
							Vector3 tPosChange = tS / 2;
							if (iBreakX == 1) tPosChange.x = 0;	if (iBreakY == 1) tPosChange.y = 0;
							transform.Translate(tPosChange);
							transform.localScale = tS;
							tRigidBody.mass /= iNeededObjects + 1;
							tRigidBody.velocity = m_tVelocity;
							tRigidBody.WakeUp();
							tCollider.enabled = true;
							//FractureForce /= 2;
							if (!CanBreak() && gameObject.GetComponent<DebrisController>() == null)
								gameObject.AddComponent<DebrisController>().Init(this);
						} else {
							var tObj = BoxPool.Instance.GetObject();
							if (tObj != null) {
								BreakController tContr = tObj.GetComponent<BreakController>();
								tContr.FractureSize = FractureSize;
								tContr.transform.parent = transform.parent;
								tContr.transform.localScale = transform.localScale;
								tContr.transform.rotation = transform.rotation;
								tContr.transform.position = transform.position;
								Vector3 tShift = new Vector3(x * transform.localScale.x, y * transform.localScale.y, 0);
								tContr.transform.Translate(-tShift);
								tContr.Init();
								tContr.FractureForce = FractureForce;
								tContr.tRigidBody.mass = tRigidBody.mass;
								tContr.tRigidBody.velocity = m_tVelocity;// GetComponent<Rigidbody>().velocity;
								tContr.tRigidBody.angularVelocity = tRigidBody.angularVelocity;
								tContr.tRigidBody.WakeUp();
								if (!tContr.CanBreak() && tContr.gameObject.GetComponent<DebrisController>() == null) {
									tContr.gameObject.AddComponent<DebrisController>().Init(tContr);
								} else if (tCol != null) {
									Vector3 tBodyPoint = tContr.tCollider.bounds.ClosestPoint(tCol.bounds.center);
									Vector3 tColliderPoint = tCol.bounds.ClosestPoint(tBodyPoint);
									if (Vector3.Distance(tBodyPoint, tColliderPoint) < FractureSize / 2.0f) {
										tContr.SetBreak(tCol);
									}
								}
							}
						}
					}
			if (tCol != null) {
				Vector3 tBodyPoint = tCollider.bounds.ClosestPoint(tCol.bounds.center);
				Vector3 tColliderPoint = tCol.bounds.ClosestPoint(tBodyPoint);
				if (Vector3.Distance(tBodyPoint, tColliderPoint) < FractureSize / 2.0f) {
					SetBreak(tCol);
				}
			}
			return true;
		}
		return false;
	}

	public void SetBreak(Collider2D tCol = null)
	{
		if (CanBreak()) {
			Break(tCol);
		} else {
			tRigidBody.WakeUp();
		}
	}

	public void Wakeup()
	{
		tRigidBody.WakeUp();
	}

	void Update()
	{
		if (fDelayStart > 0 && fDelayStart != float.MaxValue)
			fDelayStart -= Time.deltaTime;

		if (fDelayStart <= 0) {
			tRigidBody.WakeUp();
			tRigidBody.velocity = InitialVelocity;
			tRigidBody.angularVelocity = InitialAngularVelocity;
		}
	}

	void FixedUpdate()
	{
		m_tVelocity = tRigidBody.velocity;
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		BreakController tOtherBody = col.gameObject.GetComponent<BreakController>();
		float fKineticSelf = LastKineticEnergy();
		float fKineticOther = (tOtherBody != null) ? tOtherBody.LastKineticEnergy() : 0;
		float fDamage = col.relativeVelocity.sqrMagnitude * (fKineticOther - fKineticSelf) / 100.0f;
		if (Mathf.Abs(fDamage) >= FractureForce) {
			if (col.transform.parent != transform.parent) {
				SetBreak(col.collider);
			} else {
				SetBreak();
			}
		}
//		if (fKineticOther < fKineticSelf && tOtherBody != null)
//			rigidbody.velocity = m_tVelocity;
	}

	protected bool CanBreak() { return CanBreakX() || CanBreakY(); }
	bool CanBreakX() { return gameObject.transform.localScale.x > FractureSize; }
	bool CanBreakY() { return gameObject.transform.localScale.y > FractureSize; }
	float KineticEnergy() { return 0.5f * tRigidBody.mass * tRigidBody.velocity.sqrMagnitude; }
	float LastKineticEnergy() { return 0.5f * tRigidBody.mass * m_tVelocity.sqrMagnitude; }
	public void Deactivate() { BoxPool.Instance.PoolObject(gameObject); }
}
