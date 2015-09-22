#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(TerraGen))]
public class TerraGenEditor : Editor
{
	public override void OnInspectorGUI()
	{
		// display the "original" inspector stuff
		base.OnInspectorGUI();
		// and add a button underneath
		if (GUILayout.Button("Generate terrain")) {
			foreach (var target in targets) {
				TerraGen tGenerator = (TerraGen)target;
				tGenerator.CreateTerrain();
			}
		}
	}
}
#endif
