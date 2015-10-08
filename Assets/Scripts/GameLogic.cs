using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class GameLogic : MonoBehaviour
{
	void Start()
	{
	}
	
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.T) || (CrossPlatformInputManager.ButtonExists("SlowMotion") && CrossPlatformInputManager.GetButtonDown("SlowMotion"))) {
			if (System.Math.Abs(Time.timeScale - 1) < Mathf.Epsilon)
				Time.timeScale = 0.2f;
			else
				Time.timeScale = 1;
		}
		if (Input.GetKeyDown(KeyCode.R) || (CrossPlatformInputManager.ButtonExists("Reset") && CrossPlatformInputManager.GetButtonDown("Reset"))) {
			Application.LoadLevel("MainScene");
		}

		if (Input.GetMouseButton(0)) {
			RaycastHit2D tHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			if (tHit.collider != null) {
				BreakableBox tBox = tHit.collider.GetComponent<BreakableBox>();
				if (tBox) {
					var tBoxList = new List<BreakableBox>();
					tBox.AddDamage(5000, tBoxList);
					foreach (BreakableBox tBreakBox in tBoxList) {
						tBreakBox.ApplyDamage();
					}
				}
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
}
