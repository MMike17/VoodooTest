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

	void Awake()
	{
		DataManager.prettySave = prettySave;
		save = DataManager.LoadData<Save>();

		InitManagers();
	}

	void InitManagers()
	{
		cameraManager.Init();
		// panelsManager.Init();
		gridManager.Init(cameraManager.SetCanTilt);
	}

	public static void SaveData() => DataManager.SaveData(save);
}