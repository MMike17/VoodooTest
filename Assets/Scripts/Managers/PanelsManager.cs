using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages the display of UI panels</summary>
public class PanelsManager : MonoBehaviour
{
	[Header("References")]
	public List<GamePanel> gamePanels;

	public enum PanelTag
	{
		NONE,
		Main_menu,
		Win,
		Lose
	}

	public PanelTag currentPanel;

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
	}

	void ClearPanel() => currentPanel = PanelTag.NONE;

	[Serializable]
	public class GamePanel
	{
		public PanelTag tag;
		public Panel panel;
	}
}
