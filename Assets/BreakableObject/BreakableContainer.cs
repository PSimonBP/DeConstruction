using System.Collections.Generic;
using UnityEngine;

public class BreakableContainer : MonoBehaviour
{
	public float Density = 10.0f;
	public float FractureSize = 0.2f;
	public float FractureForce = 1.0f;
	
	Vector3 m_tVelocity;
	float m_tAngVelocity;
	Rigidbody2D m_tRigidBody;
	List<BreakableBox> m_tChilds = new List<BreakableBox>();

	public void AddChild(BreakableBox tBox)
	{
		m_tChilds.Add(tBox);
	}
	public void RemoveChild(BreakableBox tBox)
	{
		m_tChilds.Remove(tBox);
		if (m_tChilds.Count == 0)
			Deactivate();
	}

	public void SetupObject(Vector3 tPosition, Quaternion tRotation, Vector3 tTranslate, Vector3 tScale, float fMass, Vector3 tVel, float fAngVel)
	{
		Init();
		transform.position = tPosition;
		transform.rotation = tRotation;
		transform.Translate(tTranslate);
		transform.localScale = tScale;
		m_tRigidBody.mass = fMass;
		m_tRigidBody.velocity = tVel;
		m_tRigidBody.angularVelocity = fAngVel;
		m_tRigidBody.WakeUp();
	}

	void Start()
	{
		Init();
	}
	
	public void Wakeup()
	{
		m_tRigidBody.WakeUp();
	}
	
	public void Init()
	{
		m_tChilds.Clear();
		m_tChilds = new List<BreakableBox>(GetComponentsInChildren<BreakableBox>());
		foreach (BreakableBox tBox in m_tChilds) {
			tBox.Init(this);
		}
		m_tRigidBody = gameObject.GetComponent<Rigidbody2D>();
//		m_tCollider = gameObject.GetComponent<BoxCollider2D>();
	}
	
	void FixedUpdate()
	{
		m_tVelocity = m_tRigidBody.velocity;
		m_tAngVelocity = m_tRigidBody.angularVelocity;
	}
	
	public float KineticEnergy()
	{
		return m_tRigidBody.mass * m_tVelocity.sqrMagnitude;
	}
	
	public void Deactivate()
	{
		if (m_tRigidBody && !m_tRigidBody.isKinematic) {
			m_tRigidBody.velocity = Vector3.zero;
		}
		foreach(BreakableBox tBox in m_tChilds)
			tBox.Deactivate();
		BoxPool.Instance.PoolObject(gameObject);
	}

}
