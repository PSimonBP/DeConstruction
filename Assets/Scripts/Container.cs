using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour {
	public GameObject	Prefab;
	public int			ObjectLimit = 100;
	List<GameObject>	m_tPooledObjects;
	int					m_iObjectCount;
	int					m_iUsedObjectCount;
		
	public void Setup(System.Type tType) {
		m_tPooledObjects = new List<GameObject>();
		var tExistingObjList = FindObjectsOfType(tType);
		m_iObjectCount = tExistingObjList.Length;
		m_iUsedObjectCount = tExistingObjList.Length;
	}
	
	bool IncreaseBufferSize() {
		if (m_iObjectCount >= ObjectLimit)
			return false;
		++m_iObjectCount;
		++m_iUsedObjectCount;
		GameObject tObj = Instantiate(Prefab);
		tObj.transform.parent = transform;
		PoolObject(tObj);
		return true;
	}
	
	public int GetFreePoolSize() {
		return ObjectLimit - m_iUsedObjectCount;
	}

	public GameObject GetObject() {
		if(m_tPooledObjects.Count == 0 && !IncreaseBufferSize())
			return null;
		
		GameObject pooledObject = m_tPooledObjects[0];
		m_tPooledObjects.RemoveAt(0);
		pooledObject.transform.parent = transform;
		pooledObject.SetActive(true);
		++m_iUsedObjectCount;
		return pooledObject;
	}
	
	public void PoolObject (GameObject tObject) {
		--m_iUsedObjectCount;
		tObject.transform.parent = transform;
		tObject.SetActive(false);
		m_tPooledObjects.Add(tObject);
	}
}
