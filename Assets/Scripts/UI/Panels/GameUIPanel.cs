using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static RequirementTicket;
using static UnityEngine.RectTransform;
using Random = UnityEngine.Random;

public class GameUIPanel : Panel
{
	[Header("Settings")]
	public int prewarmPoolSize;
	[Space]
	public float starParticleSpeed;

	[Header("References")]
	public Button openSettingsButton;
	public Button resetTiltButton;
	[Space]
	public TMP_Text turnsCount;
	[Space]
	public RequirementTicket requirementPrefab;
	public Transform requirementList;
	[Space]
	public TMP_Text starsCounter;
	public RectTransform starPrefab;
	public RectTransform starHolder;
	[Space]
	public Transform linksHolder;
	public RectTransform nodePrefab;
	public RectTransform linkPrefab;

	List<Cell> selectedCells;
	List<RectTransform> nodes;
	List<RectTransform> nodePool;
	List<RectTransform> links;
	List<RectTransform> linkPool;
	Func<Vector3, Vector2> GetUIPos;

	public void Init(Action OnOpenSettings, Action ResetTilt, Func<Vector3, Vector2> getUIPos)
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
		starsCounter.text = "0";
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

	IEnumerator AnimateStars(Vector2 spawnPos, int totalStarsCount, int addStarsCount)
	{
		Debug.DrawLine(spawnPos, starsCounter.transform.position, Color.red, 10);

		// TODO : Pool stars
		List<RectTransform> flyingStars = new List<RectTransform>();
		float distance = Vector3.Distance(spawnPos, starsCounter.transform.position);
		float starDuration = distance / starParticleSpeed / 2; // next star will spawn when current is 1/2 of distance
		float timer = starDuration;
		int currentStarCounter = totalStarsCount - addStarsCount;

		while (currentStarCounter != totalStarsCount)
		{
			timer += Time.deltaTime;

			while (timer >= starDuration && addStarsCount > 0)
			{
				timer -= starDuration;

				flyingStars.Add(Instantiate(
					starPrefab,
					spawnPos,
					Quaternion.Euler(0, 0, Random.Range(0, 360)),
					starHolder
				));

				addStarsCount--;
			}

			List<RectTransform> toDelete = new List<RectTransform>();
			flyingStars.ForEach(star =>
			{
				star.position = Vector3.MoveTowards(
					star.position,
					starsCounter.transform.position,
					starParticleSpeed * Time.deltaTime
				);
				star.rotation = Quaternion.RotateTowards(
					star.rotation,
					Quaternion.identity,
					starParticleSpeed * Time.deltaTime
				);

				if (star.position == starsCounter.transform.position)
					toDelete.Add(star);
			});

			toDelete.ForEach(item =>
			{
				flyingStars.Remove(item);
				Destroy(item.gameObject);

				currentStarCounter++;
			});

			starsCounter.text = currentStarCounter.ToString();
			yield return null;
		}

		starsCounter.text = totalStarsCount.ToString();
	}

	public void UpdateTurns(int turns) => turnsCount.text = turns.ToString();

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

	public void FinishLink(int totalStarsCount, int addStarsCount)
	{
		StartCoroutine(AnimateStars(nodes[^1].position, totalStarsCount, addStarsCount));

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