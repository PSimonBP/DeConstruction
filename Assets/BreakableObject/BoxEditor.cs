#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

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
			Object target;
			for (int i = 0; i < targets.Length; i++) {
				target = targets [i];
				var tBox = (BreakableBox)target;
				tBox.Break();
			}
		}
	}
}
#endif
