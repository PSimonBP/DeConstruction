using UnityEngine;

public class DebrisController : MonoBehaviour
{

	Rigidbody2D		tRigidBody;
	BreakableBox	tBreakableBox;
	float			fEndTime;
	int				iOriginalLayer;

	void Start()
	{
		iOriginalLayer = gameObject.layer;
		gameObject.layer = 8;
		tRigidBody = gameObject.GetComponent<Rigidbody2D>();
		tBreakableBox = gameObject.GetComponent<BreakableBox>();
		fEndTime = Time.time + Random.Range(0.5f, 2.0f);
	}

	void Update()
	{
		if (fEndTime < Time.time || tRigidBody.IsSleeping()) {
			gameObject.layer = iOriginalLayer;
			tBreakableBox.Deactivate();
			Destroy(this);
		}
	}
}
