using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class GameLogic : MonoBehaviour
{
//	int m_iUpdateSteps;
//	int m_iDefaultPosIter;
//	int m_iDepaultVelIter;
//	Vector2 InitialGravity = Vector2.zero;

	void Start ()
	{
//		m_iDefaultPosIter = Physics2D.positionIterations;
//		m_iDepaultVelIter = Physics2D.velocityIterations;
//		InitialGravity = Physics2D.gravity;
//		Input.gyro.enabled = true;
	}

	
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.T) || (CrossPlatformInputManager.ButtonExists ("SlowMotion") && CrossPlatformInputManager.GetButtonDown ("SlowMotion"))) {
			if (System.Math.Abs (Time.timeScale - 1) < Mathf.Epsilon)
				Time.timeScale = 0.2f;
			else
				Time.timeScale = 1;
			return;
		}
		if (Input.GetKeyDown (KeyCode.R) || (CrossPlatformInputManager.ButtonExists ("Reset") && CrossPlatformInputManager.GetButtonDown ("Reset"))) {
			Application.LoadLevel ("MainScene");
			return;
		}

		if (Input.touchCount > 0) {
			var tTouches = Input.touches;
			for (int i = 0; i < Input.touchCount; i++) {
//				Fire(Camera.main.ScreenToWorldPoint(tTouches [i].position));
			}
		}
	}

/*	void FixedUpdate()
	{
		if (++m_iUpdateSteps % 12 == 0) {
			Physics2D.positionIterations = m_iDefaultPosIter;
			Physics2D.velocityIterations = m_iDepaultVelIter;
		} else {
			Physics2D.positionIterations = 1;
			Physics2D.velocityIterations = 1;
		}
	}
*/
}
