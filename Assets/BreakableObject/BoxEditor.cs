#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(BreakableBox))]
public class BoxEditor : Editor
{
	public override void OnInspectorGUI()
	{
		// display the "original" inspector stuff
		base.OnInspectorGUI();
		// and add a button underneath
		if (GUILayout.Button("Break box")) {
			foreach (var target in targets) {
				BreakableBox tBox = (BreakableBox)target;
				tBox.SetBreak();
			}
		}
	}
}
#endif