using UnityEngine;
using System.Collections;

public class BreakableBox : MonoBehaviour
{
    public float Density = 10.0f;
    public float FractureSize = 0.2f;
    public float FractureForce = 1.0f;
    private Vector3 m_tVelocity;
    private float m_tAngVelocity;
    protected Rigidbody2D m_tRigidBody = null;
    protected BoxCollider2D m_tCollider = null;
    private DebrisController m_tDebris = null;

    public void SetupObject(Vector3 tPosition, Quaternion tRotation, Vector3 tTranslate, Vector3 tScale, float fMass, Vector3 tVel, float fAngVel)
	{
        Init();
        transform.position = tPosition;
        transform.rotation = tRotation;
        transform.Translate(tTranslate);
		transform.localScale = tScale;
		m_tRigidBody.mass = fMass;
		m_tRigidBody.velocity = tVel;
		m_tRigidBody.angularVelocity = fAngVel;
		m_tRigidBody.WakeUp();
		m_tCollider.enabled = true;
		if (!CanBreakX() && !CanBreakY())
		   	SetDebris();
	}

	public void SetDebris()
	{
		if (m_tDebris == null)
			m_tDebris = gameObject.AddComponent<DebrisController>();
	}

	public void Break(Collider2D tCol = null)
	{
		if (m_tDebris != null)
            return;
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
	                        SetupObject(transform.position, transform.rotation, tPosChange, tS, m_tRigidBody.mass / iNeededObjects + 1, m_tVelocity, m_tAngVelocity);
							FractureForce /= 1.5f;
                    } else {
							BreakableBox tContr = BoxPool.Instance.GetObject();
							if (tContr != null) {
								tContr.FractureSize = FractureSize;
                            	tContr.transform.parent = transform.parent;
								Vector3 tShift = new Vector3(x * transform.localScale.x, y * transform.localScale.y, 0);
	                            tContr.SetupObject(transform.position, transform.rotation, -tShift, transform.localScale, m_tRigidBody.mass, m_tVelocity, m_tAngVelocity);
	                            tContr.FractureForce = FractureForce;
								if (tCol != null)
									tContr.BreakAt(tCol);
							}
						}
					}
			if (tCol != null)
                BreakAt(tCol);
		}
	}

	protected void BreakAt(Collider2D tCol)
	{
		Vector3 tBodyPoint = m_tCollider.bounds.ClosestPoint(tCol.bounds.center);
		Vector3 tColliderPoint = tCol.bounds.ClosestPoint(tBodyPoint);
		if (Vector3.Distance(tBodyPoint, tColliderPoint) < FractureSize / 2.0f)
			Break(tCol);
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		BreakableBox tOtherBody = col.gameObject.GetComponent<BreakableBox>();
		float fKineticSelf = KineticEnergy();
		float fKineticOther = (tOtherBody != null) ? tOtherBody.KineticEnergy() : 0;
		float fDamage = col.relativeVelocity.sqrMagnitude * (fKineticOther - fKineticSelf) / 100.0f;
		if (Mathf.Abs(fDamage) >= FractureForce)
			Break(col.collider);
	}

    void Start()				{ Init(); m_tRigidBody.mass = transform.localScale.x * transform.localScale.y * Density; }
    public void Wakeup()		{ m_tRigidBody.WakeUp(); }
    void Init()					{ m_tRigidBody = gameObject.GetComponent<Rigidbody2D>();	m_tCollider = gameObject.GetComponent<BoxCollider2D>(); }
    void FixedUpdate()			{ m_tVelocity = m_tRigidBody.velocity; m_tAngVelocity = m_tRigidBody.angularVelocity; }
	bool CanBreakX()			{ return transform.localScale.x > FractureSize; }
	bool CanBreakY()			{ return transform.localScale.y > FractureSize; }
	float KineticEnergy()		{ return m_tRigidBody.mass * m_tVelocity.sqrMagnitude; }
	public void Deactivate()
	{
		if (m_tRigidBody && !m_tRigidBody.isKinematic)
				m_tRigidBody.velocity = Vector3.zero;
        m_tDebris = null;
        BoxPool.Instance.PoolObject(this);
	}
}
