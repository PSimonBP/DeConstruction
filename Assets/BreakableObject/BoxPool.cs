using System.Collections.Generic;
using UnityEngine;

public class BoxPool : Container
{
	static BoxPool				_instance;
	public static BoxPool		Instance { get { return _instance; } }

	float m_fDebrisSize { get; set; }
	List<DebrisController> m_tDebrisList = new List<DebrisController> ();

	public static List<DebrisController> DebrisList { get { return _instance.m_tDebrisList; } }
	public static float DebrisSize { get { return _instance.m_fDebrisSize; } }

	bool bLowFrameRate;

	void Start ()
	{
		bLowFrameRate = false;
		m_fDebrisSize = 0.2f;
		_instance = this;
		Setup (typeof(BreakableBox));
	}

	public static BreakableBox GetBox ()
	{
		if (_instance.GetFreePoolSize () == 0 && DebrisList.Count > 0) {
			DebrisList [0].Kill ();
			DebrisList.RemoveAt (0);
		}
		GameObject tObj = Instance.GetObject ();
		if (tObj == null)
			return null;
//		_instance.CalcDebrisSize ();
		return tObj.GetComponent<BreakableBox> ();
	}

	public static void PoolBox (GameObject tBox)
	{
		_instance.PoolObject (tBox);
//		_instance.CalcDebrisSize ();
	}

/*	void Update ()
	{
		bool bPrevFR = bLowFrameRate;
		bLowFrameRate = 1 / Time.deltaTime <= 30;
		if (bPrevFR != bLowFrameRate)
			CalcDebrisSize ();
	}
*/
/*	void CalcDebrisSize ()
	{
		float fFillRatio = (float)_instance.GetFreePoolSize () / ObjectLimit;
		if (fFillRatio <= 0.05f || bLowFrameRate) {
			m_fDebrisSize = 0.6f;
		} else if (fFillRatio <= 0.1f) {
			m_fDebrisSize = 0.5f;
		} else if (fFillRatio <= 0.15f) {
			m_fDebrisSize = 0.4f;
		} else if (fFillRatio <= 0.2f) {
			m_fDebrisSize = 0.3f;
		} else {		
			m_fDebrisSize = 0.2f;
		}
	}
*/
}
