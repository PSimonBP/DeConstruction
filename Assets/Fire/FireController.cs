using UnityEngine;

public class FireController : MonoBehaviour
{
	public Rigidbody2D RigidBody { get; set; }
	float Life;
	int SplitCount;
	public float MaxLife { get; set; }
	public bool Split { get; set; }

	void Start()
	{
		Init();
	}

	void OnEnable()
	{
		Init();
	}

	public void Init()
	{
		Split = true;
		SplitCount = 1;
		Life = 0;
		MaxLife = FirePool.Instance.LifeTime * (Random.Range(0.8f, 1.2f));
		RigidBody = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		Life += Time.deltaTime;
		if (Split && SplitCount > 0 && Life >= MaxLife / (SplitCount + 1)) {
			SplitCount--;
			var tObj = FirePool.GetWater();
			if (tObj) {
				tObj.transform.SetParent(transform.parent);
				tObj.transform.localPosition = transform.localPosition;
				tObj.transform.localRotation = transform.localRotation;
				tObj.Init();
				Vector2 tVel = RigidBody.velocity;
				tVel.x += Random.Range(-3, 3);
				tVel.y += Random.Range(-3, 3);
				tObj.RigidBody.velocity = tVel;
				tObj.Split = false;
			}
		} else if (Life >= MaxLife) {
			FirePool.Instance.PoolObject(gameObject);
		}
	}
}
