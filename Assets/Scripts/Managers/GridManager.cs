using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static AudioManager;
using static RequirementTicket;
using Random = UnityEngine.Random;

/// <summary>Manages the grid of elements</summary>
public class GridManager : MonoBehaviour
{
	[Header("Settings")]
	public int gridSize;
	public float cellSize;
	public float cellSpacing;
	[Space]
	public int minLinkLength;
	public Color[] cellColors;
	[Space]
	public int gridDepth;
	public float layerSpacing;
	public float expandedSpacing;
	public AnimationCurve expandingCurve;
	[Space]
	public float spawnLayerDelay; // grid size * grid size * spawnCellDelay = time to spawn 1 layer
	public float spawnCellDelay;
	public float showcaseEndDelay;
	[Space]
	public float toGameplayDuration;
	public AnimationCurve toGameplayCurve;
	[Space]
	public float elementForwardDuration;
	public AnimationCurve elementForwardCurve;
	[Space]
	public float turnGain;
	[Space]
	public int minRequirements;
	public int maxRequirements;
	public int minRequirementAmount;
	public int maxRequirementAmount;

	// TODO : Move most of the game design settings to a separate scriptable object

	[Header("References")]
	public Transform showcaseTarget;
	public Transform gameplayTarget;
	[Space]
	public Cell cellPrefab;

	List<Layer> cellsLayers;
	List<Cell> linkedCells;
	List<Requirement> requiredColors;
	Action<List<Requirement>, Color[]> DisplayRequirements;
	Action<Cell> AddCellUI;
	Action<int, int> FinishLineUI;
	Action<SoundTag> PlaySound;
	Action<int> SetTurn;
	Action<bool> SetTilt;
	Action OnGameOver;
	Action OnWin;
	Action Vibrate;
	(int total, int current) turns;
	int selectedColorIndex;
	int starsCount;
	bool canLink;
	bool isLinking;

	void OnDrawGizmos()
	{
		GizmosSettings settings = GizmosSettings.Get();

		if (cellsLayers != null && settings.enableGrid)
		{
			if (settings.layerDisplay >= cellsLayers.Count)
				settings.layerDisplay = -1;

			if (settings.layerDisplay < -1)
				settings.layerDisplay = cellsLayers.Count - 1;

			if (settings.layerDisplay >= 0)
			{
				Layer selected = cellsLayers[settings.layerDisplay];
				Gizmos.color = settings.layerColor;
				float gridSideSize = cellSize * gridSize + cellSpacing * (gridSize - 1);
				Gizmos.DrawCube(selected.transform.position, new Vector3(gridSideSize, gridSideSize, 0.1f));
			}

			foreach (Layer layer in cellsLayers)
			{
				// TODO : I have to fix those gizmos
				if (settings.showExpandLayer)
				{
					Gizmos.color = settings.layerExpandColor;
					Gizmos.DrawSphere(
						transform.position + layer.transform.forward + layer.expandedOffset,
						settings.layerPointSize
					);
				}
				else
				{
					Gizmos.color = settings.layerNormalColor;
					Gizmos.DrawSphere(
						transform.position + layer.transform.forward + layer.normalOffset,
						settings.layerPointSize
					);
				}
			}
		}
	}

	public void Init(
		Action<bool> setTilt,
		Action<int> setTurn,
		Action<List<Requirement>, Color[]> displayRequirements,
		Action<Cell> addCellUI,
		Action<int, int> finishLineUI,
		Action<SoundTag> playSound,
		Action onGameOver,
		Action onWin,
		Action vibrate
	)
	{
		SetTilt = setTilt;
		SetTurn = setTurn;
		DisplayRequirements = displayRequirements;
		AddCellUI = addCellUI;
		FinishLineUI = finishLineUI;
		PlaySound = playSound;
		OnGameOver = onGameOver;
		OnWin = onWin;
		Vibrate = vibrate;
		linkedCells = new List<Cell>();
		canLink = false;
		isLinking = false;

		GenerateGridLayers();
	}

	void GenerateGridLayers()
	{
		cellsLayers = new List<Layer>();

		// we start building from the last layer (in depth) and move to the closest one to the camera
		for (int i = 0; i < gridDepth; i++)
		{
			// TODO : Should I add a wait for GameObject to be ready ?
			Transform layerTransform = new GameObject("Layer " + (i + 1)).transform;
			layerTransform.SetParent(transform);

			// we want the closest (current) layer to be at the pivot of the holder
			float normalLayerSpacing = cellSize + layerSpacing;
			float expandedLayerSpacing = cellSize + expandedSpacing;

			layerTransform.localPosition = new Vector3(0, 0, normalLayerSpacing * i);
			cellsLayers.Add(new Layer(layerTransform, expandedLayerSpacing * i));

			GenerateGridCells(cellsLayers[i], i);
		}
	}

