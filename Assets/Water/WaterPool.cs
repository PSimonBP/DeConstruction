using UnityEngine;

public class WaterPool : Container
{
	public float LifeTime = 10;
	static WaterPool			_instance;
	public static WaterPool		Instance { get { return _instance; } }

	void Start ()
	{
		_instance = this;
		Setup (typeof(WaterController));
	}

	void Update ()
	{
		GameObject tDrop = null;
//		int iCounter = 0;
//		do {
		tDrop = WaterPool.Instance.GetObject ();
		if (tDrop != null) {
			tDrop.transform.position = transform.position + new Vector3 (Random.Range (-0.1f, 0.1f), Random.Range (-0.1f, 0.1f), 0);
			tDrop.transform.SetParent (transform);
		}
//			++ iCounter;
//		} while (tDrop != null && iCounter < 10);
	}
}
