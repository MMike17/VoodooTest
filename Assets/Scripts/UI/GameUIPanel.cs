using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static GridManager;

public class GameUIPanel : Panel
{
	[Header("Settings")]
	public int prewarmPoolSize;

	[Header("References")]
	public Button openSettingsButton;
	public Button resetTiltButton;
	[Space]
	public TMP_Text turnsCount;
	[Space]
	public RequirementTicket requirementPrefab;
	public Transform requirementList;
	[Space]
	public Transform linksHolder;
	public RectTransform nodePrefab;
	public RectTransform linkPrefab;

	List<Cell> selectedCells;
	List<RectTransform> nodes;
	List<RectTransform> nodePool;
	List<RectTransform> links;
	List<RectTransform> linkPool;

	public void Init(Action OnOpenSettings, Action ResetTilt)
	{
		selectedCells = new List<Cell>();

		nodes = new List<RectTransform>();
		nodePool = new List<RectTransform>();
		links = new List<RectTransform>();
		linkPool = new List<RectTransform>();

		for (int i = 0; i < prewarmPoolSize; i++)
		{
			nodePool.Add(Instantiate(nodePrefab, linksHolder));
			linkPool.Add(Instantiate(linkPrefab, linksHolder));
		}

		nodePool.ForEach(node => node.gameObject.SetActive(false));
		linkPool.ForEach(node => node.gameObject.SetActive(false));

		openSettingsButton.onClick.AddListener(() => OnOpenSettings());
		resetTiltButton.onClick.AddListener(() => ResetTilt());
	}

	public override void Open()
	{
		resetTiltButton.gameObject.SetActive(GameManager.save.tiltType == InputManager.TiltType.Gyroscope);
		base.Open();
	}

	public void UpdateTurns(int turns)
	{
		turnsCount.text = turns.ToString();
	}

	public void DisplayRequirements(List<Requirement> requirements, Color[] colors)
	{
		int requirementIndex = 0;

		foreach (RequirementTicket ticket in requirementList.GetComponentsInChildren<RequirementTicket>())
		{
			if (requirementIndex >= requirements.Count)
				ticket.gameObject.SetActive(false);
			else
			{
				Requirement requirement = requirements[requirementIndex];
				ticket.Init(requirement.index == -1 ? Color.grey : colors[requirement.index], requirement.count);
				requirementIndex++;
			}
		}

		while (requirementIndex < requirements.Count)
		{
			Requirement requirement = requirements[requirementIndex];
			Instantiate(requirementPrefab, requirementList).Init(
				requirement.index == -1 ? Color.grey : colors[requirement.index],
				requirement.count
			);

			requirementIndex++;
		}
	}

	public void AddCell(Cell cell)
	{
		selectedCells.Add(cell);
	}

	public void RemoveCell(Cell cell)
	{
		//
	}
}