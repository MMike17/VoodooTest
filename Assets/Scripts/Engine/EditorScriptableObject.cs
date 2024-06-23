using UnityEditor;
using UnityEngine;

/// <summary>Scriptable objects that should have a static accessor in the editor</summary>
public class EditorScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
	static T instance;

	public static T Get()
	{
#if UNITY_EDITOR
		if (instance == null)
		{
			string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

			if (guids.Length == 0)
				Debug.LogError("Couldn't find instance of " + typeof(T).Name + " in assets. Please create one.");
			else
				instance = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
		}
#endif

		return instance;
	}
}