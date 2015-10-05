using System.Collections.Generic;
using UnityEngine;

public class BreakableBox : MonoBehaviour
{

	public BreakableContainer Container {
		get;
		set;
	}
	public float Mass {
		get;
		set;
	}
	public bool InBody {
		get;
		set;
	}
	protected BoxCollider2D Collider;
	float m_fFractureForce = 1.0f;
	DebrisController m_tDebris;

	public void SetupObject (BreakableContainer tContr, Transform tTransform, Vector3 tTranslate)
	{
		Init (tContr);
		transform.localPosition = tTransform.localPosition;
		transform.localRotation = tTransform.localRotation;
		transform.Translate (tTranslate);
		transform.localScale = tTransform.localScale;
		Mass = transform.localScale.x * transform.localScale.y * Container.Density;
	}

	public void Break (Collision2D tCol = null)
	{
		int iNeededObjects = 1;
		int iBreakX = 1;
		int iBreakY = 1;
		if (CanBreakX ()) {
			iBreakX = 2;
			if (transform.localScale.x * 2 > transform.localScale.y) {
				iBreakX = (int)Mathf.Max (iBreakX, transform.localScale.y / transform.localScale.x);
			}
		}
		if (CanBreakY ()) {
			iBreakY = 2;
			if (transform.localScale.y * 2 > transform.localScale.x) {
				iBreakY = (int)Mathf.Max (iBreakY, transform.localScale.x / transform.localScale.y);
			}
		}

		iNeededObjects *= iBreakX;
		iNeededObjects *= iBreakY;
		iNeededObjects -= 1;

		if (iNeededObjects <= BoxPool.Instance.GetFreePoolSize ()) {
			var tScale = new Vector3 (transform.localScale.x / iBreakX, transform.localScale.y / iBreakY, 0);
			bool bCantBreak = tScale.x <= Container.FractureSize && tScale.y <= Container.FractureSize;
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
						SetupObject (Container, transform, tPosChange);
//						m_fFractureForce /= 1.5f;
						if (bCantBreak) {
							SetDebris ();
						}
					} else {
						tContr = BoxPool.GetBox ();
						if (tContr != null) {
							tContr.transform.parent = transform.parent;
							tContr.SetupObject (Container, transform, -(new Vector3 (x * transform.localScale.x, y * transform.localScale.y, 0)));
							tContr.m_fFractureForce = m_fFractureForce;
							if (bCantBreak) {
								tContr.SetDebris ();
							} else if (tCol != null){
								tContr.BreakAt (tCol);
							}
						}
					}
				}
			}
			if (!bCantBreak && tCol != null)
				BreakAt(tCol);
		}
	}

	protected void BreakAt (Collision2D tCol)
	{
		foreach (ContactPoint2D tPoint in tCol.contacts) {
			Vector3 tBodyPoint = Collider.bounds.ClosestPoint (tPoint.point);
			Vector3 tColliderPoint = tPoint.point;
			if (Vector3.Distance (tBodyPoint, tColliderPoint) < Container.FractureSize / 2.0f) {
				Break (tCol);
			}
		}
	}

	void OnCollisionEnter2D (Collision2D col)
	{
		Vector2 tDeltaV = Container.RigidBody.velocity - Container.Velocity;
		float fForce = Container.RigidBody.mass * tDeltaV.sqrMagnitude;
		if (fForce < Mathf.Epsilon || fForce < m_fFractureForce)
			return;
		BreakAt(col);
		return;
/*

		Debug.Log ("DeltaV: " + tDeltaV + " / EstimatedForce: " + fForce);
		if (!col.rigidbody || col.rigidbody.isKinematic) {
//			if (Container.RigidBody.velocity.sqrMagnitude < 0.1f)
//				return;
//			float fDamage = col.relativeVelocity.sqrMagnitude * fKineticSelf;
//			BreakAt (col);
//			Debug.Break ();
		} else {
			float fKineticSelf = Container.KineticEnergy();
			BreakableContainer tOtherBody = col.gameObject.GetComponent<BreakableContainer> ();
			if (tOtherBody) {
				float fKineticOther = tOtherBody.KineticEnergy ();
				float fDamage;
				fDamage = col.relativeVelocity.sqrMagnitude * (fKineticOther - fKineticSelf) / 100.0f;
				if (Mathf.Abs (fDamage) >= m_fFractureForce) {
					BreakAt (col);
				}
			} else {
//				fDamage = col.relativeVelocity.sqrMagnitude * fKineticSelf / 100.0f;
//				Debug.Log ("Other contact: " + fDamage);
//				if (Mathf.Abs (fDamage) >= m_fFractureForce) {
					BreakAt (col);
//				}
			}
		}
*/
	}

	public void Init (BreakableContainer tContainer)
	{
		Container = tContainer;
		Container.AddChild(this);
		transform.parent = Container.transform;
		m_fFractureForce = Container.FractureForce;
		Mass = transform.localScale.x * transform.localScale.y * Container.Density;
		Collider = gameObject.GetComponent<BoxCollider2D> ();
	}

	public void SetDebris ()
	{
		if (m_tDebris == null) {
			m_tDebris = gameObject.AddComponent<DebrisController> ();
			var tList = new List<BreakableBox> ();
			tList.Add (this);
			Container.DetachBody (tList);
		}
	}

	public bool CanBreakX ()
	{
		return transform.localScale.x > Container.FractureSize;
	}

	public bool CanBreakY ()
	{
		return transform.localScale.y > Container.FractureSize;
	}

	public void Deactivate ()
	{
		m_tDebris = null;
		Container.RemoveChild (this);
		BoxPool.Instance.PoolObject (gameObject);
	}

	void CheckRay (Vector2 tDir)
	{
		tDir = new Vector2((transform.localScale.x + (Container.FractureSize / 2.0f)) * tDir.x,
		                   (transform.localScale.y + (Container.FractureSize / 2.0f)) * tDir.y);
		tDir = transform.TransformDirection(tDir);
		var tStart = new Vector2 (transform.position.x, transform.position.y);
//		Debug.Log("Raycast from: " + tStart + " to: " + tDir);
		Debug.DrawRay (tStart, tDir, Color.green);
		RaycastHit2D[] tHits = Physics2D.RaycastAll (tStart, tDir, tDir.magnitude / 2);
		foreach (RaycastHit2D tHit in tHits) {			                                     
			if (tHit.collider != Collider) {
				BreakableBox tBox = tHit.collider.gameObject.GetComponent<BreakableBox> ();
				if (tBox && tBox.Container == Container && !tBox.InBody) {
//					Debug.Log("Ray Hit");
					tBox.InBody = true;
					tBox.PingNeighbors ();
				}
			}
		}
	}

	public void PingNeighbors ()
	{
		CheckRay (Vector2.up);
		CheckRay (Vector2.down);
		CheckRay (Vector2.left);
		CheckRay (Vector2.right);
//		Debug.Break();
		//		Debug.DrawRay (tOtherCenter, (tCenter - tOtherCenter), Color.green);
//		RaycastHit2D tHit2 = Physics2D.Raycast (tOtherCenter, (tCenter - tOtherCenter));
		
//		if (tHit1.collider != m_tCollider)
//			return false;
//		if (tHit2.collider != tBox.m_tCollider)
//			return false;
//		fDist = Vector2.Distance (tHit1.point, tHit2.point);
//		Debug.Log (fDist);
//		return fDist < Container.FractureSize / 2.0f;

//		foreach (BreakableBox tBox in tOthers) {
//			if (tBox != this && !tBox.InBody) {
//				if (CheckNeighbour (tBox)) {
//					tBox.InBody = true;
//					tBox.PingNeighbors (tOthers);
//				}
//			}
//		}
	}
}
