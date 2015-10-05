using System.Collections.Generic;
using UnityEngine;

public class BreakableContainer : MonoBehaviour
{
	public bool	 FastMoving;
	public float Density = 10.0f;
	public float FractureSize = 0.2f;
	public float FractureForce = 1.0f;

	Vector2 m_tVelocity;
	float m_tAngVelocity;
	Rigidbody2D m_tRigidBody;
	bool m_bIntegrityCheck;

	public Rigidbody2D RigidBody {
		get {
			return m_tRigidBody;
		}
	}

	public Vector2 Velocity {
		get {
			return m_tVelocity;
		}
	}

	public float AngVelocity {
		get {
			return m_tAngVelocity;
		}
	}

	List<BreakableBox> m_tChilds = new List<BreakableBox> ();

	public void AddChild (BreakableBox tBox)
	{
		if (!m_tChilds.Contains (tBox)) {
			m_tChilds.Add (tBox);
		}
	}
	public void RemoveChild (BreakableBox tBox)
	{
		m_tChilds.Remove (tBox);
		if (m_tChilds.Count == 0) {
			Deactivate ();
		} else {
			m_bIntegrityCheck = true;
		}
	}

	public void CheckIntegrity ()
	{
		if (!m_bIntegrityCheck) {
			return;
		}
		m_bIntegrityCheck = false;
		if (m_tChilds.Count <= 1) {
			return;
		}
		foreach (BreakableBox tBox in m_tChilds) {
			tBox.InBody = false;
		}
		m_tChilds [0].InBody = true;
		m_tChilds [0].PingNeighbors ();

		List<BreakableBox> tOthers = new List<BreakableBox> ();
		foreach (BreakableBox tBox in m_tChilds) {
			if (!tBox.InBody) {
				tOthers.Add (tBox);
			}
		}
		DetachBody (tOthers);
	}
	public void SetupObject (Vector3 tPosition, Quaternion tRotation, Vector3 tTranslate, Vector3 tScale, float fMass, Vector3 tVel, float fAngVel)
	{
		Init ();
		transform.position = tPosition;
		transform.rotation = tRotation;
		transform.Translate (tTranslate);
		transform.localScale = tScale;
		m_tRigidBody.mass = fMass;
		m_tRigidBody.velocity = tVel;
		m_tRigidBody.angularVelocity = fAngVel;
		m_tRigidBody.WakeUp ();
	}

	void Start ()
	{
		Init ();
	}

	public void Wakeup ()
	{
		m_tRigidBody.WakeUp ();
	}

	public void Init ()
	{
//		m_bIntegrityCheck = true;
		m_tChilds.Clear ();
		m_tChilds = new List<BreakableBox> (GetComponentsInChildren<BreakableBox> ());
		float fMass = 0;
		foreach (BreakableBox tBox in m_tChilds) {
			tBox.Init (this);
			fMass += tBox.Mass;
		}
		m_tRigidBody = gameObject.GetComponent<Rigidbody2D> ();
		m_tRigidBody.mass = fMass;
		if (FastMoving) {
			m_tRigidBody.interpolation = RigidbodyInterpolation2D.Interpolate;
			m_tRigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		} else {
			m_tRigidBody.interpolation = RigidbodyInterpolation2D.None;
			m_tRigidBody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
		}
				//		m_tCollider = gameObject.GetComponent<BoxCollider2D>();
	}

	void Update ()
	{
	}

	void FixedUpdate ()
	{
		m_bIntegrityCheck = true;
		if (m_bIntegrityCheck) {
			CheckIntegrity ();
		}
		m_tVelocity = m_tRigidBody.velocity;
		m_tAngVelocity = m_tRigidBody.angularVelocity;
	}

	public float KineticEnergy ()
	{
		return m_tRigidBody.mass * m_tVelocity.sqrMagnitude;
	}

	public void Deactivate ()
	{
		if (m_tRigidBody && !m_tRigidBody.isKinematic) {
			m_tRigidBody.velocity = Vector3.zero;
		}
		foreach (BreakableBox tBox in m_tChilds) {
			tBox.Deactivate ();
		}
		FastMoving = false;
		ContainerPool.Instance.PoolObject (gameObject);
	}

	public void DetachBody (List<BreakableBox> tBoxes)
	{
		if (tBoxes.Count == 0) {
			return;
		}

		BreakableContainer tNewContr = ContainerPool.GetContainer ();
		tNewContr.transform.parent = transform.parent;
		tNewContr.transform.position = transform.position;
		tNewContr.transform.rotation = transform.rotation;
		foreach (BreakableBox tBox in tBoxes) {
			tBox.transform.parent = tNewContr.transform;
			RemoveChild (tBox);
			tBox.SetupObject (tNewContr, tBox.transform, Vector3.zero);
		}
		tNewContr.Init ();
		tNewContr.RigidBody.velocity = Velocity;
		tNewContr.RigidBody.angularVelocity = AngVelocity;

		float fMass = 0;
		foreach (BreakableBox tBox in m_tChilds) {
			fMass += tBox.Mass;
		}
		if (System.Math.Abs (fMass - 0) < 0.001f) {
			fMass = 0.001f;
		}
		m_tRigidBody.mass = fMass;
	}
}
