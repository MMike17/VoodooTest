using UnityEngine;

/// <summary>Manages saving and loading of complex data</summary>
public static class DataManager
{
	public static string saveKey = "save";
	public static bool prettySave = false;

	public static void SaveData<T>(T data)
	{
		if (!data.GetType().IsSerializable)
		{
			Debug.LogError("The class \"" + data.GetType() + "\" isn't serializable and thus cannot be saved.");
			return;
		}

		string jsonData = JsonUtility.ToJson(data, prettySave);

		if (string.IsNullOrWhiteSpace(saveKey))
		{
			Debug.LogError("You can't save data with an empty or whitespace key (key is \"" + saveKey + "\")");
			return;
		}

		PlayerPrefs.SetString(saveKey, jsonData);
		PlayerPrefs.Save();
	}

	public static T LoadData<T>()
	{
		T data = default;

		if (PlayerPrefs.HasKey(saveKey))
			data = JsonUtility.FromJson<T>(PlayerPrefs.GetString(saveKey));
		else
			Debug.Log("Couldn't find data for save key \"" + saveKey + "\"");

		return data;
	}
}