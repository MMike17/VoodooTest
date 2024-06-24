using UnityEngine;

/// <summary>Manages the main flow of the game</summary>
public class GameManager : MonoBehaviour
{
	// we can access and modify this from anywhere
	// I should really make sure the save state makes sense at all times
	public static Save save { get; private set; }

	[Header("Settings")]
	public bool prettySave;

	[Header("Managers")]
	public CameraManager cameraManager;
	public PanelsManager panelsManager;
	public GridManager gridManager;
	public InputManager inputManager;

	void Awake()
	{
		DataManager.prettySave = prettySave;
		save = DataManager.LoadData<Save>();
		save ??= new Save();

		InitManagers();
	}

	void InitManagers()
	{
		inputManager.Init();
		cameraManager.Init(
			inputManager.ResetTilt,
			gridManager.ExpandLayers
		);
		panelsManager.Init(
			inputManager.ResetTilt,
			gridManager.StartGame
		);
		gridManager.Init(
			cameraManager.SetCanTilt,
			panelsManager.gameUI.UpdateTurns,
			() => panelsManager.PopPanel(PanelsManager.PanelTag.Lose),
			() => panelsManager.PopPanel(PanelsManager.PanelTag.Win)
		);
	}

	public static void SaveData() => DataManager.SaveData(save);
}