	void GenerateGridCells(Layer layer, int layerIndex)
	{
		float firstCellX = (-(cellSize * gridSize + cellSpacing * (gridSize - 1)) + cellSize) / 2;
		float firstCellY = -firstCellX;
		float totalCellSpacing = cellSize + cellSpacing;

		// we move left to right and up to down (for a cooler animation)
		for (int y = 0; y < gridSize; y++)
		{
			for (int x = 0; x < gridSize; x++)
			{
				Transform cell = new GameObject("Cell " + (y + x) + " (" + layer.transform.name + ")").transform;
				layer.cells.Add((cell, new Vector3Int(x, y, layerIndex)));

				cell.SetParent(layer.transform);
				cell.localPosition = new Vector3(
					firstCellX + totalCellSpacing * x,
					firstCellY - totalCellSpacing * y,
					0
				);
			}
		}
	}

	void Update()
	{
		if (isLinking && InputManager.GetPointerUp())
			FinishLink();
	}

	IEnumerator ShowcaseAnim()
	{
		transform.SetPositionAndRotation(showcaseTarget.position, showcaseTarget.rotation);

		SetTilt(false);
		cellsLayers.ForEach(layer => layer.PositionLayer(1));

		int layerIndex = cellsLayers.Count - 1;
		while (layerIndex >= 0)
		{
			// we shoot and forget to have simultaneous layer generation
			StartCoroutine(SpawnLayerAnim(layerIndex));
			yield return new WaitForSeconds(spawnLayerDelay);

			layerIndex--;
		}

		yield return new WaitForSeconds(showcaseEndDelay);

		float timer = 0;
		while (timer < toGameplayDuration)
		{
			timer += Time.deltaTime;
			float percent = toGameplayCurve.Evaluate(timer / toGameplayDuration);

			transform.SetPositionAndRotation(
				Vector3.Lerp(showcaseTarget.position, gameplayTarget.position, percent),
				Quaternion.Lerp(showcaseTarget.rotation, gameplayTarget.rotation, percent)
			);

			cellsLayers.ForEach(layer => layer.PositionLayer(1 - percent));
			yield return null;
		}

		transform.SetPositionAndRotation(gameplayTarget.position, gameplayTarget.rotation);

		SetTilt(true);
		canLink = true;
	}

	IEnumerator SpawnLayerAnim(int layerIndex)
	{
		foreach ((Transform cellPoint, Vector3Int gridPos) in cellsLayers[layerIndex].cells)
		{
			yield return new WaitForSeconds(spawnCellDelay);

			Cell cell = Instantiate(cellPrefab, cellPoint);
			cell.transform.localPosition = Vector3.zero;

			int colorIndex = Random.Range(0, cellColors.Length);
			cell.Init(cellColors[colorIndex], colorIndex, gridPos, StartLink, HoverCell);
		}
	}

	void StartLink(Cell cell)
	{
		if (!canLink || isLinking || cell.gridPos.z > 0)
			return;

		isLinking = true;
		linkedCells.Add(cell);

		selectedColorIndex = cell.colorIndex;
		AddCellUI(cell);
		SetTilt(false);

		PlaySound(SoundTag.Element_Selection);
	}

	void HoverCell(Cell cell)
	{
		bool validColor = cell.colorIndex == selectedColorIndex || cell.colorIndex == -1;

		if (!isLinking || linkedCells.Contains(cell) || !validColor)
			return;

		Cell lastCell = linkedCells[^1];
		Vector3Int gridOffset = cell.gridPos - lastCell.gridPos;

		if (cell.gridPos.z > 0)
			return;

		if ((gridOffset.x * (gridOffset.x < 0 ? -1 : 1)) <= 1 && (gridOffset.y * (gridOffset.y < 0 ? -1 : 1)) <= 1)
		{
			linkedCells.Add(cell);
			AddCellUI(cell);
			PlaySound(SoundTag.Element_Selection);
		}
	}

	void FinishLink()
	{
		isLinking = false;
		Vibrate();

		// stars
		int rewardStars = Mathf.Max(linkedCells.FindAll(item => item.colorIndex != -1).Count - minLinkLength, 0);
		starsCount += rewardStars;

		// clear ui & display stars
		FinishLineUI(starsCount, rewardStars);

		if (linkedCells.Count >= minLinkLength)
		{
			SetTilt(false);

			// requirements
			requiredColors.FindAll(item => item.index == -1 || item.index == selectedColorIndex)
				.ForEach(requirement => requirement.count -= linkedCells.Count);

			DisplayRequirements(requiredColors, cellColors);

			// destroy cells
			Vector3Int[] emptyGridPos = new Vector3Int[linkedCells.Count];

			for (int i = 0; i < emptyGridPos.Length; i++)
				emptyGridPos[i] = linkedCells[i].gridPos;

			linkedCells.ForEach(cell => Destroy(cell.gameObject));
			linkedCells.Clear();

			// turns
			turns.current--;
			SetTurn(turns.current);

			bool canGameOver = true;

			if (requiredColors.Find(item => item.count > 0) == null)
			{
				OnWin();
				canGameOver = false;
			}

			PlaySound(SoundTag.Element_Destruction);
			MoveNextCellsIn(emptyGridPos, canGameOver);
		}
		else
			PlaySound(SoundTag.Link_too_short);

		linkedCells.Clear();
	}

