using UnityEngine;
using System.Collections;

public class TerrainGenerator : MonoBehaviour {
	public GameObject[]	TerrainPieces;
	void Start () {
		for (int x=-8; x<8; ++x) {
			for (int y=-8; y<8; ++y) {
				GameObject tObj = Instantiate(TerrainPieces[Random.Range(0, TerrainPieces.Length)]) as GameObject;
				tObj.transform.parent = transform;
				tObj.transform.position = new Vector3(x, y, 0);
				tObj.name = "Terrain";
			}
		}
	}
}
