﻿using UnityEngine;

public class WaterController : MonoBehaviour
{
	public Rigidbody2D RigidBody { get; set; }
	float Life;
	int SplitCount;
	public float MaxLife { get; set; }
	public bool Split { get; set; }

	void Start ()
	{
		Init ();
	}

	void OnEnable ()
	{
		Init ();
	}

	public void Init ()
	{
		Split = true;
		SplitCount = 1;
		Life = 0;
		MaxLife = WaterPool.Instance.LifeTime * (Random.Range (0.8f, 1.2f));
		RigidBody = GetComponent<Rigidbody2D> ();
	}

	void Update ()
	{
		Life += Time.deltaTime;
		if (Split && SplitCount > 0 && Life >= MaxLife / (SplitCount + 1)) {
			SplitCount--;
			var tObj = WaterPool.Instance.GetObject ();
			if (tObj) {
				var tNewObj = tObj.GetComponent<WaterController> ();
				tNewObj.transform.SetParent (transform.parent);
				tNewObj.transform.localPosition = transform.localPosition;
				tNewObj.transform.localRotation = transform.localRotation;
				tNewObj.Init ();
				Vector2 tVel = RigidBody.velocity;
				tVel.x += Random.Range (-3, 3);
				tVel.y += Random.Range (-3, 3);
				tNewObj.RigidBody.velocity = tVel;
				tNewObj.Split = false;
			}
		} else if (Life >= MaxLife) {
			WaterPool.Instance.PoolObject (gameObject);
		}
	}
}
