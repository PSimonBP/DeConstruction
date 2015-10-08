using UnityEngine;

public class ContainerPool : Container
{
	static ContainerPool		_instance;
	public static ContainerPool	Instance { get { return _instance; } }
	
	void Start ()
	{
		_instance = this;
		Setup (typeof(BreakableContainer));
	}
	
	public static BreakableContainer GetContainer ()
	{
		GameObject tObj = Instance.GetObject ();
		if (tObj == null)
			return null;
		return tObj.GetComponent<BreakableContainer> ();
	}
}
