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
	List<BreakableBox> childs = new List<BreakableBox> ();
	List<BreakableBox> childsToRemove = new List<BreakableBox> ();
	public List<BreakableBox> Childs { get { return childs; } }
	public List<BreakableBox> ChildsToRemove { get { return childsToRemove; } }

	bool m_bIntegrityCheck;

	public void AddChild (BreakableBox tBox)
	{
		if (!Childs.Contains (tBox)) {
			Childs.Add (tBox);
		}
	}
	public void RemoveChild (BreakableBox tBox)
	{
		Childs.Remove (tBox);
		if (Childs.Count == 0) {
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
		if (Childs.Count <= 1) {
			return;
		}
		foreach (BreakableBox tBox in Childs) {
			tBox.NeedRefreshNeighbours = true;
			tBox.Neighbours.Clear ();
		}
		foreach (BreakableBox tBox in Childs) {
			tBox.RefreshNeighbours ();
		}

		var tConn = new List<BreakableBox> ();
		childs [0].GetConnectedBox (tConn);
		if (tConn.Count < childs.Count)
			DetachBody (tConn);
	}

	void Start ()
	{
		Init ();
	}

	public void Init ()
	{
		childs = new List<BreakableBox> (GetComponentsInChildren<BreakableBox> ());
		Body = gameObject.GetComponent<Rigidbody2D> ();
		Body.velocity = Velocity;
		Body.angularVelocity = AngVelocity;
		m_bIntegrityCheck = true;
		float fMass = 0;
		foreach (BreakableBox tBox in Childs) {
			tBox.Init (this);
			fMass += tBox.Mass;
		}
		Body.mass = fMass;
	}

	void FixedUpdate ()
	{
		DetachBody (ChildsToRemove);

		if (m_bIntegrityCheck) {
			CheckIntegrity ();
		}
		Velocity = Body.velocity;
		AngVelocity = Body.angularVelocity;
	}

	public void Deactivate ()
	{
		if (Body && !Body.isKinematic) {
			Body.velocity = Vector3.zero;
		}
		foreach (BreakableBox tBox in Childs) {
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
		tNewContr.Velocity = Body.velocity;
		tNewContr.AngVelocity = Body.angularVelocity;

		float fMass = 0;
		foreach (BreakableBox tBox in tBoxes) {
			tBox.transform.parent = tNewContr.transform;
			RemoveChild (tBox);
			tBox.Init (tNewContr, tBox.transform, Vector3.zero);
			fMass += tBox.Mass;
		}
		tBoxes.Clear ();
		tNewContr.Init ();

		if (System.Math.Abs (fMass - 0) < 0.1f) {
			fMass = 0.1f;
		}
		Body.mass = fMass;
	}
}
