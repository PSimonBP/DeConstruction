using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBox : MonoBehaviour
{
	public BreakableContainer Container { get; set; }
	protected BoxCollider2D Collider;
	List<BreakableBox> neighbours = new List<BreakableBox>();
	public List<BreakableBox> Neighbours { get { return neighbours; } set { neighbours = value; } }
	public bool NeedRefreshNeighbours { get; set; }
	public float Temperature;
	public int Size;

	float fTempChange;
	public bool Kinematic { get; set; }

	DebrisController m_tDebris;
	public bool Debris { get { return m_tDebris != null; } }
	public SpriteRenderer Sprite { get; set; }
	Color OriginalColor;

	public void Update()
	{
		if (Container == null)
			return;

		if (!Debris && GetSize() == 1 && Container.Childs.Count <= 4) {
			m_tDebris = gameObject.AddComponent<DebrisController>();
/*			Detach();
			return;
*/
		}
		
		/*		if (Container.Body.IsSleeping ())
			Sprite.color = Color.gray;
		else
			Sprite.color = OriginalColor;
*/
		var tNewColor = (Temperature < 0.05f) ? OriginalColor : Color.Lerp(OriginalColor, Color.red, Temperature / Container.MaxHeat);
		if (Sprite.color != tNewColor)
			Sprite.color = tNewColor;

		if (Temperature < 0.05f)
			Temperature = 0;
		else {
			Temperature *= 0.99999f;

			int iSize = GetSize();
			Size = iSize;
			if (Temperature >= Container.MaxHeat / iSize) {
				Temperature = Container.MaxHeat / iSize;
				if (Container.MaxHeat > 0) {
					if (iSize == 1) {
						Detach();
					} else {
						Break();
					}
				}
				return;
			}
			if (neighbours.Count > 0) {
				float fSumSize = 0;
				int iColderNeighbourCount = 0;
				for (int i=0; i<neighbours.Count; ++i) {
					if (neighbours [i].Temperature < Temperature) {
						fSumSize += neighbours [i].GetSize();
						iColderNeighbourCount++;
					}
				}
				for (int i=0; i<neighbours.Count; ++i) {
					if (neighbours [i].Temperature < Temperature) {
						float fNewTemp = neighbours [i].Temperature + (Temperature - neighbours [i].Temperature) * (neighbours [i].GetSize() / fSumSize) * Container.HeatSpread;
						if (fNewTemp < Container.MaxHeat * 0.5f)
							neighbours [i].Temperature = fNewTemp;
					}
				}
				if (iColderNeighbourCount != 0)
					Temperature *= 1 - Container.HeatSpread;
			}
		}
	}

	int BreakCount(float a, float b)
	{
		if (b >= a)
			return 1;
		return (int)((a - (a % b)) / b) + 1;
	}

	public int GetSize()
	{
		return BreakCount(transform.localScale.x, BoxPool.DebrisSize) * BreakCount(transform.localScale.y, BoxPool.DebrisSize);
	}

	public void Init(BreakableContainer tContr, Transform tTransform = null, Vector3 tTranslate = new Vector3())
	{
		Sprite = GetComponent<SpriteRenderer>();
		OriginalColor = Sprite.color;
		ResetNeighbours();
		Kinematic = false;
		Collider = gameObject.GetComponent<BoxCollider2D>();
		Container = tContr;
		Container.AddChild(this);
		if (tTransform != null) {
			transform.localPosition = tTransform.localPosition;
			transform.localRotation = tTransform.localRotation;
			transform.Translate(tTranslate);
			transform.localScale = tTransform.localScale;
		}
	}
	
	public void Break(Vector2 tCollisionPoint = new Vector2(), bool bRecursive = false)
	{
		int iBreakX = (transform.localScale.x > BoxPool.DebrisSize) ? 2 : 1;
		int iBreakY = (transform.localScale.y > BoxPool.DebrisSize) ? 2 : 1;

		if ((iBreakX * iBreakY - 1) <= BoxPool.Instance.GetFreePoolSize()) {
			ResetNeighbours();
			var tScale = new Vector3(transform.localScale.x / iBreakX, transform.localScale.y / iBreakY, 1);
			BreakableBox tContr = null;
			Vector3 tPosChange = tScale / 2;
			for (int x = 0; x < iBreakX; ++x) {
				for (int y = 0; y < iBreakY; ++y) {
					if (x == 0 && y == 0) {
						if (iBreakX == 1)
							tPosChange.x = 0;
						if (iBreakY == 1)
							tPosChange.y = 0;
						transform.localScale = tScale;
						transform.Translate(tPosChange);
					} else {
						tContr = BoxPool.GetBox();
						if (tContr != null) {
							tContr.Temperature = Temperature;
							tContr.Init(Container, transform, -(new Vector3(x * transform.localScale.x, y * transform.localScale.y, 0)));
							tContr.Sprite.color = Sprite.color;
							tContr.OriginalColor = OriginalColor;
							if (bRecursive && GetSize() > 1 && tContr.Collider == Physics2D.OverlapPoint(tCollisionPoint))
								tContr.Break(tCollisionPoint, bRecursive);
						}
					}
				}
			}
			if (bRecursive && GetSize() > 1 && Collider == Physics2D.OverlapPoint(tCollisionPoint))
				Break(tCollisionPoint, bRecursive);
		}
	}

	int GetFractureCount()
	{
		return (int)(((transform.localScale.x / BoxPool.DebrisSize)) * (transform.localScale.y / BoxPool.DebrisSize));
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		if (m_tDebris != null)
			return;
		if (!col.enabled)
			return;
		if (col.transform.parent == transform.parent)
			return;
		var tWater = col.gameObject.GetComponent<FireController>();
		if (tWater) {
			Temperature += 5 / transform.localScale.magnitude;
//			return;
		}

		float fForce = 0;
		if (!Container.Body.isKinematic)
			fForce += Container.Body.mass * (Container.Body.velocity - Container.Velocity).sqrMagnitude;
		if (col.rigidbody && !col.rigidbody.isKinematic)
			fForce += col.relativeVelocity.sqrMagnitude * col.rigidbody.mass;
		if (fForce >= Container.FractureForce)
			Break(col.contacts [0].point, true);
	}

	void OnCollisionStay2D(Collision2D col)
	{
		if (!col.enabled)
			return;
		if (col.collider.GetType() != typeof(CircleCollider2D))
			return;
//		var tWater = col.gameObject.GetComponent<WaterController> ();
//		if (tWater) {
		Temperature += 5 / transform.localScale.magnitude;
//		}
	}

	void Detach()
	{
		for (int i = 0; i < neighbours.Count; i++)
			neighbours [i].Neighbours.Remove(this);
		ResetNeighbours();
		Container.DetachBox(this);
	}

	IEnumerator WaitForUpdate()
	{
		yield return new WaitForFixedUpdate();
	}

	public void Deactivate()
	{
		WaitForUpdate();
		ResetNeighbours();
		if (Temperature >= Container.MaxHeat/* * 0.25f*/) {
			for (int i=0; i<Container.FlameSpread; ++i) {
				var tDrop = FirePool.GetWater();
				if (tDrop != null) {
					tDrop.transform.position = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
					tDrop.MaxLife = Random.Range(0.5f, 2.5f);
					tDrop.transform.SetParent(FirePool.Instance.transform);
					Rigidbody2D tRB = tDrop.RigidBody;
					tRB.AddRelativeForce(new Vector2(Random.Range(-1 * tRB.mass, 1 * tRB.mass), Random.Range(-1 * tRB.mass, 1 * tRB.mass)), ForceMode2D.Impulse);
				}		
			}
		}

		Sprite.color = Color.white;
		Temperature = 0;
		m_tDebris = null;
		Container.RemoveChild(this);
		BoxPool.PoolBox(gameObject);
	}
	
	List<BreakableBox> CheckRay(Vector2 tStart, Vector2 tDir, List<BreakableBox> tBoxes)
	{
		tDir = new Vector2((BoxPool.DebrisSize / 4) * tDir.x,
		                   (BoxPool.DebrisSize / 4) * tDir.y);
		tDir = transform.TransformDirection(tDir);
//		Debug.DrawRay(tStart, tDir, Color.yellow);
//		Debug.Break();
		LayerMask tMask = ~0;
		RaycastHit2D tHit = Physics2D.Raycast(tStart, tDir, Mathf.Epsilon, tMask);
		if (tHit.collider != null && tHit.collider != Collider) {
			BreakableBox tBox = tHit.collider.gameObject.GetComponent<BreakableBox>();
			if (tBox) {
				if (tBox.Container == Container && !tBoxes.Contains(tBox))
					tBoxes.Add(tBox);
			} else if (tHit.rigidbody == null || tHit.rigidbody.isKinematic) {
				Kinematic = true;
			}
		}
		return tBoxes;
	}

	public void ResetNeighbours()
	{
		for (int i = 0; i < neighbours.Count; i++) {
			BreakableBox tBox = neighbours [i];
			tBox.Neighbours.Remove(this);
			tBox.NeedRefreshNeighbours = true;
		}
		neighbours.Clear();
		NeedRefreshNeighbours = true;
	}

	void FindNeighbours(List<BreakableBox> tBoxes)
	{
		float fGap = BoxPool.DebrisSize / 8;
		var tStart = new Vector2(transform.position.x, transform.position.y);
		Vector2 tDir = transform.TransformDirection(new Vector3(0, (transform.localScale.y / 2) + fGap, 0));
		CheckRay(tStart + tDir, Vector2.up, tBoxes);
		tDir = transform.TransformDirection(new Vector3(0, (-transform.localScale.y / 2) - fGap, 0));
		CheckRay(tStart + tDir, Vector2.down, tBoxes);
		tDir = transform.TransformDirection(new Vector3((-transform.localScale.x / 2) - fGap, 0, 0));
		CheckRay(tStart + tDir, Vector2.left, tBoxes);
		tDir = transform.TransformDirection(new Vector3((transform.localScale.x / 2) + fGap, 0, 0));
		CheckRay(tStart + tDir, Vector2.right, tBoxes);
	}

	public void RefreshNeighbours()
	{
		if (!NeedRefreshNeighbours)
			return;
		FindNeighbours(Neighbours);
		NeedRefreshNeighbours = false;
		var tList = neighbours.ToArray();
		for (int i = 0; i < tList.Length; i++) {
			BreakableBox tBox = tList [i];
			if (!tBox.Neighbours.Contains(this)) {
				tBox.NeedRefreshNeighbours = true;
				tBox.Neighbours.Add(this);
			}
		}
	}

	public void GetConnectedBoxes(List<BreakableBox> tBoxes)
	{
		if (tBoxes.Contains(this))
			return;
		tBoxes.Add(this);
		RefreshNeighbours();
		for (int i = 0; i < Neighbours.Count; i++)
			Neighbours [i].GetConnectedBoxes(tBoxes);
	}
}
