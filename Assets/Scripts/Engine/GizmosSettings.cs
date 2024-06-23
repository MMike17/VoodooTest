using UnityEditor;
using UnityEngine;

/// <summary>Centralized settings for Gizmos display</summary>
[CreateAssetMenu(menuName = "Gizmos/GizmosSettings")]
public class GizmosSettings : EditorScriptableObject<GizmosSettings>
{
#if UNITY_EDITOR
	[MenuItem("Tools/Select " + nameof(GizmosSettings))]
	static void SelectSettings() => Selection.activeObject = Get();
#endif

	[Header(nameof(GridManager))]
	public bool enableGrid;
	public bool showExpandLayer;
	[Space]
	public float layerPointSize;
	public Color layerNormalColor;
	public Color layerExpandColor;
	[Space]
	public int layerDisplay;
	public Color layerColor;

	[Header(nameof(CameraManager))]
	public bool enableCamera;
	public float camTargetSize;
	public Color camTargetColor;
	public float camTargetSimSize;
	public Color camTargetSimColor;
	[Space]
	[Range(-1, 1)]
	public float simulatedXTilt;
	[Range(-1, 1)]
	public float simulatedYTilt;
}