using System;
using UnityEngine;
using UnityEngine.UI;

using static InputManager;

public class SettingsPanel : Panel
{
	[Header("References")]
	public Toggle sound;
	public Toggle vibrations;
	[Space]
	public Toggle tiltTypeGyro;
	public Toggle tiltTypeDrag;
	[Space]
	public Button closeButton;

	public void Init(Action OnClose)
	{
		// set init state
		sound.SetIsOnWithoutNotify(GameManager.save.soundOn);
		vibrations.SetIsOnWithoutNotify(GameManager.save.vibrationsOn);

		Toggle tiltSelected = GameManager.save.tiltType switch
		{
			TiltType.Gyroscope => tiltTypeGyro,
			TiltType.Drag => tiltTypeDrag,
			_ => null
		};

		tiltSelected?.SetIsOnWithoutNotify(true);

		// subscribe events
		sound.onValueChanged.AddListener(state => GameManager.save.soundOn = state);
		vibrations.onValueChanged.AddListener(state => GameManager.save.vibrationsOn = state);

		tiltTypeGyro.onValueChanged.AddListener(state => RefreshTilt());
		tiltTypeDrag.onValueChanged.AddListener(state => RefreshTilt());

		closeButton.onClick.AddListener(() => OnClose());
	}

	void RefreshTilt()
	{
		if (tiltTypeGyro.isOn)
			GameManager.save.tiltType = TiltType.Gyroscope;

		if (tiltTypeDrag.isOn)
			GameManager.save.tiltType = TiltType.Drag;
	}
}