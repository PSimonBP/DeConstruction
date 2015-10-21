﻿using System.Collections;
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
		UpdateMass();
	}
}
