using System.Collections.Generic;
using UnityEngine;

public class BreakableContainer : MonoBehaviour
{
	public float Density = 10.0f;
	public float FractureSize = 0.2f;
	public float FractureForce = 1.0f;
	
	public Rigidbody2D Body { get; set; }
	public Vector2 Velocity { get; set; }
	public float AngVelocity { get; set; }

	bool m_bIntegrityCheck;
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
				tBox.PingNeighbors (true);
				if (!tBox.InBody)
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
		Body.mass = fMass;
		Body.velocity = tVel;
		Body.angularVelocity = fAngVel;
		Body.WakeUp ();
	}

	void Start ()
	{
		Init ();
	}

	public void Wakeup ()
	{
		Body.WakeUp ();
	}

	public void Init ()
	{
		m_bIntegrityCheck = true;
		m_tChilds.Clear ();
		m_tChilds = new List<BreakableBox> (GetComponentsInChildren<BreakableBox> ());
		float fMass = 0;
		foreach (BreakableBox tBox in m_tChilds) {
			tBox.Init (this);
			fMass += tBox.Mass;
		}
		Body = gameObject.GetComponent<Rigidbody2D> ();
		Body.mass = fMass;
	}

	void Update ()
	{
	}

	void FixedUpdate ()
	{
		if (m_bIntegrityCheck) {
			CheckIntegrity ();
		}
		Velocity = Body.velocity;
		AngVelocity = Body.angularVelocity;
	}

	public float KineticEnergy ()
	{
		return Body.mass * Velocity.sqrMagnitude;
	}

	public void Deactivate ()
	{
		if (Body && !Body.isKinematic) {
			Body.velocity = Vector3.zero;
		}
		foreach (BreakableBox tBox in m_tChilds) {
			tBox.Deactivate ();
		}
		Body.isKinematic = false;
		ContainerPool.Instance.PoolObject (gameObject);
	}

	public void DetachBody (List<BreakableBox> tBoxes)
	{
		if (tBoxes.Count == 0) {
			return;
		}
		// todo check if kinematic, and attached to kinematic neighbor
		BreakableContainer tNewContr = ContainerPool.GetContainer ();
		tNewContr.FractureForce = FractureForce;
		tNewContr.FractureSize = FractureSize;
		tNewContr.Density = Density;

		tNewContr.transform.parent = transform.parent;
		tNewContr.transform.position = transform.position;
		tNewContr.transform.rotation = transform.rotation;
		foreach (BreakableBox tBox in tBoxes) {
			tBox.transform.parent = tNewContr.transform;
			RemoveChild (tBox);
			tBox.SetupObject (tNewContr, tBox.transform, Vector3.zero);
		}
		tNewContr.Init ();
		tNewContr.Body.velocity = Body.velocity;
		tNewContr.Body.angularVelocity = Body.angularVelocity;
		tNewContr.Velocity = Body.velocity;
		tNewContr.AngVelocity = Body.angularVelocity;

		float fMass = 0;
		foreach (BreakableBox tBox in m_tChilds) {
			fMass += tBox.Mass;
		}
		if (System.Math.Abs (fMass - 0) < 0.001f) {
			fMass = 0.001f;
		}
		Body.mass = fMass;
	}
}
