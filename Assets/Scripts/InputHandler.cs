using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {
	private static volatile InputHandler	_instance;
	private Camera							m_tMainCamera = null;
	private Vector3							m_tMousePos = Vector3.zero;

	public static InputHandler Instance { get { return _instance; } }

	void Start() {
		_instance = this;
		m_tMainCamera = GameObject.FindObjectOfType<Camera>();
	}

	void Update () {
		float iWheel = Input.GetAxis("Mouse ScrollWheel");
		Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - iWheel, 1, 10);
		m_tMousePos = m_tMainCamera.ScreenToWorldPoint(Input.mousePosition);
	}
	
	public static Vector3	GetMousePos()	{ return Instance.m_tMousePos; }
	public static bool		Forward()		{ return ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))	&& !(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))); }
	public static bool		Backward()		{ return ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))	&& !(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))); }
	public static bool		Left()			{ return ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))	&& !(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))); }
	public static bool		Right()			{ return ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))	&& !(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))); }
	public static bool		StrafeLeft()	{ return ((Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.End))		&& !(Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.PageDown))); }
	public static bool		StrafeRight()	{ return ((Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.PageDown))	&& !(Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.End))); }
	public static bool		Run()			{ return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); }
	public static bool		Attack()		{ return Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space); }
}
