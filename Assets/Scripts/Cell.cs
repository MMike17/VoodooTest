using UnityEngine;

/// <summary>Cell of the game grid</summary>
public class Cell : MonoBehaviour
{
	[Header("References")]
	public Animator anim;
	public Renderer rend;

	public int colorIndex { get; set; }

	public void Init(Color color, int colorIndex)
	{
		rend.material.color = color;
		this.colorIndex = colorIndex;

		anim.Play("Spawn");
	}
}