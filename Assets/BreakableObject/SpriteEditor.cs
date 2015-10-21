#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SpriteProcessor))]
public class SpriteEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (GUILayout.Button("Build Object")) {
			Object target;
			for (int i = 0; i < targets.Length; i++) {
				target = targets [i];
				var tSpriteProcessor = (SpriteProcessor)target;
				tSpriteProcessor.BuildObject();
			}
		}
	}
}
#endif
