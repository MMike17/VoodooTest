using UnityEditor;
using UnityEngine;

// TODO : Add description here
[CreateAssetMenu(menuName = "Gizmos/GizmosSettings")]
public class GizmosSettings : EditorScriptableObject<GizmosSettings>
{
	[MenuItem("Tools/Select " + nameof(GizmosSettings))]
	static void SelectSettings() => Selection.activeObject = Get();

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
}