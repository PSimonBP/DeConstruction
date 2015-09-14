using UnityEngine;
using System.Collections;

public class TerraGen : MonoBehaviour {
	public int TerrainWidth = 128;
	public int TerrainHeight = 128;
	public int Height = 64;
	public int Complexity = 2;

	public void CreateTerrain()
	{
		TerrainData tData = new TerrainData();
		tData.size = new Vector3(TerrainWidth, Height, TerrainHeight);
		tData.heightmapResolution = 64;
		GenerateHeightMap(tData, Complexity);
		GameObject tTerrainGO = Terrain.CreateTerrainGameObject(tData);
		tTerrainGO.transform.parent = transform;
		Terrain tTerrain = tTerrainGO.GetComponent<Terrain>();
		Vector3 tPos = tTerrain.transform.localPosition;
		tPos.x -= TerrainWidth / 2;
		tPos.z -= TerrainHeight / 2;
		tTerrain.transform.localPosition = tPos;
	}

	public void GenerateHeightMap(TerrainData tData, int iTileSize)
	{
        float[,] heights = new float[tData.heightmapWidth, tData.heightmapHeight];
        for (int i = 0; i < tData.heightmapWidth; i++)
        {
            for (int k = 0; k < tData.heightmapHeight; k++)
            {
				heights[i, k] = (Mathf.PerlinNoise(((float)i / (float)tData.heightmapWidth) * iTileSize, ((float)k / (float)tData.heightmapHeight) * iTileSize));
			}
        }

        tData.SetHeights(0, 0, heights);
	}

// 	void Start () {
// 	
// 	}
	
// 	void Update () {
// 	
// 	}
}
