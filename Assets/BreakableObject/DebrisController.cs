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
		fEndTime = Time.time + Random.Range(0.1f, 0.5f);
		BoxPool.DebrisList.Add(this);
	}

	void Update()
	{
		if (fEndTime < Time.time) {
			BoxPool.DebrisList.Remove(this);
			Kill();
		}
	}

	public void Kill()
	{
		gameObject.layer = iOriginalLayer;
		tBreakableBox.Deactivate();
		Destroy(this);
	}
}
