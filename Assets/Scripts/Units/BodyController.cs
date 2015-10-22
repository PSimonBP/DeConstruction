using UnityEngine;
using System.Collections;

public class BodyController : MonoBehaviour {
	public float	RotationSpeed = 1.5f;
	public float	RotationLimit = 80;

	bool			m_bLock = false;
	Vector3			m_tTarget = Vector3.zero;

	void Update () {
		if (m_bLock) {
			Quaternion tTargetRotation = Quaternion.Slerp(transform.rotation, Quaternion.FromToRotation(Vector3.up, m_tTarget - transform.position), Time.deltaTime * RotationSpeed);
			transform.rotation = tTargetRotation;

			Vector3 tTargetAngle = transform.localRotation.eulerAngles;
			bool bCorrect = true;
			if (tTargetAngle.z > RotationLimit && tTargetAngle.z < 360 - RotationLimit) {
				if (tTargetAngle.z > 180)
					tTargetAngle.z = 360 - RotationLimit;
				else
					tTargetAngle.z = RotationLimit;
			} else
				bCorrect = false;
			if (bCorrect) {
				Quaternion tLocalRot = transform.localRotation;
				tLocalRot.eulerAngles = tTargetAngle;
				transform.localRotation = tLocalRot;
			}
		} else {
			transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * RotationSpeed);
		}
	}

	public void LockTo(Vector3 tTarget) {
		m_bLock = true;
		m_tTarget = tTarget;
		m_tTarget.z = 0;
	}

	public void LockRelease() {
		m_bLock = false;
	}
}
