using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class GameLogic : MonoBehaviour
{
	int m_iUpdateSteps;
	int m_iDefaultPosIter;
	int m_iDepaultVelIter;
//	Vector2 InitialGravity = Vector2.zero;

	void Start()
	{
		m_iDefaultPosIter = Physics2D.positionIterations;
		m_iDepaultVelIter = Physics2D.velocityIterations;
//		InitialGravity = Physics2D.gravity;
//		Input.gyro.enabled = true;
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

		if (Input.touchCount > 0) {
			var tTouches = Input.touches;
			for (int i = 0; i < Input.touchCount; i++) {
				RaycastHit2D tHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(tTouches [i].position), Vector2.zero);
				if (tHit.collider != null) {
					BreakableBox tBox = tHit.collider.GetComponent<BreakableBox>();
					if (tBox)
						tBox.AddDamage(tBox.Container.FractureForce * 4);
				}
			}
		} else if (Input.GetMouseButton(0)) {
			RaycastHit2D tHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			if (tHit.collider != null) {
				BreakableBox tBox = tHit.collider.GetComponent<BreakableBox>();
				if (tBox)
					tBox.AddDamage(tBox.Container.FractureForce * 4);
			}
		}

/*		if (Input.gyro.enabled) {
			var tGrav = Input.gyro.gravity - new Vector2(Input.acceleration.x, Input.acceleration.y);
			Physics2D.gravity = InitialGravity.magnitude * new Vector2(tGrav.x, tGrav.y);
		}
*/
		var tContainers = FindObjectsOfType<BreakableContainer>();
		foreach (BreakableContainer tCont in tContainers) {
			tCont.DebugDraw = false;
		}
		RaycastHit2D tRayHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
		if (tRayHit.collider != null) {
			BreakableBox tBox = tRayHit.collider.GetComponent<BreakableBox>();
			if (tBox)
				tBox.Container.DebugDraw = true;
		}
	}

	void FixedUpdate()
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
