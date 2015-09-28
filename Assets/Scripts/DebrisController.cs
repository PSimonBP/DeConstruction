using UnityEngine;
using System.Collections;

public class DebrisController : MonoBehaviour {

	private Rigidbody2D		tRigidBody = null;
	private BreakController	tBreakController = null;
	private float			fEndTime = 0;

	// Use this for initialization
	void Start () {
		tRigidBody = gameObject.GetComponent<Rigidbody2D>();
	}
	
	public void Init(BreakController tCtrl) {
		tBreakController = tCtrl;
		fEndTime = Time.time + Random.Range(0.5f, 2.0f);
	}
	// Update is called once per frame
	void Update () {
		if (fEndTime < Time.time || tRigidBody.IsSleeping()) {
			tBreakController.Deactivate();
			Destroy(this);
		}
	}
}