	void MoveNextCellsIn(Vector3Int[] gridPositions, bool canGameOver)
	{
		List<Cell> movingCells = new List<Cell>();

		// we only loop through the cells we need
		foreach (Vector3Int pos in gridPositions)
		{
			// select all cells at the same coord with higher depth
			for (int i = pos.z + 1; i < cellsLayers.Count; i++)
			{
				// it's faster to get the index arithmetically
				int cellIndex = pos.x + pos.y * gridSize;
				Cell cell = cellsLayers[i].cells[cellIndex].transform.GetComponentInChildren<Cell>();
				bool shouldSpawnOmni = false;

				if (cell != null)
				{
					// i should never be equal to 0 since pos.z can't be < 0
					cell.transform.SetParent(cellsLayers[i - 1].cells[cellIndex].transform);
					movingCells.Add(cell);

					if (i + 1 == cellsLayers.Count || cellsLayers[i + 1].cells[cellIndex].transform.childCount == 0)
						shouldSpawnOmni = true;
				}
				else
					shouldSpawnOmni = true;

				if (shouldSpawnOmni)
				{
					Vector3Int gridPos = cellsLayers[i].cells[cellIndex].gridPos;
					gridPos.z = i + 1; // even if layer i+1 doesn't exist we still get a virtual grid pos

					Cell omniCell = Instantiate(cellPrefab, cellsLayers[i].cells[cellIndex].transform);
					omniCell.Init(Color.grey, -1, gridPos, StartLink, HoverCell);

					omniCell.transform.position = i + 1 == cellsLayers.Count ?
						cellsLayers[i].cells[cellIndex].transform.position :
						cellsLayers[i + 1].cells[cellIndex].transform.position; ;

					movingCells.Add(omniCell);
				}
			}
		}

		canLink = false;
		StartCoroutine(MoveCellsAnim(movingCells, canGameOver));
	}

	IEnumerator MoveCellsAnim(List<Cell> cells, bool canGameOver)
	{
		Vector3 initialCellPos = cells[0].transform.localPosition;
		float timer = 0;

		while (timer < elementForwardDuration)
		{
			timer += Time.deltaTime;
			float percent = elementForwardCurve.Evaluate(timer / elementForwardDuration);

			cells.ForEach(cell => cell.transform.localPosition = Vector3.Lerp(initialCellPos, Vector3.zero, percent));
			yield return null;
		}

		cells.ForEach(cell =>
		{
			cell.transform.localPosition = Vector3.zero;
			cell.ForwardAnim();
		});

		if (canGameOver && turns.current == 0)
			OnGameOver();
		else
			canLink = true;
	}

	public void StartGame(bool isRestart)
	{
		starsCount = 0;

		// clear cells
		cellsLayers.ForEach(layer => layer.cells.ForEach(cell =>
		{
			if (cell.transform.childCount > 0)
				Destroy(cell.transform.GetChild(0).gameObject);
		}));

		// requirements
		if (isRestart)
			requiredColors.ForEach(requirement => requirement.count = requirement.maxCount);
		else
		{
			List<int> possibleColors = new List<int>();

			while (possibleColors.Count < cellColors.Length)
				possibleColors.Add(possibleColors.Count);

			possibleColors.Add(-1);
			requiredColors = new List<Requirement>();
			int requirementCount = Random.Range(minRequirements, maxRequirements);

			while (requirementCount > 0)
			{
				int index = possibleColors[Random.Range(0, possibleColors.Count)];

				possibleColors.Remove(index);
				requiredColors.Add(new Requirement(index, Random.Range(minRequirementAmount, maxRequirementAmount)));

				requirementCount--;
			}
		}

		// turns
		if (isRestart)
			turns.current = turns.total; // reset score
		else
		{
			// turn count depends on requirements amount
			int totalRequest = 0;
			requiredColors.ForEach(item => totalRequest += item.count);
			turns.total = turns.current = Mathf.FloorToInt(totalRequest / requiredColors.Count * turnGain);
		}

		SetTurn(turns.current);

		DisplayRequirements(requiredColors, cellColors);

		StartCoroutine(ShowcaseAnim());
	}

	public void ExpandLayers(float percent)
	{
		cellsLayers.ForEach(cell => cell.PositionLayer(Mathf.Clamp01(percent)));
	}

	public (Color[], List<Requirement>) GetCurrentRequirements() => (cellColors, requiredColors);

	public int GetStars() => starsCount;

	///<summary>Represents a layer of the grid</summary>
	[Serializable] // used for debug
	public class Layer
	{
		public Transform transform;
		public List<(Transform transform, Vector3Int gridPos)> cells;

		public Vector3 normalOffset;
		public Vector3 expandedOffset;

		public Layer(Transform transform, float expandedDepth)
		{
			this.transform = transform;
			cells = new List<(Transform transform, Vector3Int gridPos)>();

			normalOffset = transform.localPosition;
			expandedOffset = new Vector3(0, 0, expandedDepth);
		}

		public void PositionLayer(float expandAmount)
		{
			transform.localPosition = Vector3.Lerp(normalOffset, expandedOffset, expandAmount);
		}
	}
}