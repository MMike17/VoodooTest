using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIPanel : Panel
{
	[Header("References")]
	public Button openSettingsButton;
	public Button resetTiltButton;
	[Space]
	public TMP_Text turnsCount;

	public void Init(Action OnOpenSettings, Action ResetTilt)
	{
		openSettingsButton.onClick.AddListener(() => OnOpenSettings());
		resetTiltButton.onClick.AddListener(() => ResetTilt());
	}

	public void UpdateTurns(int turns)
	{
		turnsCount.text = turns.ToString();
	}

	public override void Open()
	{
		resetTiltButton.gameObject.SetActive(GameManager.save.tiltType == InputManager.TiltType.Gyroscope);
		base.Open();
	}
}