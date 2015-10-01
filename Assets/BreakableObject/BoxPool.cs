using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoxPool : MonoBehaviour {
	static BoxPool				_instance = null;
	private static GameObject	ContainerObject;
	public GameObject			BoxPrefab;
	public static BoxPool		Instance {
		get {
			return _instance;
		}
	}

	public int					ObjectLimit = 100;
	List<BreakableBox>			m_tPooledObjects;
	int							m_iObjectCount = 0;
	int							m_iUsedObjectCount = 0;


	void Start() {
		ContainerObject = this.gameObject;
		_instance = this;
		m_tPooledObjects = new List<BreakableBox>();
		BreakableBox[] tObjList = FindObjectsOfType<BreakableBox>();
		m_iObjectCount = tObjList.Length;
		m_iUsedObjectCount = tObjList.Length;
	}

	bool IncreaseBufferSize() {
		if (m_iObjectCount >= ObjectLimit)
			return false;
		++m_iObjectCount;
		++m_iUsedObjectCount;
		GameObject tObj = Instantiate(BoxPrefab) as GameObject;
		tObj.transform.parent = ContainerObject.transform;
		PoolObject(tObj.GetComponent<BreakableBox>());
        return true;
    }

	public int GetFreePoolSize() {
		return ObjectLimit - m_iUsedObjectCount;
	}

	public BreakableBox GetObject() {
		if(m_tPooledObjects.Count == 0 && !IncreaseBufferSize())
			return null;

		BreakableBox pooledObject = m_tPooledObjects[0];
		m_tPooledObjects.RemoveAt(0);
		pooledObject.transform.parent = ContainerObject.transform;
		pooledObject.gameObject.SetActive(true);
		++m_iUsedObjectCount;
		return pooledObject;
	}

	public void PoolObject (BreakableBox tObject) {
		--m_iUsedObjectCount;
		tObject.transform.parent = ContainerObject.transform;
		tObject.gameObject.SetActive(false);
		m_tPooledObjects.Add(tObject);
	}
}