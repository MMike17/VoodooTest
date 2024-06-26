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
	public HapticsManager hapticsManager;
	public InputManager inputManager;
	public AudioManager audioManager;
	public CameraManager cameraManager;
	public PanelsManager panelsManager;
	public GridManager gridManager;

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
		audioManager.Init();

		cameraManager.Init(
			inputManager.ResetTilt,
			gridManager.ExpandLayers
		);

		panelsManager.Init(
			inputManager.ResetTilt,
			() => gridManager.StartGame(false),
			cameraManager.GetUIPos,
			() => gridManager.StartGame(true),
			gridManager.GetCurrentRequirements,
			gridManager.GetStars,
			audioManager.PlaySound,
			hapticsManager.Vibrate
		);

		gridManager.Init(
			cameraManager.SetCanTilt,
			panelsManager.gameUI.UpdateTurns,
			panelsManager.gameUI.DisplayRequirements,
			panelsManager.gameUI.AddCell,
			panelsManager.gameUI.FinishLink,
			audioManager.PlaySound,
			() => panelsManager.PopPanel(PanelsManager.PanelTag.Lose),
			() => panelsManager.PopPanel(PanelsManager.PanelTag.Win),
			hapticsManager.Vibrate
		);
	}

	public static void SaveData() => DataManager.SaveData(save);
}