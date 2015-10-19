﻿using UnityEngine;

public class WaterPool : Container
{
	public float LifeTime = 3;
	static WaterPool			_instance;
	public static WaterPool		Instance { get { return _instance; } }

	void Start()
	{
		_instance = this;
		Setup(typeof(WaterController));
	}

	void Update()
	{
		GameObject tDrop = WaterPool.Instance.GetObject();
		if (tDrop != null) {
			tDrop.transform.position = transform.position;
			tDrop.transform.SetParent(transform);
		}			
	}
}
