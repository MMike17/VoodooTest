using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages the grid of elements</summary>
public class GridManager : MonoBehaviour
{
	[Header("Settings")]
	public int gridSize;
	public float cellSize;
	public float cellSpacing;
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

	[Header("References")]
	public Transform showcaseTarget;
	public Transform gameplayTarget;
	[Space]
	public Cell cellPrefab;

	List<Layer> cellsLayers;

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

	public void Init()
	{
		GenerateGridLayers();
		StartCoroutine(ShowcaseAnim());
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

			GenerateGridCells(cellsLayers[i]);
		}
	}

	void GenerateGridCells(Layer layer)
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
				layer.cells.Add(cell);

				cell.SetParent(layer.transform);
				cell.localPosition = new Vector3(
					firstCellX + totalCellSpacing * x,
					firstCellY - totalCellSpacing * y,
					0
				);
			}
		}
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

		// TODO : Start game here
	}

	IEnumerator SpawnLayerAnim(int layerIndex)
	{
		foreach (Transform cellPoint in cellsLayers[layerIndex].cells)
		{
			// TODO : I'll probably have to modify this once I get to the actual gameplay
			yield return new WaitForSeconds(spawnCellDelay);
			Instantiate(cellPrefab, cellPoint).transform.localPosition = Vector3.zero;
		}
	}

	///<summary>Represents a layer of the grid</summary>
	[Serializable] // used for debug
	public class Layer
	{
		public Transform transform;
		public List<Transform> cells;

		public Vector3 normalOffset;
		public Vector3 expandedOffset;

		public Layer(Transform transform, float expandedDepth)
		{
			this.transform = transform;
			cells = new List<Transform>();

			normalOffset = transform.localPosition;
			expandedOffset = new Vector3(0, 0, expandedDepth);
		}

		// TODO : Call this when we tilt the phone
		public void PositionLayer(float expandAmount)
		{
			transform.localPosition = Vector3.Lerp(normalOffset, expandedOffset, expandAmount);
		}
	}
}