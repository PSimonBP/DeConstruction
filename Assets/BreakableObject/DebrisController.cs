using UnityEngine;
using System.Collections;

public class DebrisController : MonoBehaviour {

	private Rigidbody2D		tRigidBody = null;
	private BreakableBox	tBreakableBox = null;
	private float			fEndTime = 0;
	private int				iOriginalLayer = 0;

	// Use this for initialization
	void Start () {
		iOriginalLayer = gameObject.layer;
		gameObject.layer = 8;
		tRigidBody = gameObject.GetComponent<Rigidbody2D>();
		tBreakableBox = gameObject.GetComponent<BreakableBox>();
		fEndTime = Time.time + Random.Range(0.5f, 2.0f);
	}

	// Update is called once per frame
	void Update () {
		if (fEndTime < Time.time || tRigidBody.IsSleeping()) {
			gameObject.layer = iOriginalLayer;
			tBreakableBox.Deactivate();
			Destroy(this);
		}
	}
}
