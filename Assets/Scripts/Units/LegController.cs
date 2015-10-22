using UnityEngine;
using System.Collections;

public class LegController : MonoBehaviour {
	public float	StepSize = 0.05f;
	public float	StepTime = 1.0f;

	bool			m_bLeftLegMovingFWD = false;
	bool			m_bLeftLegTurning = false;
	UnitController	m_tController = null;
	GameObject		m_tLeftLeg = null;
	GameObject		m_tRightLeg = null;
	
	Vector2			m_tLeftDefPos = Vector2.zero;
	Vector2			m_tRightDefPos = Vector2.zero;
	float			m_fLeftDefRot = 0;
	float			m_fRightDefRot = 0;
	
	float			m_fCurrentStepTime = 0.5f;
	Vector2			m_tMoveSpeed = Vector2.zero;
	float			m_fTurnSpeed = 0;
	
	void Start () {
		m_fCurrentStepTime = StepTime / 2.0f;
		
		m_tController = gameObject.GetComponentInParent<UnitController>();
		Transform[] tChildren = gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform tChild in tChildren) {
			if (tChild.gameObject.name == "Leg_L")
				m_tLeftLeg = tChild.gameObject;
			else if (tChild.gameObject.name == "Leg_R")
				m_tRightLeg = tChild.gameObject;
		}
		
		m_tLeftDefPos = m_tLeftLeg.transform.localPosition;
		m_fLeftDefRot = m_tLeftLeg.transform.localRotation.z;
		m_tRightDefPos = m_tRightLeg.transform.localPosition;
		m_fRightDefRot = m_tRightLeg.transform.localRotation.z;

		if (Random.Range(0, 10) >= 5)
			m_bLeftLegMovingFWD = true;
		if (Random.Range(0, 10) >= 5)
			m_bLeftLegTurning = true;
	}
	
	void Update () {
		UnitController.EMovementStates tMovement = m_tController.GetCurrentMovement();
		float fWalkSpeed = 0.0f;
		float fSideSpeed = 0.0f;
		float fTurnSpeed = 0.0f;
		switch (tMovement) {
			case UnitController.EMovementStates.MS_WALK_FWD:	fWalkSpeed = 1.0f;	break;
			case UnitController.EMovementStates.MS_WALK_FWD_L:	fWalkSpeed = 1.0f;	fTurnSpeed = 10.0f;	break;
			case UnitController.EMovementStates.MS_WALK_FWD_R:	fWalkSpeed = 1.0f;	fTurnSpeed = -10.0f;	break;
			case UnitController.EMovementStates.MS_RUN_FWD:		fWalkSpeed = 1.6f;	break;
			case UnitController.EMovementStates.MS_RUN_FWD_L:	fWalkSpeed = 1.6f;	fTurnSpeed = 10.0f;	break;
			case UnitController.EMovementStates.MS_RUN_FWD_R:	fWalkSpeed = 1.6f;	fTurnSpeed = -10.0f;	break;
			case UnitController.EMovementStates.MS_WALK_BWD:	fWalkSpeed = -0.6f;	break;
			case UnitController.EMovementStates.MS_WALK_BWD_L:	fWalkSpeed = -0.6f;	fTurnSpeed = 10.0f;	break;
			case UnitController.EMovementStates.MS_WALK_BWD_R:	fWalkSpeed = -0.6f;	fTurnSpeed = -10.0f;	break;
			case UnitController.EMovementStates.MS_WALK_LEFT:	fSideSpeed = -0.6f;	break;
			case UnitController.EMovementStates.MS_WALK_RIGHT:	fSideSpeed = 0.6f;	break;
			case UnitController.EMovementStates.MS_TURN_L:		fTurnSpeed = 10.0f;	break;
			case UnitController.EMovementStates.MS_TURN_R:		fTurnSpeed = -10.0f;	break;
		}
		
		m_tMoveSpeed = new Vector2(SideStep(fSideSpeed), Step(fWalkSpeed));
		m_fTurnSpeed = Turn(fTurnSpeed);
	}

	void FixedUpdate() {
//		m_tController.rigidbody2D.AddTorque(m_fTurnSpeed * Time.fixedDeltaTime, ForceMode2D.Force);
//		m_tController.rigidbody2D.AddRelativeForce(m_tMoveSpeed * 3, ForceMode2D.Force);
		m_tController.GetComponent<Rigidbody2D>().velocity = m_tController.GetComponent<Rigidbody2D>().GetRelativeVector(m_tMoveSpeed);
		m_tController.GetComponent<Rigidbody2D>().angularVelocity = m_fTurnSpeed * 6;
	}

	protected virtual float Step(float fWalkSpeed) {
		if (fWalkSpeed == 0)
			return 0;
		m_fCurrentStepTime += Time.deltaTime * fWalkSpeed * (m_bLeftLegMovingFWD ? 1.0f : -1.0f);
		if (m_fCurrentStepTime > StepTime) {
			m_fCurrentStepTime = StepTime;
			m_bLeftLegMovingFWD = !m_bLeftLegMovingFWD;
		} else if (m_fCurrentStepTime < 0) {
			m_fCurrentStepTime = 0;
			m_bLeftLegMovingFWD = !m_bLeftLegMovingFWD;
		}
		float fSin = Mathf.Sin((Mathf.PI * 2.0f) * (m_fCurrentStepTime / StepTime)) * (m_bLeftLegMovingFWD ? 1 : -1);
		Vector3 tPos = m_tLeftDefPos;
		Vector3 tRPos = m_tRightDefPos;
		tPos.y += StepSize * fSin;
		tRPos.y -= StepSize * fSin;
		m_tLeftLeg.transform.localPosition = tPos;
		m_tRightLeg.transform.localPosition = tRPos;
		return fWalkSpeed * Mathf.Abs(fSin); // todo precise
	}

	protected virtual float SideStep(float fSideSpeed) {
		// todo
		return 0;
	}

	protected virtual float Turn(float fTurnSpeed) {
		float fAngle = fTurnSpeed;
		if (fTurnSpeed != 0 || m_tLeftLeg.transform.localRotation.z != m_fLeftDefRot) {
			float fTrnSpeed = Time.deltaTime * fTurnSpeed;
			Quaternion tRot = m_tLeftLeg.transform.localRotation;
			tRot.z -= m_fLeftDefRot;
			
			if (fTurnSpeed == 0) {
				if (tRot.z < 0)
					tRot.z += Time.deltaTime / 5.0f;
				else
					tRot.z -= Time.deltaTime / 5.0f;
				if (Mathf.Abs(tRot.z) < 0.001f)
					tRot.z = 0;
			} else {
				tRot.z += fTrnSpeed / (m_bLeftLegTurning ? 50.0f : -50.0f);
			}
			if (tRot.z > StepSize) {
				tRot.z = StepSize;
				m_bLeftLegTurning = !m_bLeftLegTurning;
			} else if (tRot.z < -StepSize) {
				tRot.z = -StepSize;
				m_bLeftLegTurning = !m_bLeftLegTurning;
			}
			Quaternion tRRot = m_tRightLeg.transform.localRotation;
			tRRot.z = m_fRightDefRot - tRot.z;
			tRot.z += m_fLeftDefRot;
			m_tLeftLeg.transform.localRotation = tRot;
			m_tRightLeg.transform.localRotation = tRRot;
		}
		return fAngle;
	}
}
