using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class GameLogic : MonoBehaviour
{
	int m_iUpdateSteps;
	int m_iDefaultPosIter;
	int m_iDepaultVelIter;

	void Start ()
	{
		m_iDefaultPosIter = Physics2D.positionIterations;
		m_iDepaultVelIter = Physics2D.velocityIterations;
	}

	
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.T) || (CrossPlatformInputManager.ButtonExists ("SlowMotion") && CrossPlatformInputManager.GetButtonDown ("SlowMotion"))) {
			if (System.Math.Abs (Time.timeScale - 1) < Mathf.Epsilon)
				Time.timeScale = 0.2f;
			else
				Time.timeScale = 1;
		}
		if (Input.GetKeyDown (KeyCode.R) || (CrossPlatformInputManager.ButtonExists ("Reset") && CrossPlatformInputManager.GetButtonDown ("Reset"))) {
			Application.LoadLevel ("MainScene");
		}

		if (Input.GetMouseButton (0)) {
			RaycastHit2D tHit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (Input.mousePosition), Vector2.zero);
			if (tHit.collider != null) {
				BreakableBox tBox = tHit.collider.GetComponent<BreakableBox> ();
				if (tBox)
					tBox.AddDamage (tBox.Container.FractureForce * 4);
			}
		}

/*		var tBoxes = FindObjectsOfType<BreakableBox>();
		foreach (BreakableBox tDebugBox in tBoxes) {
			tDebugBox.DebugDraw = false;
		}
		if (tBox) {
			var tBoxList = new List<BreakableBox>();
			tBox.GetConnectedBoxes(tBoxList);
			foreach (BreakableBox tRedBox in tBoxList) {
				tRedBox.DebugDraw = true;
			}
		}
*/
	}

	void FixedUpdate ()
	{
		if (++m_iUpdateSteps % 12 == 0) {
			Physics2D.positionIterations = m_iDefaultPosIter;
			Physics2D.velocityIterations = m_iDepaultVelIter;
		} else {
			Physics2D.positionIterations = 1;
			Physics2D.velocityIterations = 1;
		}
	}
}
