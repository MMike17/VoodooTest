using UnityEngine;

/// <summary>Manages the main flow of the game</summary>
public class GameManager : MonoBehaviour
{
	// we can access and modify this from anywhere
	// I should really make sure the save state makes sense at all times
	public static Save save { get; private set; }

	[Header("Managers")]
	public PanelsManager panelsManager;

	void Awake()
	{
		save = DataManager.LoadData<Save>();

		InitManagers();
	}

	void InitManagers()
	{
		panelsManager.Init();
	}

	public static void SaveData() => DataManager.SaveData(save);
}