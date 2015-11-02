#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(BreakableContainer))]
public class ContainerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		// display the "original" inspector stuff
		base.OnInspectorGUI();
		// and add a button underneath
		if (GUILayout.Button("Simplify")) {
			Object target;
			for (int i = 0; i < targets.Length; i++) {
				target = targets [i];
				var tBox = (BreakableContainer)target;
				tBox.SimplifyObject();
			}
		}
	}
}
#endif
