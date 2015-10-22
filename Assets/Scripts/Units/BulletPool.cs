using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour {
	static BulletPool			_instance = null;
	protected static GameObject	ContainerObject;
	public GameObject			BulletPrefab;
	public static BulletPool	Instance {
		get {
			return _instance;
		}
	}
	
	public int					DefaultBufferSize = 10;
	List<GameObject>			m_tPooledObjects;


	void Start() {
		ContainerObject = new GameObject("Bullets");
		_instance = this;
		m_tPooledObjects = new List<GameObject>();
		for ( int n=0; n<DefaultBufferSize; n++) {
			GameObject tObj = Instantiate(BulletPrefab) as GameObject;
			tObj.transform.parent = ContainerObject.transform;
			tObj.name = "Bullet";
			PoolObject(tObj);
		}
	}
	
	public GameObject GetObject() {
		if(m_tPooledObjects.Count > 0) {
			GameObject pooledObject = m_tPooledObjects[0];
			m_tPooledObjects.RemoveAt(0);
			pooledObject.transform.parent = ContainerObject.transform;
			pooledObject.SetActive(true);
			return pooledObject;
		} else {
			GameObject tObj = Instantiate(BulletPrefab) as GameObject;
			tObj.transform.parent = ContainerObject.transform;
			tObj.name = "Bullet";
			tObj.SetActive(true);
			return tObj;
		}
	}
	
	public void PoolObject (GameObject tObject) {
		tObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		tObject.SetActive(false);
		m_tPooledObjects.Add(tObject);
	}	
}