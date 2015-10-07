using System.Collections.Generic;
using UnityEngine;

public class BreakableBox : MonoBehaviour
{
	public BreakableContainer Container { get; set; }
	public float Mass { get; set; }
	protected BoxCollider2D Collider;
	List<BreakableBox> neighbours = new List<BreakableBox> ();
	public List<BreakableBox> Neighbours { get { return neighbours; } set { neighbours = value; } }
	public bool NeedRefreshNeighbours { get; set; }
	public float Damage { get; set; }

	DebrisController m_tDebris;
	public bool Debris { get { return m_tDebris != null; } }
	public bool DebugDraw { get; set; }
	public void Update ()
	{
		if (DebugDraw) {
			GetComponent<SpriteRenderer> ().color = Color.red;
		} else {
			GetComponent<SpriteRenderer> ().color = Color.white;
		}
	}

	public void Init (BreakableContainer tContr, Transform tTransform = null, Vector3 tTranslate = new Vector3 ())
	{
		NeedRefreshNeighbours = true;
		neighbours.Clear ();
		Container = tContr;
		Container.AddChild (this);
		transform.parent = Container.transform;
		Mass = transform.localScale.x * transform.localScale.y * Container.Density;
		Collider = gameObject.GetComponent<BoxCollider2D> ();

		if (tTransform != null) {
			transform.localPosition = tTransform.localPosition;
			transform.localRotation = tTransform.localRotation;
			transform.Translate (tTranslate);
			transform.localScale = tTransform.localScale;

			if (Damage > Container.FractureForce) {
				RefreshNeighbours ();
				if (transform.localScale.x <= Container.FractureSize && transform.localScale.y <= Container.FractureSize) {
					Damage -= Container.FractureForce;
					if (Damage < 0) {
						Damage = 0;
					} else {
/*						if (neighbours.Count > 0) {
							Damage /= Mathf.Min (2, neighbours.Count);
							var tNeighbourList = new List<BreakableBox> (neighbours);
							foreach (BreakableBox tBox in tNeighbourList) {
								tBox.AddDamage (Damage);
							}
						}
*/
					}
					SetDebris ();
					Damage = 0;
				} //else {
//					Break();
//				}
			}
		}
		Mass = transform.localScale.x * transform.localScale.y * Container.Density;
	}
	
	public void Break ()
	{
		int iBreakX = (transform.localScale.x > Container.FractureSize) ? 2 : 1;
		int iBreakY = (transform.localScale.y > Container.FractureSize) ? 2 : 1;

		if ((iBreakX * iBreakY - 1) <= BoxPool.Instance.GetFreePoolSize ()) {
			RefreshNeighbours ();
			var tScale = new Vector3 (transform.localScale.x / iBreakX, transform.localScale.y / iBreakY, 0);
			Damage -= Container.FractureForce;
			if (Damage < 0) {
				Damage = 0;
			} else {
				Damage /= iBreakX * iBreakY;
			}
			BreakableBox tContr = null;
			Vector3 tPosChange = tScale / 2;
			for (int x = 0; x < iBreakX; ++x) {
				for (int y = 0; y < iBreakY; ++y) {
					if (x == 0 && y == 0) {
						if (iBreakX == 1) {
							tPosChange.x = 0;
						}
						if (iBreakY == 1) {
							tPosChange.y = 0;
						}
						transform.localScale = tScale;
						transform.Translate (tPosChange);
					} else {
						tContr = BoxPool.GetBox ();
						if (tContr != null) {
							tContr.transform.parent = transform.parent;
							tContr.Damage = Damage;
							tContr.Init (Container, transform, -(new Vector3 (x * transform.localScale.x, y * transform.localScale.y, 0)));
						}
					}
				}
			}
			if (Damage >= Container.FractureForce) {
				Init (Container, transform, Vector2.zero);
			}
		}
	}
	
	public void AddDamage (float fDamage)
	{
		if (m_tDebris != null) {
			return;
		}
		Damage += fDamage;
		if (Damage < Mathf.Epsilon || Damage < Container.FractureForce) {
			return;
		}
		Break ();
	}

	void OnCollisionEnter2D (Collision2D col)
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
		AddDamage (fForce);
	}
	
	public void SetDebris ()
	{
		if (m_tDebris == null) {
			m_tDebris = gameObject.AddComponent<DebrisController> ();
			foreach (BreakableBox tBox in neighbours) {
				tBox.Neighbours.Remove (this);
			}
			Neighbours.Clear ();
			Container.DetachBox (this);
		}
	}
	
	public void Deactivate ()
	{
		ResetNeighbours ();
		Damage = 0;
		m_tDebris = null;
		Container.RemoveChild (this);
		BoxPool.Instance.PoolObject (gameObject);
	}
	
	List<BreakableBox> CheckRay (Vector2 tDir, List<BreakableBox> tBoxes)
	{
		tDir = new Vector2 ((transform.localScale.x + (Container.FractureSize / 2)) * tDir.x,
		                   (transform.localScale.y + (Container.FractureSize / 2)) * tDir.y);
		tDir = transform.TransformDirection (tDir);
		var tStart = new Vector2 (transform.position.x, transform.position.y);
		RaycastHit2D[] tHits = Physics2D.RaycastAll (tStart, tDir, tDir.magnitude / 2);
		foreach (RaycastHit2D tHit in tHits) {			                                     
			if (tHit.collider != Collider) {
				BreakableBox tBox = tHit.collider.gameObject.GetComponent<BreakableBox> ();
				if (tBox && tBox.Container == Container) {
					if (!tBoxes.Contains (tBox)) {
						tBoxes.Add (tBox);
					}
				}
			}
		}
		return tBoxes;
	}

	public void ResetNeighbours ()
	{
		foreach (BreakableBox tBox in neighbours) {
			tBox.Neighbours.Remove (this);
			tBox.NeedRefreshNeighbours = true;
		}
		neighbours.Clear ();
		NeedRefreshNeighbours = true;
	}

	void FindNeighbours (List<BreakableBox> tBoxes)
	{
		CheckRay (Vector2.up, tBoxes);
		CheckRay (Vector2.down, tBoxes);
		CheckRay (Vector2.left, tBoxes);
		CheckRay (Vector2.right, tBoxes);
	}

	public void RefreshNeighbours ()
	{
		if (!NeedRefreshNeighbours) {
			return;
		}
//		Neighbours.Clear();
		FindNeighbours (Neighbours);
		NeedRefreshNeighbours = false;
		var tList = neighbours.ToArray ();
		foreach (BreakableBox tBox in tList) {
			if (!tBox.Neighbours.Contains (this)) {
				tBox.NeedRefreshNeighbours = true;
				tBox.Neighbours.Add (this);
			}
			tBox.RefreshNeighbours ();
		}
	}

	public void GetConnectedBoxes (List<BreakableBox> tBoxes)
	{
		if (tBoxes.Contains (this)) {
			return;
		}
		tBoxes.Add (this);
		foreach (BreakableBox tBox in Neighbours) {
			tBox.GetConnectedBoxes (tBoxes);
		}
	}
}
