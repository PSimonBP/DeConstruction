using UnityEngine;

public class BoxPool : Container {
	static BoxPool				_instance;
	public static BoxPool		Instance { get { return _instance; } }

	void Start() {
		_instance = this;
		Setup(typeof(BreakableBox));
	}

	public static BreakableBox GetBox() {
		GameObject tObj = Instance.GetObject();
		if (tObj == null)
			return null;
		return tObj.GetComponent<BreakableBox>();
	}
}
