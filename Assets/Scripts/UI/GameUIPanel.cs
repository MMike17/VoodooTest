using System;
using UnityEngine;
using UnityEngine.UI;

public class GameUIPanel : Panel
{
	[Header("References")]
	public Button openSettingsButton;

	// TODO : Make panel prefab

	public void Init(Action OnOpenSettings)
	{
		openSettingsButton.onClick.AddListener(() => OnOpenSettings());
	}
}