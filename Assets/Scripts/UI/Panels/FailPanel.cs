using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static RequirementTicket;

public class FailPanel : Panel
{
	[Header("References")]
	public Transform requirementsHolder;
	public RequirementTicket requirementPrefab;
	public Button restartButton;
	public Button toMenuButton;

	Func<(Color[], List<Requirement>)> GetCurrentRequirements;

	public void Init(Action RestartGame, Action ToMainMenu, Func<(Color[], List<Requirement>)> getCurrentRequirements)
	{
		GetCurrentRequirements = getCurrentRequirements;

		restartButton.onClick.AddListener(() => RestartGame());
		toMenuButton.onClick.AddListener(() => ToMainMenu());
	}

	public override void Open()
	{
		(Color[] colors, List<Requirement> requirements) = GetCurrentRequirements();
		int index = 0;

		foreach (RequirementTicket ticket in requirementsHolder.GetComponentsInChildren<RequirementTicket>())
		{
			if (index >= requirements.Count)
				ticket.gameObject.SetActive(false);
			else
			{
				Requirement requirement = requirements[index];
				ticket.Init(requirement.index == -1 ? Color.gray : colors[requirement.index], requirement.count);
				index++;
			}
		}

		while (index < requirements.Count)
		{
			Requirement requirement = requirements[index];
			Instantiate(requirementPrefab, requirementsHolder).Init(
				requirement.index == -1 ? Color.grey : colors[requirement.index],
				requirement.count
			);

			index++;
		}

		base.Open();
	}
}