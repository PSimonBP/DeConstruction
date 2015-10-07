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
	public List<BreakableBox> Childs { get { return childs; } }

	bool m_bIntegrityCheck;

	public void AddChild (BreakableBox tBox)
	{
		if (!Childs.Contains (tBox)) {
			Childs.Add (tBox);
			m_bIntegrityCheck = true;
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
		do {
			tConn.Clear ();
			childs [0].GetConnectedBoxes (tConn);
			if (tConn.Count < childs.Count) {
				m_bIntegrityCheck = true;
				DetachBody (tConn);
			}
		} while (tConn.Count < childs.Count);
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

	void Update ()
	{
/*		if (m_bIntegrityCheck) {
			CheckIntegrity();
		}
*/
	}

	void FixedUpdate ()
	{
		Velocity = Body.velocity;
		AngVelocity = Body.angularVelocity;
		if (m_bIntegrityCheck) {
			CheckIntegrity ();
		}
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

	public void DetachBox (BreakableBox tBox)
	{
		var tList = new List<BreakableBox> ();
		tList.Add (tBox);
		DetachBody (tList);
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

		foreach (BreakableBox tBox in tBoxes) {
			RemoveChild (tBox);
		}
		tNewContr.Init ();

		float fMass = 0;
		foreach (BreakableBox tBox in childs) {
			fMass += tBox.Mass;
		}
		Body.mass = fMass;
		m_bIntegrityCheck = true;
	}
}
