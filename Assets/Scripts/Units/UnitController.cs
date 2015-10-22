using UnityEngine;
using System.Collections;

public class UnitController : MonoBehaviour {
	public enum EMovementStates {
		MS_STANDSTILL,
		MS_TURN_L,
		MS_TURN_R,
		MS_WALK_FWD,
		MS_WALK_FWD_L,
		MS_WALK_FWD_R,
		MS_WALK_LEFT,
		MS_WALK_RIGHT,
		MS_WALK_BWD,
		MS_WALK_BWD_L,
		MS_WALK_BWD_R,
		MS_RUN_FWD,
		MS_RUN_FWD_L,
		MS_RUN_FWD_R
	};

	public float	HP = 100.0f;
	public bool		Player = false;

	EMovementStates m_tMovement = EMovementStates.MS_STANDSTILL;
	BodyController	m_tBody = null;

///////////////////////////////////////////////////////////////////////////////
	void Start() {
/*		SpriteRenderer[] tRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer tRend in tRenderers) {
			Vector2 tOffset = tRend.material.mainTextureOffset;
			tOffset.y += tRend.sortingOrder * 5.5f;
			tRend.material.mainTextureOffset = tOffset;
		}*/

		m_tBody = gameObject.GetComponentInChildren<BodyController>();
	}

	void Update () {
		if (Player) {
			if (InputHandler.Forward()) {
				if (InputHandler.Left())
					m_tMovement = InputHandler.Run() ? EMovementStates.MS_RUN_FWD_L : EMovementStates.MS_WALK_FWD_L;
				else if (InputHandler.Right())
					m_tMovement = InputHandler.Run() ? EMovementStates.MS_RUN_FWD_R : EMovementStates.MS_WALK_FWD_R;
				else
					m_tMovement = InputHandler.Run() ? EMovementStates.MS_RUN_FWD : EMovementStates.MS_WALK_FWD;
			} else if (InputHandler.Backward()) {
				if (InputHandler.Left())
					m_tMovement = EMovementStates.MS_WALK_BWD_L;
				else if (InputHandler.Right())
					m_tMovement = EMovementStates.MS_WALK_BWD_R;
				else
					m_tMovement = EMovementStates.MS_WALK_BWD;
			} else if (InputHandler.Left())
				m_tMovement = EMovementStates.MS_TURN_L;
			else if (InputHandler.Right())
				m_tMovement = EMovementStates.MS_TURN_R;
			else if (InputHandler.StrafeLeft())
				m_tMovement = EMovementStates.MS_WALK_LEFT;
			else if (InputHandler.StrafeRight())
				m_tMovement = EMovementStates.MS_WALK_RIGHT;
			else
				m_tMovement = EMovementStates.MS_STANDSTILL;

			m_tBody.LockTo(InputHandler.GetMousePos());
		} else {
			// todo AI
		}
	}

///////////////////////////////////////////////////////////////////////////////
	public EMovementStates	GetCurrentMovement()	{ return m_tMovement; }
	public bool				IsAttacking()			{ return Player ? InputHandler.Attack() : false; } // todo AI

	public void				Damage(float fDamage, Vector2 tPosition, GameObject tAttacker) {
		HP -= fDamage;
		if (HP <= 0)
			gameObject.SetActive(false);
	}

/*	void OnTriggerEnter(Collider tOther) {
	}

	void OnTriggerExit(Collider tOther) {
	}*/
}
