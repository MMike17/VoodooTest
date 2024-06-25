using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Ticket used to display requirements</summary>
public class RequirementTicket : MonoBehaviour
{
	[Header("References")]
	public Image selectedColor;
	public TMP_Text countDisplay;
	public Image completed;

	public void Init(Color color, int count)
	{
		selectedColor.color = color;
		countDisplay.enabled = count > 0;
		completed.enabled = count <= 0;
		countDisplay.text = count.ToString();

		gameObject.SetActive(true);
	}

	[Serializable] // used for debug
	public class Requirement
	{
		public int index;
		public int count;

		public Requirement(int index, int count)
		{
			this.index = index;
			this.count = count;
		}
	}
}