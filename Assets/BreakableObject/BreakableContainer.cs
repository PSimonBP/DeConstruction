using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableContainer : MonoBehaviour
{
	public float Density = 10.0f;
	public float FractureForce = 20.0f;
	public float MaxHeat = 100;
	public float HeatSpread = 0.05f;
	public int FlameSpread = 1;
	public Rigidbody2D Body { get; set; }
	public Vector2 Velocity { get; set; }
	public float AngVelocity { get; set; }
	public bool DebugDraw { get; set; }
	List<BreakableBox> childs = new List<BreakableBox>();
	public List<BreakableBox> Childs { get { return childs; } }

	public struct SBox
	{
		public BreakableBox	Box;
		public float Temperature;
	};
	
	public SBox[,] StructGrid;
	public Bounds bounds = new Bounds();

	bool m_bIntegrityCheck;
	bool m_bSimplifyCheck;
	float m_fSimplifyTimer;

	public void AddChild(BreakableBox tBox)
	{
		if (!Childs.Contains(tBox)) {
			Childs.Add(tBox);
			tBox.transform.SetParent(transform);
			m_bSimplifyCheck = true;

			int iWidth = BreakCount (bounds.size.x, BoxPool.DebrisSize);
			int iHeight = BreakCount (bounds.size.y, BoxPool.DebrisSize);
			Vector3 tTopLeft = bounds.center - bounds.extents;
			tTopLeft.x += BoxPool.DebrisSize / 2;
			tTopLeft.y += BoxPool.DebrisSize / 2;
			for (int x = 0; x < iWidth; ++x)
				for (int y = 0; y < iHeight; ++y) {
					if (tBox.Collider.bounds.Contains (transform.localToWorldMatrix.MultiplyPoint(new Vector3 (tTopLeft.x + (x * BoxPool.DebrisSize), tTopLeft.y + (y * BoxPool.DebrisSize))))) {
						Debug.Log (x + ", " + y);
						StructGrid [x, y].Box = tBox;
					}
				}		
		}
	}
	public void RemoveChild(BreakableBox tBox)
	{
		childs.Remove(tBox);
		int iWidth = BreakCount (bounds.size.x, BoxPool.DebrisSize);
		int iHeight = BreakCount (bounds.size.y, BoxPool.DebrisSize);
		for (int x = 0; x < iWidth; ++x)
			for (int y = 0; y < iHeight; ++y) {
				if (StructGrid [x, y].Box == tBox)
					StructGrid [x, y].Box = null;
			}		
		if (childs.Count == 0)
			Deactivate();
		else if (childs.Count > 1) {
			m_bIntegrityCheck = true;
			m_bSimplifyCheck = true;
		}
	}

	IEnumerator WaitForUpdate()
	{
		yield return new WaitForFixedUpdate();
	}

	public void GetConnectedBoxes( BreakableBox tBox, List<BreakableBox> tBoxes)
	{
		if (tBoxes.Contains(tBox))
			return;
		tBoxes.Add(tBox);
		tBox.RefreshNeighbours();
		for (int i = 0; i < tBox.Neighbours.Count; i++)
			GetConnectedBoxes(tBox.Neighbours [i], tBoxes);
	}

	public void CheckIntegrity()
	{
		m_bIntegrityCheck = false;
		WaitForUpdate();
		if (childs.Count == 0) {
			Deactivate();
			return;
		}
		for (int i = 0; i < childs.Count; i++)
			childs [i].RefreshNeighbours();
		var tConn = new List<BreakableBox>();
		do {
			tConn.Clear();
			GetConnectedBoxes(childs [0], tConn);
			if (tConn.Count < childs.Count) {
				for (int i=0; i<childs.Count; ++i) {
					if (!tConn.Contains(childs [i])) {
						var tDetach = new List<BreakableBox>();
						GetConnectedBoxes(childs [i], tDetach);
						if (!DetachBody(tDetach))
							return;
						break;
					}
				}
			}
		} while (tConn.Count < childs.Count);
		if (Body.isKinematic && Body.mass < 100)
			Body.isKinematic = false;
	}

	void Start()
	{
		Init();
		for (int i = 0; i < childs.Count; i++)
			childs [i].RefreshNeighbours();
		SimplifyObject();
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

	public int BreakCount(float a, float b)
	{
		if (b >= a)
			return 1;
		return (int)((a - (a % b)) / b) + 1;
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
		InitStructureGrid();
		UpdateMass();
	}

	public void InitStructureGrid()
	{
		bool bFirst = true;
		foreach (BreakableBox tBox in childs) {
			if (!bFirst) {
				bounds.Encapsulate (tBox.Collider.bounds);
			} else {
				bounds = tBox.Collider.bounds;
				bFirst = false;
			}
		}
		int iWidth = BreakCount (bounds.size.x, BoxPool.DebrisSize);
		int iHeight = BreakCount (bounds.size.y, BoxPool.DebrisSize);
		StructGrid = new SBox[iWidth, iHeight];
		Vector3 tTopLeft = bounds.center - bounds.extents;
		tTopLeft.x += BoxPool.DebrisSize / 2;
		tTopLeft.y += BoxPool.DebrisSize / 2;
		for (int x=0; x<iWidth; ++x)
			for (int y=0; y<iHeight; ++y) {
				StructGrid[x, y].Box = null;
				StructGrid[x, y].Temperature = 0;
				foreach (BreakableBox tBox in childs) {
					if (tBox.Collider.bounds.Contains(new Vector3(tTopLeft.x + (x * BoxPool.DebrisSize), tTopLeft.y + (y * BoxPool.DebrisSize)))) {
						StructGrid[x, y].Box = tBox;
						break;
					}
				}
			}
	}
	
	void Update()
	{
		if (m_bIntegrityCheck) {
			CheckIntegrity();
		} else {
			if (Time.frameCount % 60 == 0)
				m_bSimplifyCheck = true;
			if (m_bSimplifyCheck) {
				m_fSimplifyTimer += Time.deltaTime;
				if (m_fSimplifyTimer > 0.25f && Body.IsSleeping()) {
					SimplifyObject();
					Body.Sleep();
					m_fSimplifyTimer = 0;
					m_bSimplifyCheck = false;
				}
			}
		}
	}

	bool SimplifyChild(BreakableBox tBox)
	{
		for (int j=0; j<tBox.Neighbours.Count; ++j) {
			BreakableBox tNeighbour = tBox.Neighbours [j];
			if ((tBox.Temperature + tNeighbour.Temperature >= 0.05f) || tBox.Sprite.color != tNeighbour.Sprite.color)
				continue;
			Vector3 tP1 = tBox.transform.localPosition;
			Vector3 tP2 = tNeighbour.transform.localPosition;
			Vector3 tS1 = tBox.transform.localScale;
			Vector3 tS2 = tNeighbour.transform.localScale;
			if (Mathf.Abs(tS1.x - tS2.x) <= 0.05f && Mathf.Abs(tP1.x - tP2.x) <= 0.05f) {
				tBox.transform.localScale = new Vector3(tS1.x, tS1.y + tS2.y, 1);
				tBox.transform.localPosition = new Vector3((tP1.x + tP2.x) / 2, ((tP1.y * tS1.y) + (tP2.y * tS2.y)) / (tS1.y + tS2.y), (tP1.z + tP2.z) / 2);
				tBox.ResetNeighbours();
				tNeighbour.Deactivate();
				tBox.RefreshNeighbours();
				return true;
			}
			if (Mathf.Abs(tS1.y - tS2.y) <= 0.05f && Mathf.Abs(tP1.y - tP2.y) <= 0.05f) {
				tBox.transform.localScale = new Vector3(tS1.x + tS2.x, tS1.y, 1);
				tBox.transform.localPosition = new Vector3(((tP1.x * tS1.x) + (tP2.x * tS2.x)) / (tS1.x + tS2.x), (tP1.y + tP2.y) / 2, (tP1.z + tP2.z) / 2);
				tBox.ResetNeighbours();
				tNeighbour.Deactivate();
				tBox.RefreshNeighbours();
				return true;
			}
		}
		return false;
	}
	
	public void SimplifyObject()
	{
		bool bChanged;
		int iStartIndex = 0;
		do {
			bChanged = false;
			for (int i=iStartIndex; i<childs.Count; ++i) {
				if (childs [i].NeedRefreshNeighbours)
					childs [i].RefreshNeighbours();
				if (SimplifyChild(childs [i])) {
					bChanged = true;
					break;
				}
			}
		} while (bChanged);
		WaitForUpdate();
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
		Body.velocity = Vector2.zero;
		Body.angularVelocity = 0;
		ContainerPool.Instance.PoolObject(gameObject);
	}

	public void DetachBox(BreakableBox tBox)
	{
		WaitForUpdate();
		var tList = new List<BreakableBox>();
		tList.Add(tBox);
		DetachBody(tList);
	}

	public bool DetachBody(List<BreakableBox> tBoxes)
	{
		if (tBoxes.Count == 0)
			return true;
		BreakableContainer tNewContr = ContainerPool.GetContainer();
		if (tNewContr == null)
			return false;
		tNewContr.FractureForce = FractureForce;
		tNewContr.Density = Density;
		tNewContr.MaxHeat = MaxHeat;
		tNewContr.FlameSpread = FlameSpread;
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
		return true;
	}
}
