using UnityEngine;

public class FirePool : Container
{
	public float LifeTime = 10;
	static FirePool			_instance;
	public static FirePool	Instance { get { return _instance; } }

	void Start()
	{
		_instance = this;
		Setup(typeof(FireController));
	}

	public static FireController GetFire()
	{
		GameObject tObj = _instance.GetObject();
		if (tObj == null) {
			_instance.PoolOldest();
			tObj = _instance.GetObject();
		}
		if (tObj)
			return tObj.GetComponent<FireController>();
		return null;
	}
}
