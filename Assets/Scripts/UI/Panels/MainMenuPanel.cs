using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : Panel
{
	[Header("References")]
	public Button playButton;
	public Button quitButton;

	public void Init(Action OnPlay)
	{
		playButton.onClick.AddListener(() => OnPlay());
		quitButton.onClick.AddListener(() => Application.Quit());
	}
}