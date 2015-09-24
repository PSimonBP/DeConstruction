using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoxPool : MonoBehaviour {
	static BoxPool				_instance = null;
	protected static GameObject	ContainerObject;
	public GameObject			BoxPrefab;
	public static BoxPool		Instance {
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
		ContainerObject = new GameObject("BoxPool");
		_instance = this;
		m_tPooledObjects = new List<GameObject>();
		BreakController[] tObjList = FindObjectsOfType<BreakController>();
		m_iObjectCount = tObjList.Length;
		m_iUsedObjectCount = tObjList.Length;
		IncreaseBufferSize (DefaultBufferSize);
//		ContainerObject.name = "BoxPool - " + m_iObjectCount + " - " + m_iUsedObjectCount;
	}

	void IncreaseBufferSize(int iSize) {
		for ( int n=0; n<iSize; n++) {
			if (m_iObjectCount >= ObjectLimit)
				return;
			++m_iObjectCount;
			++m_iUsedObjectCount;
			GameObject tObj = Instantiate(BoxPrefab) as GameObject;
			tObj.transform.parent = ContainerObject.transform;
			tObj.name = "Box";
			PoolObject(tObj);
		}
	}

	public int GetFreePoolSize() {
//		ContainerObject.name = "BoxPool - " + m_iObjectCount + " - " + m_iUsedObjectCount;
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
//		pooledObject.transform.parent = ContainerObject.transform;
		pooledObject.SetActive(true);
		++m_iUsedObjectCount;
//		ContainerObject.name = "BoxPool - " + m_iObjectCount + " - " + m_iUsedObjectCount;
		return pooledObject;
	}
	
	public void PoolObject (GameObject tObject) {
		--m_iUsedObjectCount;
//		ContainerObject.name = "BoxPool - " + m_iObjectCount + " - " + m_iUsedObjectCount;
		Rigidbody2D tBody = tObject.GetComponent<Rigidbody2D>();
		if (tBody && !tBody.isKinematic)
				tBody.velocity = Vector3.zero;
//		tObject.transform.parent = ContainerObject.transform;
		tObject.SetActive(false);
		m_tPooledObjects.Add(tObject);
	}	
}