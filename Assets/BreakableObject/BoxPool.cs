using System.Collections.Generic;
using UnityEngine;

public class BoxPool : Container
{
	static BoxPool				_instance;
	public static BoxPool		Instance { get { return _instance; } }

	float m_fDebrisSize { get; set; }
	List<DebrisController> m_tDebrisList = new List<DebrisController>();

	public static List<DebrisController> DebrisList { get { return _instance.m_tDebrisList; } }
	public static float DebrisSize { get { return _instance.m_fDebrisSize; } }

	void Start()
	{
		m_fDebrisSize = 0.2f;
		_instance = this;
		Setup(typeof(BreakableBox));
	}

	public static BreakableBox GetBox()
	{
		if (_instance.GetFreePoolSize() == 0 && DebrisList.Count > 0) {
			DebrisList [0].Kill();
			DebrisList.RemoveAt(0);
		}
		GameObject tObj = Instance.GetObject();
		if (tObj == null)
			return null;
		_instance.CalcDebrisSize();
		return tObj.GetComponent<BreakableBox>();
	}

	public static void PoolBox(GameObject tBox)
	{
		_instance.PoolObject(tBox);
		_instance.CalcDebrisSize();
	}

	void CalcDebrisSize()
	{
		float fFillRatio = (float)_instance.GetFreePoolSize() / ObjectLimit;
		if (fFillRatio < 0.2f) {
			float fRatio = fFillRatio * 5;
			if (fRatio < Mathf.Epsilon)
				fRatio = Mathf.Epsilon;
//			Debug.Log(fRatio);
			m_fDebrisSize = 0.4f;
		} else {		
			m_fDebrisSize = 0.2f;
		}
	}
}
