using System;
using UnityEngine;
using UnityEngine.EventSystems;

using static UnityEngine.EventSystems.EventTrigger;

/// <summary>Cell of the game grid</summary>
public class Cell : MonoBehaviour
{
	[Header("References")]
	public Animator anim;
	public Renderer rend;
	public EventTrigger eventTrigger;

	[Header("Debug")]
	public Vector3Int debug_gridPos;

	public int colorIndex { get; private set; }
	public Vector3Int gridPos { get; private set; }

	public void Init(
		Color color,
		int colorIndex,
		Vector3Int gridPos,
		Action<Cell> StartLink,
		Action<Cell> HoverCell
	)
	{
		this.colorIndex = colorIndex;
		this.gridPos = gridPos;
		rend.material.color = color;

		eventTrigger.triggers.Add(BuildEventEntry(EventTriggerType.BeginDrag, () => StartLink(this)));
		eventTrigger.triggers.Add(BuildEventEntry(EventTriggerType.Drag, () => HoverCell(this)));

		eventTrigger.triggers.Add(BuildEventEntry(EventTriggerType.PointerDown, () => StartLink(this)));
		eventTrigger.triggers.Add(BuildEventEntry(EventTriggerType.PointerEnter, () => HoverCell(this)));

		anim.Play("Spawn");
		debug_gridPos = gridPos;
	}

	Entry BuildEventEntry(EventTriggerType type, Action callback)
	{
		Entry entry = new Entry() { eventID = type, callback = new TriggerEvent() };
		entry.callback.AddListener(data => callback());
		return entry;
	}

	// TODO : Make "Pop" anim
	public void ForwardAnim()
	{
		anim.Play("Pop");
		gridPos = new Vector3Int(gridPos.x, gridPos.y, gridPos.z - 1);
		debug_gridPos = gridPos;
	}
}