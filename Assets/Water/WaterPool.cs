using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterPool : MonoBehaviour {
	static WaterPool				_instance = null;
	protected static GameObject	ContainerObject;
	public GameObject			WaterPrefab;
	public static WaterPool		Instance {
		get {
			return _instance;
		}
	}
	
	public int					DefaultBufferSize = 10;
	public int					BufferGrowSize = 10;
	public int					ObjectLimit = 1000;
	List<GameObject>			m_tPooledObjects;
	int							m_iObjectCount = 0;
	int							m_iUsedObjectCount = 0;


	void Start() {
		ContainerObject = this.gameObject;
		_instance = this;
		m_tPooledObjects = new List<GameObject>();
		WaterController[] tObjList = FindObjectsOfType<WaterController>();
		m_iObjectCount = tObjList.Length;
		m_iUsedObjectCount = tObjList.Length;
		IncreaseBufferSize (DefaultBufferSize);
//		ContainerObject.name = "WaterPool - " + m_iObjectCount + " - " + m_iUsedObjectCount;
	}

	void Update() {
		if (Random.Range(0, 10) > 6) {
			var tDrop = WaterPool.Instance.GetObject();
			if (tDrop != null) {
				tDrop.GetComponent<WaterController>().gameObject.transform.position = gameObject.transform.position;
			}
		}
	}
	
	void IncreaseBufferSize(int iSize) {
		for ( int n=0; n<iSize; n++) {
			if (m_iObjectCount >= ObjectLimit)
				return;
			++m_iObjectCount;
			++m_iUsedObjectCount;
			GameObject tObj = Instantiate(WaterPrefab) as GameObject;
			tObj.transform.parent = ContainerObject.transform;
			tObj.name = "Water";
			PoolObject(tObj);
		}
	}

	public int GetFreePoolSize() {
//		ContainerObject.name = "WaterPool - " + m_iObjectCount + " - " + m_iUsedObjectCount;
		return ObjectLimit - m_iUsedObjectCount;
	}

	public GameObject GetObject() {
		if(m_tPooledObjects.Count == 0) {
			IncreaseBufferSize(BufferGrowSize);
		}
		if (m_tPooledObjects.Count == 0)
			return null;

		GameObject pooledObject = m_tPooledObjects[0];
		m_tPooledObjects.RemoveAt(0);
		pooledObject.transform.parent = ContainerObject.transform;
		pooledObject.SetActive(true);
		++m_iUsedObjectCount;
//		ContainerObject.name = "WaterPool - " + m_iObjectCount + " - " + m_iUsedObjectCount;
		return pooledObject;
	}
	
	public void PoolObject (GameObject tObject) {
		--m_iUsedObjectCount;
//		ContainerObject.name = "WaterPool - " + m_iObjectCount + " - " + m_iUsedObjectCount;
		Rigidbody2D tBody = tObject.GetComponent<Rigidbody2D>();
		if (tBody && !tBody.isKinematic)
				tBody.velocity = Vector3.zero;
		tObject.transform.parent = ContainerObject.transform;
		tObject.SetActive(false);
		m_tPooledObjects.Add(tObject);
	}	
}