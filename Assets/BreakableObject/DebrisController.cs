using UnityEngine;

public class DebrisController : MonoBehaviour
{

	BreakableBox	tBreakableBox;
	float			fEndTime;
	int				iOriginalLayer;

	void Start()
	{
		iOriginalLayer = gameObject.layer;
		gameObject.layer = 8;
		tBreakableBox = gameObject.GetComponent<BreakableBox>();
		fEndTime = Time.time + Random.Range(0.5f, 2.0f);
	}

	void Update()
	{
		if (fEndTime < Time.time) {
			gameObject.layer = iOriginalLayer;
			tBreakableBox.Deactivate();
			Destroy(this);
		}
	}
}
