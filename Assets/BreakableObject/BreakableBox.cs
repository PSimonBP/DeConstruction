using System.Collections.Generic;
using UnityEngine;

public class BreakableBox : MonoBehaviour
{
	public BreakableContainer Container { get; set; }
	public float Mass { get; set; }
	protected BoxCollider2D Collider;
	public List<BreakableBox> Neighbours { get; set; }
	public float Damage { get; set; }

	DebrisController m_tDebris;

	public void Init(BreakableContainer tContr, Transform tTransform = null, Vector3 tTranslate = new Vector3())
	{
		Container = tContr;
		Container.AddChild(this);
		transform.parent = Container.transform;
		Mass = transform.localScale.x * transform.localScale.y * Container.Density;
		Collider = gameObject.GetComponent<BoxCollider2D>();

		if (tTransform != null) {
			transform.localPosition = tTransform.localPosition;
			transform.localRotation = tTransform.localRotation;
			transform.Translate(tTranslate);
			transform.localScale = tTransform.localScale;
		}
		Mass = transform.localScale.x * transform.localScale.y * Container.Density;
	}
	
	public void Break()
	{
		if (m_tDebris) {
			return;
		}
		int iBreakX = (transform.localScale.x > Container.FractureSize) ? 2 : 1;
		int iBreakY = (transform.localScale.y > Container.FractureSize) ? 2 : 1;
		int iNeededObjects = iBreakX * iBreakY - 1;

		if (iNeededObjects == 0) {
			SetDebris();
			Damage -= Container.FractureForce;
			if (Damage < 0) {
				Damage = 0;
			} else {
				var tBoxes = new List<BreakableBox>();
				GetNeighbours(tBoxes);
				if (tBoxes.Count > 0) {
					Damage /= Mathf.Min(2, tBoxes.Count);
					foreach (BreakableBox tBox in tBoxes) {
						tBox.AddDamage(Damage);
					}
				}
				Container.ChildsToRemove.Add(this);
			}
			return;
		}

		if (iNeededObjects <= BoxPool.Instance.GetFreePoolSize()) {
			var tScale = new Vector3(transform.localScale.x / iBreakX, transform.localScale.y / iBreakY, 0);
			Damage -= Container.FractureForce;
			if (Damage < 0) {
				Damage = 0;
			} else {
				Damage /= 4.0f;
			}
			BreakableBox tContr = null;
			for (int x = 0; x < iBreakX; ++x) {
				for (int y = 0; y < iBreakY; ++y) {
					if (x == 0 && y == 0) {
						Vector3 tPosChange = tScale / 2;
						if (iBreakX == 1) {
							tPosChange.x = 0;
						}
						if (iBreakY == 1) {
							tPosChange.y = 0;
						}
						transform.localScale = tScale;
						Init(Container, transform, tPosChange);
					} else {
						tContr = BoxPool.GetBox();
						if (tContr != null) {
							tContr.transform.parent = transform.parent;
							tContr.Init(Container, transform, -(new Vector3(x * transform.localScale.x, y * transform.localScale.y, 0)));
							tContr.Damage = Damage;
							if (tContr.Damage >= Container.FractureForce) {
								tContr.Break();
							}
						}
					}
				}
			}
			if (Damage >= Container.FractureForce) {
				Break();
			}
		}
	}
	
	public void AddDamage(float fDamage)
	{
		if (m_tDebris != null) {
			return;
		}
		Damage += fDamage;
		if (Damage < Mathf.Epsilon || Damage < Container.FractureForce) {
			return;
		}
		Break();
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		if (m_tDebris != null) {
			return;
		}
		float fForce = 0;
		if (!Container.Body.isKinematic) {
			fForce += Container.Body.mass * (Container.Body.velocity - Container.Velocity).sqrMagnitude;
		}
		if (col.rigidbody && !col.rigidbody.isKinematic) {
			fForce += col.relativeVelocity.sqrMagnitude * col.rigidbody.mass;
		}
		AddDamage(fForce);
	}
	
	public void SetDebris()
	{
		if (m_tDebris == null) {
			m_tDebris = gameObject.AddComponent<DebrisController>();
		}
	}
	
	public void Deactivate()
	{
		Damage = 0;
		m_tDebris = null;
		Container.RemoveChild(this);
		BoxPool.Instance.PoolObject(gameObject);
	}
	
	List<BreakableBox> CheckRay(Vector2 tDir, List<BreakableBox> tBoxes)
	{
		tDir = new Vector2((transform.localScale.x + (Container.FractureSize)) * tDir.x,
		                   (transform.localScale.y + (Container.FractureSize)) * tDir.y);
		tDir = transform.TransformDirection(tDir);
		var tStart = new Vector2(transform.position.x, transform.position.y);
		RaycastHit2D[] tHits = Physics2D.RaycastAll(tStart, tDir, tDir.magnitude / 2);
		foreach (RaycastHit2D tHit in tHits) {			                                     
			if (tHit.collider != Collider) {
				BreakableBox tBox = tHit.collider.gameObject.GetComponent<BreakableBox>();
				if (tBox && tBox.Container == Container) {
					tBoxes.Add(tBox);
				}
			}
		}
		return tBoxes;
	}

	void GetNeighbours(List<BreakableBox> tBoxes)
	{
		CheckRay(Vector2.up, tBoxes);
		CheckRay(Vector2.down, tBoxes);
		CheckRay(Vector2.left, tBoxes);
		CheckRay(Vector2.right, tBoxes);
	}

	void CheckNeighbours(ICollection<BreakableBox> tBoxes)
	{
		var tBoxList = new List<BreakableBox>();
		GetNeighbours(tBoxList);
		foreach (BreakableBox tBox in tBoxList) {
			tBoxes.Add(tBox);
		}
	}
	
	public void PingNeighbors()
	{
		var tBoxes = new List<BreakableBox>();
		CheckNeighbours(tBoxes);
		foreach (BreakableBox tBox in tBoxes) {
			tBox.PingNeighbors();
		}
	}

}
