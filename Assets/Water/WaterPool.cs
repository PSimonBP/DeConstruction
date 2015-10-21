using UnityEngine;

public class WaterPool : Container
{
	public float LifeTime = 10;
	static WaterPool			_instance;
	public static WaterPool		Instance { get { return _instance; } }

	void Start()
	{
		_instance = this;
		Setup(typeof(WaterController));
	}
}
