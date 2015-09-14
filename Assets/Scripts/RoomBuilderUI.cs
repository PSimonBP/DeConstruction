#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(RoomBuilder))]
public class RoomBuilderUI : Editor
{
	public override void OnInspectorGUI()
	{
		// display the "original" inspector stuff
		base.OnInspectorGUI();
		// and add a button underneath
		if (GUILayout.Button("Generate terrain")) {
			foreach (var target in targets) {
				RoomBuilder tGenerator = (RoomBuilder)target;
				tGenerator.CreateRoom();
			}
		}
	}
}
#endif
