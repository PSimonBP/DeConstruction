using UnityEngine;
using System.Collections;

public class BreakController : MonoBehaviour
{
	public float FractureSize = 0.5f;
	public float FractureForce = 2.0f;
	public float MinLifeTime = 0.2f;
	public float MaxLifeTime = 2.0f;
	public float DelayStart = 10;
	public Vector3 InitialVelocity = Vector3.zero;
	public Vector3 InitialAngularVelocity = Vector3.zero;

	BoxCollider tCollider = null;

	float fDelayStart = float.MaxValue;
	float fLifeTime = float.MaxValue;
	protected Vector3 m_tVelocity;

	bool bInitialized = false;
	bool bBreak = false;

	void Start()
	{
		if (!bInitialized) {
			Init();
			if (InitialVelocity != Vector3.zero || InitialAngularVelocity != Vector3.zero)
				fDelayStart = DelayStart;
		}
	}

	void Init()
	{
		gameObject.layer = 0;
		bBreak = false;
		fLifeTime = float.MaxValue;
		BoxCollider[] tColliders = gameObject.GetComponents<BoxCollider>();
		foreach (BoxCollider tColl in tColliders) {
			tCollider = tColl;
		}
		bInitialized = true;
	}

	public bool Break(Collider tCol = null)
	{
		int iNeededObjects = 1;
		float iBreakX = 1;
		float iBreakY = 1;
		float iBreakZ = 1;
		if (CanBreakX()) {
			iBreakX = 2;
			if (transform.localScale.x * 2 >= transform.localScale.y)	iBreakX = (int)Mathf.Max(iBreakX, transform.localScale.y / transform.localScale.x);
			if (transform.localScale.x * 2 >= transform.localScale.z)	iBreakX = (int)Mathf.Max(iBreakX, transform.localScale.z / transform.localScale.x);
		}
		if (CanBreakY()) {
			iBreakY = 2;
			if (transform.localScale.y * 2 >= transform.localScale.x)	iBreakY = (int)Mathf.Max(iBreakY, transform.localScale.x / transform.localScale.y);
			if (transform.localScale.y * 2 >= transform.localScale.z)	iBreakY = (int)Mathf.Max(iBreakY, transform.localScale.z / transform.localScale.y);
		}
		if (CanBreakZ()) {
			iBreakZ = 2;
			if (transform.localScale.z * 2 >= transform.localScale.x)	iBreakZ = (int)Mathf.Max(iBreakZ, transform.localScale.x / transform.localScale.z);
			if (transform.localScale.z * 2 >= transform.localScale.y)	iBreakZ = (int)Mathf.Max(iBreakZ, transform.localScale.y / transform.localScale.z);
		}

		iBreakX = (int)iBreakX;	iBreakY = (int)iBreakY;	iBreakZ = (int)iBreakZ;
		iNeededObjects *= (int)iBreakX;	iNeededObjects *= (int)iBreakY;	iNeededObjects *= (int)iBreakZ;	iNeededObjects -= 1;

		if (iNeededObjects <= BoxPool.Instance.GetFreePoolSize()) {
			for (float x = 0; x < iBreakX; ++x)
				for (float y = 0; y < iBreakY; ++y)
					for (float z = 0; z < iBreakZ; ++z) {
						if (x == 0 && y == 0 && z == 0) {
							Vector3 tS = new Vector3(transform.localScale.x / iBreakX, transform.localScale.y / iBreakY, transform.localScale.z / iBreakZ);
							Vector3 tPosChange = tS / 2;
							if (iBreakX == 1) tPosChange.x = 0;	if (iBreakY == 1) tPosChange.y = 0;	if (iBreakZ == 1) tPosChange.z = 0;
							transform.Translate(tPosChange);
							transform.localScale = tS;
							bBreak = false;
							fLifeTime = float.MaxValue;
							Rigidbody tRBody = gameObject.GetComponent<Rigidbody>();
							tRBody.mass /= iNeededObjects + 1;
							tRBody.velocity = m_tVelocity;
							tRBody.WakeUp();
							tCollider.enabled = true;
							if (!CanBreak())
								gameObject.layer = 8;
						} else {
							var tObj = BoxPool.Instance.GetObject();
							if (tObj != null) {
								BreakController tContr = tObj.GetComponent<BreakController>();
								tContr.FractureSize = FractureSize;
								tContr.MinLifeTime = MinLifeTime;
								tContr.MaxLifeTime = MaxLifeTime;
								tContr.transform.parent = transform.parent;
								tContr.transform.localScale = transform.localScale;
								tContr.transform.localRotation = transform.localRotation;
								tContr.transform.position = transform.position;
								Vector3 tShift = new Vector3(x * transform.localScale.x, y * transform.localScale.y, z * transform.localScale.z);
								tContr.transform.Translate(-tShift);
								tContr.Init();
								tContr.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
								tContr.tCollider.material = GetComponent<Collider>().material;
								Rigidbody tRBody = tContr.GetComponent<Rigidbody>();
								tRBody.mass = GetComponent<Rigidbody>().mass;
								tRBody.velocity = m_tVelocity;// GetComponent<Rigidbody>().velocity;
								tRBody.angularVelocity = GetComponent<Rigidbody>().angularVelocity;
								tRBody.WakeUp();
								if (!CanBreak())
									tContr.gameObject.layer = 8;
								if (tCol != null) {
									Vector3 tBodyPoint = tContr.tCollider.ClosestPointOnBounds(tCol.bounds.center);
									Vector3 tColliderPoint = tCol.ClosestPointOnBounds(tBodyPoint);
									if (Vector3.Distance(tBodyPoint, tColliderPoint) < FractureSize / 2.0f) {
										tContr.SetBreak(tCol);
									}
								}
							}
						}
					}
			if (tCol != null) {
				Vector3 tBodyPoint = tCollider.ClosestPointOnBounds(tCol.bounds.center);
				Vector3 tColliderPoint = tCol.ClosestPointOnBounds(tBodyPoint);
				if (Vector3.Distance(tBodyPoint, tColliderPoint) < FractureSize / 2.0f) {
					SetBreak(tCol);
				}
			}
			return true;
		}
		return false;
	}

