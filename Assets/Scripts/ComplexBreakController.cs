using UnityEngine;
using System.Collections;

public class ComplexBreakController : MonoBehaviour {
	public float FractureSize = 3.0f;
	public float FractureForce = 20.0f;
	public float MinLifeTime = 2.0f;
	public float MaxLifeTime = 5.0f;
	public Vector3 InitialVelocity = Vector3.zero;
	public Vector3 InitialAngularVelocity = Vector3.zero;

	float fLifeTime = float.MaxValue;
	protected Vector3 m_tVelocity;

	bool bInitialized = false;
	bool bBreak = false;

	void Start()
	{
		if (!bInitialized) {
			Init();
			GetComponent<Rigidbody>().velocity = InitialVelocity;
			GetComponent<Rigidbody>().angularVelocity = InitialAngularVelocity;
		}
	}

	void Init()
	{
		gameObject.layer = 0;
		bBreak = false;
		fLifeTime = float.MaxValue;
		bInitialized = true;
	}

	public bool Break(Collider tCol = null)
	{
		int iNeededObjects = 1;
		float iBreakX = 1;
		float iBreakY = 1;
		float iBreakZ = 1;
		if (CanBreakX())
			iBreakX = 2;
		if (CanBreakY())
			iBreakY = 2;
		if (CanBreakZ())
			iBreakZ = 2;
		iBreakX = (int)iBreakX;
		iBreakY = (int)iBreakY;
		iBreakZ = (int)iBreakZ;
		iNeededObjects *= (int)iBreakX;
		iNeededObjects *= (int)iBreakY;
		iNeededObjects *= (int)iBreakZ;
		iNeededObjects -= 1;
		{
			Vector3 tS = new Vector3(transform.localScale.x / iBreakX, transform.localScale.y / iBreakY, transform.localScale.z / iBreakZ);
			Vector3 tSize = GetLargestChildSize();
			Vector3 tPosChange = new Vector3(tSize.x * tS.x, tSize.y * tS.y, tSize.z * tS.z);
			if (iBreakX == 1)
				tPosChange.x = 0;
			if (iBreakY == 1)
				tPosChange.y = 0;
			if (iBreakZ == 1)
				tPosChange.z = 0;

			for (float x = 0; x < iBreakX; ++x)
				for (float y = 0; y < iBreakY; ++y)
					for (float z = 0; z < iBreakZ; ++z) {
						if (x == 0 && y == 0 && z == 0) {
							transform.Translate(tPosChange / 2);
							transform.localScale = tS;
							bBreak = false;
							fLifeTime = float.MaxValue;
							Rigidbody tRBody = gameObject.GetComponent<Rigidbody>();
							tRBody.mass /= iNeededObjects + 1;
							tRBody.velocity = m_tVelocity;
							tRBody.WakeUp();
							if (!CanBreak())
								gameObject.layer = 8;
						} else {
							var tObj = Instantiate(transform);
							if (tObj != null) {
								ComplexBreakController tContr = tObj.GetComponent<ComplexBreakController>();
								tContr.FractureSize = FractureSize;
								tContr.MinLifeTime = MinLifeTime;
								tContr.MaxLifeTime = MaxLifeTime;
								tContr.transform.parent = transform.parent;
								tContr.transform.localScale = transform.localScale;
								tContr.transform.localRotation = transform.localRotation;
								tContr.transform.position = transform.position;
								tContr.transform.Translate(-tPosChange);
								tContr.Init();
								Rigidbody tRBody = tContr.GetComponent<Rigidbody>();
								tRBody.mass = GetComponent<Rigidbody>().mass;
								tRBody.velocity = m_tVelocity;// GetComponent<Rigidbody>().velocity;
								tRBody.angularVelocity = GetComponent<Rigidbody>().angularVelocity;
								tRBody.WakeUp();
								if (!CanBreak())
									tContr.gameObject.layer = 8;
							}
						}
					}
		}
		return true;
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
		if (fLifeTime > 0 && fLifeTime != float.MaxValue)
			fLifeTime -= Time.deltaTime;

		if (gameObject.layer == 8) {
			if (gameObject.GetComponent<Rigidbody>().IsSleeping())
				Destroy(gameObject);
			else
				fLifeTime = Mathf.Min(fLifeTime, Random.Range(MinLifeTime, MaxLifeTime));
		}
	}

	void LateUpdate()
	{
		if (fLifeTime <= 0)
			Destroy(gameObject);
	}

	void FixedUpdate()
	{
		m_tVelocity = GetComponent<Rigidbody>().velocity;
		if (bBreak)
			SetBreak();
	}

	void OnCollisionEnter(Collision col)
	{
//		BreakController tOtherBody = col.gameObject.GetComponent<BreakController>();
		float fKineticSelf = LastKineticEnergy();
		float fKineticOther = 0; // (tOtherBody != null) ? tOtherBody.LastKineticEnergy() : 0;
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

	Vector3 GetLargestChildSize() {
		Vector3 tSize = gameObject.transform.localScale;
		Transform[] tTransforms = GetComponentsInChildren<Transform>();
		foreach (Transform tTransform in tTransforms) {
			if (tTransform.localScale.x > tSize.x || tTransform.localScale.y > tSize.y || tTransform.localScale.z > tSize.z)
				tSize = tTransform.localScale;
		}
		if (tSize != transform.localScale)
			tSize = new Vector3(tSize.x * transform.localScale.x, tSize.y * transform.localScale.y, tSize.z * transform.localScale.z);
		return tSize;
	}

	bool CanBreak() { return CanBreakX() || CanBreakY() || CanBreakZ(); }
	bool CanBreakX() { return GetLargestChildSize().x >= FractureSize; }
	bool CanBreakY() { return GetLargestChildSize().y >= FractureSize; }
	bool CanBreakZ() { return GetLargestChildSize().z >= FractureSize; }
	float KineticEnergy() { return 0.5f * GetComponent<Rigidbody>().mass * GetComponent<Rigidbody>().velocity.sqrMagnitude; }
	float LastKineticEnergy() { return 0.5f * GetComponent<Rigidbody>().mass * m_tVelocity.sqrMagnitude; }
}
