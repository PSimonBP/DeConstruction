using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableContainer : MonoBehaviour
{
	public float Density = 1.0f;
	public float FractureForce = 2.0f;
	public float MaxHeat = 1000;
	public int FlameSpread = 1;
	public Rigidbody2D Body { get; set; }
	public Vector2 Velocity { get; set; }
	public float AngVelocity { get; set; }
	public bool DebugDraw { get; set; }
	List<BreakableBox> childs = new List<BreakableBox>();
	public List<BreakableBox> Childs { get { return childs; } }

	bool m_bIntegrityCheck;
	float m_fIntegrityTimer;

/*	Vector3 m_tPosition = new Vector3();
	Vector3 m_tRotation = new Vector3();
	int m_iStayCounter;
*/
	public void AddChild(BreakableBox tBox)
	{
		if (!Childs.Contains(tBox)) {
			Childs.Add(tBox);
			tBox.transform.SetParent(transform);
		}
	}
	public void RemoveChild(BreakableBox tBox)
	{
		childs.Remove(tBox);
		if (childs.Count == 0)
			Deactivate();
		else if (childs.Count > 1)
			m_bIntegrityCheck = true;
	}

	IEnumerator WaitForUpdate()
	{
		yield return new WaitForFixedUpdate();
	}

	public void CheckIntegrity()
	{
		m_bIntegrityCheck = false;
		WaitForUpdate();
		for (int i = 0; i < childs.Count; i++)
			childs [i].RefreshNeighbours();
		var tConn = new List<BreakableBox>();
		do {
			tConn.Clear();
			childs [0].GetConnectedBoxes(tConn);
			if (tConn.Count < childs.Count) {
				for (int i=0; i<childs.Count; ++i) {
					if (!tConn.Contains(childs [i])) {
						var tDetach = new List<BreakableBox>();
						childs [i].GetConnectedBoxes(tDetach);
						DetachBody(tDetach);
						break;
					}
				}
//				Debug.Log("Detach #" + (childs.Count - tConn.Count));
//				DetachBody(tConn);
			}
		} while (tConn.Count < childs.Count);
		if (Body.isKinematic) {
			bool bKinematic = false;
			for (int i=0; i<childs.Count; ++i)
				bKinematic |= childs [i].Kinematic;
			Body.isKinematic = bKinematic;
		}
	}

	void Start()
	{
		Init();
		for (int i = 0; i < childs.Count; i++)
			childs [i].RefreshNeighbours();
	}

	void UpdateMass()
	{
		float fMass = 0;
		for (int i = 0; i < childs.Count; i++) {
			Vector3 tScale = childs [i].transform.localScale;
			fMass += tScale.x * tScale.y;
		}
		Body.mass = fMass * Density;
	}

	public void Init()
	{
		childs = new List<BreakableBox>(GetComponentsInChildren<BreakableBox>());
		Body = gameObject.GetComponent<Rigidbody2D>();
		Body.velocity = Velocity;
		Body.angularVelocity = AngVelocity;
		m_bIntegrityCheck = true;
		for (int i = 0; i < childs.Count; i++)
			childs [i].Init(this);
		UpdateMass();
	}

	void Update()
	{
		if (m_bIntegrityCheck) {
			m_fIntegrityTimer += Time.deltaTime;
//			if (m_fIntegrityTimer >= 0.1f)
			CheckIntegrity();
		} else {
			m_fIntegrityTimer = 0;
		}

		if (Body.IsSleeping()) {
			bool bChanged;
			do {
				bChanged = false;
				for (int i=0; i<childs.Count; ++i) {
					if (childs [i].NeedRefreshNeighbours)
						childs [i].RefreshNeighbours();
					for (int j=0; j<childs[i].Neighbours.Count; ++j) {
						BreakableBox tBox = childs [i];
						BreakableBox tNeighbour = tBox.Neighbours [j];
						Vector3 tP1 = tBox.transform.localPosition;
						Vector3 tP2 = tNeighbour.transform.localPosition;
						Vector3 tS1 = tBox.transform.localScale;
						Vector3 tS2 = tNeighbour.transform.localScale;
						if (tS1 == tS2) {
							if (Mathf.Abs(tP1.x - tP2.x) <= 0.05f) {
								tBox.transform.localScale = new Vector3(tS1.x, tS1.y + tS2.y, 1);
								tBox.transform.localPosition = (tP1 + tP2) / 2;
								tNeighbour.Deactivate();
								tBox.ResetNeighbours();
								bChanged = true;
								break;
							} else if (Mathf.Abs(tP1.y - tP2.y) <= 0.05f) {
								tBox.transform.localScale = new Vector3(tS1.x + tS2.x, tS1.y, 1);
								tBox.transform.localPosition = (tP1 + tP2) / 2;
								tNeighbour.Deactivate();
								tBox.ResetNeighbours();
								bChanged = true;
								break;
							}
						} else if (Mathf.Abs(tS1.x - tS2.x) <= 0.05f && Mathf.Abs(tP1.x - tP2.x) <= 0.05f) {
							tBox.transform.localScale = new Vector3(tS1.x, tS1.y + tS2.y, 1);
							tBox.transform.localPosition = new Vector3((tP1.x + tP2.x) / 2, ((tP1.y * tS1.y) + (tP2.y * tS2.y)) / (tS1.y + tS2.y), (tP1.z + tP2.z) / 2);
							tBox.ResetNeighbours();
							tNeighbour.Deactivate();
							bChanged = true;
							break;
						} else if (Mathf.Abs(tS1.y - tS2.y) <= 0.05f && Mathf.Abs(tP1.y - tP2.y) <= 0.05f) {
							tBox.transform.localScale = new Vector3(tS1.x + tS2.x, tS1.y, 1);
							tBox.transform.localPosition = new Vector3(((tP1.x * tS1.x) + (tP2.x * tS2.x)) / (tS1.x + tS2.x), (tP1.y + tP2.y) / 2, (tP1.z + tP2.z) / 2);
							tBox.ResetNeighbours();
							tNeighbour.Deactivate();
							bChanged = true;
							break;
						}
					}
				}
				if (bChanged)
					Body.Sleep();
			} while (bChanged);
		}/* else {
			if (Vec3Cmp(transform.position, m_tPosition) && Vec3Cmp(transform.rotation.eulerAngles, m_tRotation)) {
				m_iStayCounter++;
				if (m_iStayCounter > 3) {
					Body.Sleep();
				}
			} else {
				m_tPosition = transform.position;
				m_tRotation = transform.rotation.eulerAngles;
				m_iStayCounter = 0;
			}
		}*/
	}

	bool Vec3Cmp(Vector3 v1, Vector3 v2)
	{
		if (Mathf.Abs(v1.x - v2.x) >= Mathf.Epsilon)
			return false;
		if (Mathf.Abs(v1.y - v2.y) >= Mathf.Epsilon)
			return false;
		if (Mathf.Abs(v1.z - v2.z) >= Mathf.Epsilon)
			return false;
		return true;
	}

	void FixedUpdate()
	{
		Velocity = Body.velocity;
		AngVelocity = Body.angularVelocity;
	}

	public void Deactivate()
	{
		if (Body && !Body.isKinematic)
			Body.velocity = Vector3.zero;
		for (int i = 0; i < Childs.Count; i++)
			Childs [i].Deactivate();
		Body.isKinematic = false;
		ContainerPool.Instance.PoolObject(gameObject);
	}

	public void DetachBox(BreakableBox tBox)
	{
		WaitForUpdate();
		var tList = new List<BreakableBox>();
		tList.Add(tBox);
		DetachBody(tList);
	}

	public void DetachBody(List<BreakableBox> tBoxes)
	{
		if (tBoxes.Count == 0)
			return;
		BreakableContainer tNewContr = ContainerPool.GetContainer();
		tNewContr.FractureForce = FractureForce;
		tNewContr.Density = Density;
		tNewContr.MaxHeat = MaxHeat;
		tNewContr.DebugDraw = false;

		tNewContr.transform.SetParent(transform.parent);
		tNewContr.transform.position = transform.position;
		tNewContr.transform.rotation = transform.rotation;
		tNewContr.Velocity = Body.velocity;
		tNewContr.AngVelocity = Body.angularVelocity;

		for (int i = 0; i < tBoxes.Count; i++) {
			BreakableBox tBox = tBoxes [i];
			RemoveChild(tBox);
			tBox.transform.SetParent(tNewContr.transform);
		}
		tNewContr.Init();
		tNewContr.Body.isKinematic = Body.isKinematic;
		UpdateMass();
	}
}
