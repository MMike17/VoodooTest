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
		Win,
		Lose
	}

	PanelTag currentPanel;

	public void Init()
	{
		gamePanels.ForEach(item => item.panel.Init(ClearPanel));

		PopPanel(PanelTag.Main_menu);
	}

	public void PopPanel(PanelTag tag)
	{
		if (currentPanel != PanelTag.NONE)
		{
			Debug.LogError("Can't pop panel while another panel is open");
			return;
		}

		currentPanel_debug = currentPanel = tag;
		GamePanel selected = gamePanels.Find(item => item.tag == tag);

		if (selected == null)
		{
			Debug.LogError("Please assign a panel in the editor for tag \"" + tag + "\"");
			return;
		}

		selected.panel.Open();
	}

	void ClearPanel() => currentPanel = PanelTag.NONE;

	[Serializable]
	public class GamePanel
	{
		public PanelTag tag;
		public Panel panel;
	}
}
