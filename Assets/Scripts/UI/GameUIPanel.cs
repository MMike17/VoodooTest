using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static GridManager;
using static UnityEngine.RectTransform;

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
	Func<Vector3, Vector3> GetUIPos;

	public void Init(Action OnOpenSettings, Action ResetTilt, Func<Vector3, Vector3> getUIPos)
	{
		GetUIPos = getUIPos;
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

	void Update()
	{
		for (int i = 0; i < selectedCells.Count; i++)
		{
			RectTransform current = nodes[i];
			current.position = GetUIPos(selectedCells[i].transform.position);

			if (i > 0)
			{
				RectTransform previous = nodes[i - 1];
				RectTransform link = links[i - 1]; // yes I know this feels weird

				link.position = Vector3.Lerp(current.position, previous.position, 0.5f);
				link.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(
					Vector3.right,
					previous.position - current.position,
					Vector3.forward
				));
				link.SetSizeWithCurrentAnchors(Axis.Horizontal, Vector3.Distance(current.position, previous.position));
			}
		}
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
		RectTransform node = null;

		if (nodePool.Count > 0)
		{
			node = nodePool[0];
			node.gameObject.SetActive(true);
			nodePool.Remove(node);
		}
		else
			node = Instantiate(nodePrefab, linksHolder);

		nodes.Add(node);

		if (selectedCells.Count > 1)
		{
			RectTransform link = null;

			if (linkPool.Count > 0)
			{
				link = linkPool[0];
				link.gameObject.SetActive(true);
				linkPool.Remove(link);
			}
			else
				link = Instantiate(linkPrefab, linksHolder);

			links.Add(link);
		}

		nodes.ForEach(item => item.SetAsLastSibling());
	}

	public void ClearCells()
	{
		nodes.ForEach(item =>
		{
			item.gameObject.SetActive(false);
			nodePool.Add(item);
		});
		links.ForEach(item =>
		{
			item.gameObject.SetActive(false);
			linkPool.Add(item);
		});

		nodes.Clear();
		links.Clear();
		selectedCells.Clear();
	}
}