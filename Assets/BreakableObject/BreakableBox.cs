﻿using System.Collections.Generic;
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
	protected BoxCollider2D m_tCollider;
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

	public void Break ()
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
						m_fFractureForce /= 1.5f;
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
							}
//							if (tCol != null)
//								tContr.BreakAt(tCol);
						}
					}
				}
			}
//			if (tCol != null)
//				BreakAt(tCol);
		}
	}

	protected void BreakAt (Collider2D tCol)
	{
		Vector3 tBodyPoint = m_tCollider.bounds.ClosestPoint (tCol.bounds.center);
		Vector3 tColliderPoint = tCol.bounds.ClosestPoint (tBodyPoint);
		if (Vector3.Distance (tBodyPoint, tColliderPoint) < Container.FractureSize / 2.0f) {
			Break ();
		}
	}

	void OnCollisionEnter2D (Collision2D col)
	{
//		Debug.Log("Collision - " + transform.name + " - " + col.transform.name);
		BreakableContainer tOtherBody = col.gameObject.GetComponent<BreakableContainer> ();
		float fKineticSelf = Container.KineticEnergy ();
		float fKineticOther = (tOtherBody != null) ? tOtherBody.KineticEnergy () : 0;
		float fDamage = col.relativeVelocity.sqrMagnitude * (fKineticOther - fKineticSelf) / 100.0f;
		if (Mathf.Abs (fDamage) >= m_fFractureForce) {
			Break ();
			Container.CheckIntegrity ();
		}
	}

	public void Init (BreakableContainer tContainer)
	{
		Container = tContainer;
		transform.parent = Container.transform;
		m_fFractureForce = Container.FractureForce;
		Mass = transform.localScale.x * transform.localScale.y * Container.Density;
		m_tCollider = gameObject.GetComponent<BoxCollider2D> ();
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

	float CheckDistance (BreakableBox tBox)
	{
		float fDist = float.MaxValue;
//		Physics2D.Raycast()
		return fDist;
	}

	public void PingNeighbors (List<BreakableBox> tOthers)
	{
		foreach (BreakableBox tBox in tOthers) {
			if (tBox != this && !tBox.InBody) {
				if (CheckDistance (tBox) < Container.FractureSize / 2.0f) {
					tBox.InBody = true;
					tBox.PingNeighbors (tOthers);
				}
			}
		}
	}
}
