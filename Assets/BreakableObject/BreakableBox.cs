using System.Collections.Generic;
using UnityEngine;

public class BreakableBox : MonoBehaviour
{
	public BreakableContainer Container { get; set; }
	public float Mass { get; set; }
	protected BoxCollider2D Collider;
	List<BreakableBox> neighbours = new List<BreakableBox>();
	public List<BreakableBox> Neighbours { get { return neighbours; } set { neighbours = value; } }
	public bool NeedRefreshNeighbours { get; set; }
	public float Damage { get; set; }
	public bool Kinematic { get; set; }

	DebrisController m_tDebris;
	public bool Debris { get { return m_tDebris != null; } }
	SpriteRenderer Sprite = null;

	public void Update()
	{
		if (Debris)
			Sprite.color = Color.red;
		else if (Container.DebugDraw) {
			Sprite.color = Color.green;
		} else {
			float fColor = Mathf.Clamp(transform.localScale.x * transform.localScale.y * 5, 0.2f, 1);
			Sprite.color = new Color(fColor, fColor, fColor, 1);
		}
	}

	public void Init(BreakableContainer tContr, Transform tTransform = null, Vector3 tTranslate = new Vector3())
	{
		Sprite = GetComponent<SpriteRenderer>();
		ResetNeighbours();
		Kinematic = false;
		Collider = gameObject.GetComponent<BoxCollider2D>();
		Container = tContr;
		Container.AddChild(this);
		Mass = transform.localScale.x * transform.localScale.y * Container.Density;

		if (tTransform != null) {
			transform.localPosition = tTransform.localPosition;
			transform.localRotation = tTransform.localRotation;
			transform.Translate(tTranslate);
			transform.localScale = tTransform.localScale;

			if (Damage > Container.FractureForce) {
//				RefreshNeighbours();
				if (transform.localScale.x <= Container.FractureSize && transform.localScale.y <= Container.FractureSize) {
					Damage -= Container.FractureForce;
					if (Damage < 0)
						Damage = 0;
					SetDebris();
					Damage = 0;
				} else {
					Break();
				}
			}
		}
		Mass = transform.localScale.x * transform.localScale.y * Container.Density;
	}
	
	public void Break()
	{
		int iBreakX = (transform.localScale.x > Container.FractureSize) ? 2 : 1;
		int iBreakY = (transform.localScale.y > Container.FractureSize) ? 2 : 1;

		if ((iBreakX * iBreakY - 1) <= BoxPool.Instance.GetFreePoolSize()) {
			ResetNeighbours();
			var tScale = new Vector3(transform.localScale.x / iBreakX, transform.localScale.y / iBreakY, 0);
			Damage -= Container.FractureForce;
			if (Damage < 0)
				Damage = 0;
			else
				Damage /= iBreakX * iBreakY;
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
							tContr.Damage = Damage;
							tContr.Init(Container, transform, -(new Vector3(x * transform.localScale.x, y * transform.localScale.y, 0)));
						}
					}
				}
			}
			if (Damage >= Container.FractureForce)
				Init(Container, transform, Vector2.zero);
		}
	}

	int GetFractureCount()
	{
		return (int)(((transform.localScale.x / Container.FractureSize)) * (transform.localScale.y / Container.FractureSize));
	}

	public void AddDamage(float fDamage, List<BreakableBox> tAdded = null)
	{
		if (m_tDebris != null)
			return;
		
		bool bApply = tAdded == null;
		if (tAdded == null)
			tAdded = new List<BreakableBox>();
		tAdded.Add(this);
		var tNewNeighbours = new List<BreakableBox>();
		if (neighbours.Count > 0) {
			for (int i = 0; i < neighbours.Count; i++) {
				BreakableBox tBox = neighbours [i];
				if (!tBox.Debris && !tAdded.Contains(tBox))
					tNewNeighbours.Add(tBox);
			}
		}

		if (tNewNeighbours.Count == 0)
			Damage += fDamage;
		else {
			Damage += fDamage * 0.9f;
			if (fDamage >= Container.FractureForce / 10)
				for (int i = 0; i < tNewNeighbours.Count; i++)
					tNewNeighbours [i].AddDamage((fDamage * 0.1f) / tNewNeighbours.Count, tAdded);
		}

		if (bApply)
			for (int i = 0; i < tAdded.Count; i++)
				tAdded [i].ApplyDamage();

	}

	public void ApplyDamage()
	{
		if (Damage < Container.FractureForce)
			return;
		Break();
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		if (m_tDebris != null)
			return;
		if (col.transform.parent == transform.parent)
			return;
		float fForce = 0;
		if (!Container.Body.isKinematic)
			fForce += Container.Body.mass * (Container.Body.velocity - Container.Velocity).sqrMagnitude;
		if (col.rigidbody && !col.rigidbody.isKinematic)
			fForce += col.relativeVelocity.sqrMagnitude * col.rigidbody.mass;
		AddDamage(fForce);
	}
	
	public void SetDebris()
	{
		if (m_tDebris == null) {
			m_tDebris = gameObject.AddComponent<DebrisController>();
			for (int i = 0; i < neighbours.Count; i++)
				neighbours [i].Neighbours.Remove(this);
			ResetNeighbours();
			Container.DetachBox(this);
		}
	}
	
	public void Deactivate()
	{
		ResetNeighbours();
		Damage = 0;
		m_tDebris = null;
		Container.RemoveChild(this);
		BoxPool.Instance.PoolObject(gameObject);
	}
	
	List<BreakableBox> CheckRay(Vector2 tDir, List<BreakableBox> tBoxes)
	{
		tDir = new Vector2((transform.localScale.x + (Container.FractureSize / 2)) * tDir.x,
		                   (transform.localScale.y + (Container.FractureSize / 2)) * tDir.y);
		tDir = transform.TransformDirection(tDir);
		var tStart = new Vector2(transform.position.x, transform.position.y);
		RaycastHit2D[] tHits = Physics2D.RaycastAll(tStart, tDir, tDir.magnitude / 2);
		for (int i = 0; i < tHits.Length; i++) {
			if (tHits [i].collider != Collider) {
				BreakableBox tBox = tHits [i].collider.gameObject.GetComponent<BreakableBox>();
				if (tBox) {
					if (tBox.Container == Container && !tBoxes.Contains(tBox))
						tBoxes.Add(tBox);
				} else if (tHits [i].rigidbody == null || tHits [i].rigidbody.isKinematic) {
					Kinematic = true;
				}
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
		CheckRay(Vector2.up, tBoxes);
		CheckRay(Vector2.down, tBoxes);
		CheckRay(Vector2.left, tBoxes);
		CheckRay(Vector2.right, tBoxes);
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
//			tBox.RefreshNeighbours();
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
