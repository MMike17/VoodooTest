using System;
using UnityEngine;
using UnityEngine.UI;

public class GameUIPanel : Panel
{
	[Header("References")]
	public Button openSettingsButton;
	public Button resetTiltButton;

	// TODO : Make panel prefab

	public void Init(Action OnOpenSettings, Action ResetTilt)
	{
		resetTiltButton.gameObject.SetActive(GameManager.save.tiltType == InputManager.TiltType.Gyroscope);

		openSettingsButton.onClick.AddListener(() => OnOpenSettings());
		resetTiltButton.onClick.AddListener(() => ResetTilt());
	}
}