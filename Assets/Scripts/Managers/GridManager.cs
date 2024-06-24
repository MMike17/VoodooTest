using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	[Header("References")]
	public Transform showcaseTarget;
	public Transform gameplayTarget;
	[Space]
	public Cell cellPrefab;

	List<Layer> cellsLayers;
	Action<bool> SetTilt;
	List<Cell> linkedCells;
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

	public void Init(Action<bool> setTilt)
	{
		SetTilt = setTilt;
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
			float lastLayerZ = cellSize * gridDepth + layerSpacing * (gridDepth - 1);
			float totalLayerSpacing = cellSize + layerSpacing;
			float lastExpandSpacing = expandedSpacing * gridDepth;

			layerTransform.localPosition = new Vector3(0, 0, lastLayerZ - totalLayerSpacing * i);
			cellsLayers.Add(new Layer(layerTransform, lastExpandSpacing - expandedSpacing * i));

			GenerateGridCells(cellsLayers[i], (gridDepth - 1) - i);
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
		cellsLayers.ForEach(layer => layer.PositionLayer(1));

		int layerIndex = 0;
		while (layerIndex < cellsLayers.Count)
		{
			// we shoot and forget to have simultaneous layer generation
			StartCoroutine(SpawnLayerAnim(layerIndex));
			yield return new WaitForSeconds(spawnLayerDelay);

			layerIndex++;
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
		if (!canLink)
			return;

		isLinking = true;
		linkedCells.Add(cell);
	}

	void HoverCell(Cell cell)
	{
		if (!isLinking || linkedCells.Contains(cell))
			return;

		Cell lastCell = linkedCells[^1];
		Vector3Int gridOffset = cell.gridPos - lastCell.gridPos;

		if (cell.gridPos.z > 0)
			return;

		if ((gridOffset.x * gridOffset.x < 0 ? -1 : 1) <= 1 && (gridOffset.y * gridOffset.z < 0 ? -1 : 1) <= 1)
			linkedCells.Add(cell);

		// TODO : Add graphical links here
	}

	void FinishLink()
	{
		// clean link graphs

		Debug.Log("Finished links");

		if (linkedCells.Count < minLinkLength)
		{
			linkedCells.Clear();
			return;
		}

		Vector3Int[] emptyGridPos = new Vector3Int[linkedCells.Count];

		for (int i = 0; i < emptyGridPos.Length; i++)
			emptyGridPos[i] = linkedCells[i].gridPos;

		linkedCells.ForEach(cell => Destroy(cell.gameObject));
		linkedCells.Clear();

		// reduce turns
		// get points

		// move next cell in

		linkedCells.Clear();
		isLinking = false;
	}

	void MoveNextCellsIn(Vector3Int[] gridPos)
	{
		// TODO : How am I going to make this ?

		// loop through all pos of deleted elements
		// loop through all layers
		// get element at x/y coord in this layer if there is one
		// parent it to cell at same coord in previous layer
		// send cell to anim routine

		// elementForwardDuration
		// elementForwardCurve

		// MoveCellsAnim()
	}

	IEnumerator MoveCellsAnim()
	{
		yield return null;
	}

	public void StartGame()
	{
		// TODO : Add clear cells here
		StartCoroutine(ShowcaseAnim());
	}

	public void ExpandLayers(float percent)
	{
		cellsLayers.ForEach(cell => cell.PositionLayer(Mathf.Clamp01(percent)));
	}

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