using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBox : MonoBehaviour
{
	public BreakableContainer Container { get; set; }
	public BoxCollider2D Collider;
	List<BreakableBox> neighbours = new List<BreakableBox>();
	public List<BreakableBox> Neighbours { get { return neighbours; } set { neighbours = value; } }
	public bool NeedRefreshNeighbours { get; set; }
	public float Temperature;

	float fTempChange;
	public bool Kinematic { get; set; }

	DebrisController m_tDebris;
	public bool Debris { get { return m_tDebris != null; } }
	public SpriteRenderer Sprite { get; set; }
	Color OriginalColor;
	
	public int Width { get { return Container.BreakCount(transform.localScale.x, BoxPool.DebrisSize); } }
	public int Height { get { return Container.BreakCount(transform.localScale.y, BoxPool.DebrisSize); } }
	public int Size	{ get { return Width * Height; } }


	public void Update()
	{
		if (Debris || Container == null)
			return;

		if (Size == 1) {
			Detach();
			return;
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

			int iSize = Size;
			if (Temperature >= Container.MaxHeat / iSize) {
				Break();
				return;
			}
			if (neighbours.Count > 0) {
				float fSumSize = 0;
				int iColderNeighbourCount = 0;
				for (int i=0; i<neighbours.Count; ++i) {
					if (neighbours [i].Temperature < Temperature) {
						fSumSize += neighbours [i].Size;
						iColderNeighbourCount++;
					}
				}
				for (int i=0; i<neighbours.Count; ++i) {
					if (neighbours [i].Temperature < Temperature) {
						float fNewTemp = neighbours [i].Temperature + (Temperature - neighbours [i].Temperature) * (neighbours [i].Size / fSumSize) * Container.HeatSpread;
						if (fNewTemp < Container.MaxHeat * 0.5f)
							neighbours [i].Temperature = fNewTemp;
					}
				}
				if (iColderNeighbourCount != 0)
					Temperature *= 1 - Container.HeatSpread;
			}

		}
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
							if (bRecursive && Size > 1 && tContr.Collider == Physics2D.OverlapPoint(tCollisionPoint))
								tContr.Break(tCollisionPoint, bRecursive);
						}
					}
				}
			}
			if (bRecursive && Size > 1 && Collider == Physics2D.OverlapPoint(tCollisionPoint))
				Break(tCollisionPoint, bRecursive);
		}
	}
		
	void OnCollisionEnter2D(Collision2D col)
	{
		if (m_tDebris != null || !col.enabled || col.transform.parent == transform.parent)
			return;
		var tFire = col.gameObject.GetComponent<FireController>();
		if (tFire && Container.MaxHeat > 0)
			Temperature += 5 / transform.localScale.magnitude;

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
		if (!col.enabled || col.collider.GetType() != typeof(CircleCollider2D))
			return;
		if (Container.MaxHeat > 0)
			Temperature += 5 / transform.localScale.magnitude;
	}

	void Detach()
	{
		if (!Debris)
			m_tDebris = gameObject.AddComponent<DebrisController>();
		if (Container.Childs.Count == 1)
			return;
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
				var tDrop = FirePool.GetFire();
				if (tDrop != null) {
					tDrop.transform.position = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
					tDrop.MaxLife = Random.Range(0.5f, 2.5f);
					tDrop.transform.SetParent(FirePool.Instance.transform);
					tDrop.Init();
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
		int iWidth = Container.BreakCount (Container.bounds.size.x, BoxPool.DebrisSize);
		int iHeight = Container.BreakCount (Container.bounds.size.y, BoxPool.DebrisSize);
		for (int x = 0; x < iWidth; ++x)
			for (int y = 0; y < iHeight; ++y) {
				if (Container.StructGrid [x, y].Box == this) {
					if(y>0) {
						if (x > 0 && Container.StructGrid [x-1, y-1].Box != this && !tBoxes.Contains(Container.StructGrid[x-1, y-1].Box))	tBoxes.Add(Container.StructGrid[x-1, y-1].Box);
						if (Container.StructGrid [x, y-1].Box != this && !tBoxes.Contains(Container.StructGrid[x, y-1].Box))	tBoxes.Add(Container.StructGrid[x, y-1].Box);
						if (x < iWidth-1 && Container.StructGrid [x+1, y-1].Box != this && !tBoxes.Contains(Container.StructGrid[x+1, y-1].Box))	tBoxes.Add(Container.StructGrid[x+1, y-1].Box);
					}
					if (x > 0 && Container.StructGrid [x-1, y].Box != this && !tBoxes.Contains(Container.StructGrid[x-1, y].Box))	tBoxes.Add(Container.StructGrid[x-1, y].Box);
					if (Container.StructGrid [x, y].Box != this && !tBoxes.Contains(Container.StructGrid[x, y].Box))	tBoxes.Add(Container.StructGrid[x, y].Box);
					if (x < iWidth-1 && Container.StructGrid [x+1, y].Box != this && !tBoxes.Contains(Container.StructGrid[x+1, y].Box))	tBoxes.Add(Container.StructGrid[x+1, y].Box);
					if(y<iHeight - 1) {
						if (x > 0 && Container.StructGrid [x-1, y+1].Box != this && !tBoxes.Contains(Container.StructGrid[x-1, y+1].Box))	tBoxes.Add(Container.StructGrid[x-1, y+1].Box);
						if (Container.StructGrid [x, y+1].Box != this && !tBoxes.Contains(Container.StructGrid[x, y+1].Box))	tBoxes.Add(Container.StructGrid[x, y+1].Box);
						if (x < iWidth-1 && Container.StructGrid [x+1, y+1].Box != this && !tBoxes.Contains(Container.StructGrid[x+1, y+1].Box))	tBoxes.Add(Container.StructGrid[x+1, y+1].Box);
					}
				}
			}			
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
}
