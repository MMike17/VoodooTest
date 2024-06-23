using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages the display of UI panels</summary>
public class PanelsManager : MonoBehaviour
{
	[Header("References")]
	public List<GamePanel> gamePanels;

	[Header("Debug")]
	public PanelTag currentPanel_debug;

	public enum PanelTag
	{
		NONE,
		Main_menu,
		GameUI,
		Settings,
		Win,
		Lose
	}

	PanelTag currentPanel;

	public void Init(Action ResetTilt, Action OnPlay)
	{
		gamePanels.ForEach(item =>
		{
			switch (item.panel)
			{
				case MainMenuPanel mainMenu:
					mainMenu.Init(() =>
					{
						PopPanel(PanelTag.GameUI);
						OnPlay();
					});
					break;

				case GameUIPanel gameUI:
					gameUI.Init(
						() => PopPanel(PanelTag.Settings),
						ResetTilt
					);
					break;

				case SettingsPanel settings:
					settings.Init(() =>
					{
						PopPanel(PanelTag.GameUI);
						GameManager.SaveData();
					});
					break;
			}

			item.panel.anim.Play("Close", 0, 1);
		});

		PopPanel(PanelTag.Main_menu);
	}

	public void PopPanel(PanelTag tag)
	{
		if (currentPanel != PanelTag.NONE)
			gamePanels.Find(item => item.tag == currentPanel).panel.Close();

		GamePanel selected = gamePanels.Find(item => item.tag == tag);

		if (selected == null)
		{
			Debug.LogError("Please assign a panel in the editor for tag \"" + tag + "\"");
			return;
		}

		currentPanel_debug = currentPanel = tag;
		selected.panel.Open();
	}

	[Serializable]
	public class GamePanel
	{
		public PanelTag tag;
		public Panel panel;
	}
}
