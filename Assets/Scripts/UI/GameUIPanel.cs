using System;
using UnityEngine;
using UnityEngine.UI;

public class GameUIPanel : Panel
{
	[Header("References")]
	public Button openSettingsButton;
	public Button resetTiltButton;

	public void Init(Action OnOpenSettings, Action ResetTilt)
	{
		openSettingsButton.onClick.AddListener(() => OnOpenSettings());
		resetTiltButton.onClick.AddListener(() => ResetTilt());
	}

	public override void Open()
	{
		resetTiltButton.gameObject.SetActive(GameManager.save.tiltType == InputManager.TiltType.Gyroscope);
		base.Open();
	}
}