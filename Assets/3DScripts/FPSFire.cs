using UnityEngine;
using System.Collections;

public class FPSFire : MonoBehaviour
{

	int iCounter = 0;
	int iOriginalPhysicsStepCount = 0;
	// Use this for initialization
	void Start()
	{
		iOriginalPhysicsStepCount = Physics.solverIterationCount;
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKey(KeyCode.Mouse0)) {
			RaycastHit tHit = new RaycastHit();
//			Vector3 tSceenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out tHit)) {
				BreakController tBody = tHit.collider.gameObject.GetComponent<BreakController>();
				if (tBody == null)
					tBody = tHit.collider.gameObject.GetComponentInParent<BreakController>();
				if (tBody != null) {
					tBody.SetBreak();
				}
				if (tBody == null) {
					ComplexBreakController tBody2 = tHit.collider.gameObject.GetComponent<ComplexBreakController>();
					if (tBody2 == null)
						tBody2 = tHit.collider.gameObject.GetComponentInParent<ComplexBreakController>();
					if (tBody2 != null) {
						tBody2.SetBreak();
					}
				}
			}
		}
	}

	void FixedUpdate()
	{
		if (iOriginalPhysicsStepCount > 0) {
			++iCounter;
			if (iCounter % 4 == 0) {
				Physics.solverIterationCount = iOriginalPhysicsStepCount;
			} else {
				Physics.solverIterationCount = 6;
			}
		}
	}
}