	public void SetBreak(Collider tCol = null)
	{
		if (CanBreak()) {
			if (Break(tCol)) {
				bBreak = false;
			} else {
				bBreak = true;
			}
		} else {
			bBreak = false;
			gameObject.layer = 8;
//			fLifeTime = Mathf.Min(fLifeTime, Random.Range(MinLifeTime, MaxLifeTime));
			gameObject.GetComponent<Rigidbody>().WakeUp();
		}
	}

	public void Wakeup()
	{
		gameObject.GetComponent<Rigidbody>().WakeUp();
	}

	void Update()
	{
		if (fDelayStart > 0 && fDelayStart != float.MaxValue)
			fDelayStart -= Time.deltaTime;

		if (fLifeTime > 0 && fLifeTime != float.MaxValue)
			fLifeTime -= Time.deltaTime;

		if (fDelayStart <= 0) {
			GetComponent<Rigidbody>().WakeUp();
			GetComponent<Rigidbody>().velocity = InitialVelocity;
			GetComponent<Rigidbody>().angularVelocity = InitialAngularVelocity;
		}

		if (gameObject.layer == 8) {
			if (gameObject.GetComponent<Rigidbody>().IsSleeping())
				Deactivate();
			else
				fLifeTime = Mathf.Min(fLifeTime, Random.Range(MinLifeTime, MaxLifeTime));
		}
	}

	void LateUpdate()
	{
		if (fLifeTime <= 0)
			Deactivate();
	}

	void FixedUpdate()
	{
		m_tVelocity = GetComponent<Rigidbody>().velocity;
		if (bBreak)
			SetBreak();
	}

	void OnCollisionEnter(Collision col)
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

	bool CanBreak() { return CanBreakX() || CanBreakY() || CanBreakZ(); }
	bool CanBreakX() { return gameObject.transform.localScale.x >= FractureSize; }
	bool CanBreakY() { return gameObject.transform.localScale.y >= FractureSize; }
	bool CanBreakZ() { return gameObject.transform.localScale.z >= FractureSize; }
	float KineticEnergy() { return 0.5f * GetComponent<Rigidbody>().mass * GetComponent<Rigidbody>().velocity.sqrMagnitude; }
	float LastKineticEnergy() { return 0.5f * GetComponent<Rigidbody>().mass * m_tVelocity.sqrMagnitude; }
	void Deactivate() { BoxPool.Instance.PoolObject(gameObject); }
}
