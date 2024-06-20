using UnityEngine;

/// <summary>Cell of the game grid</summary>
public class Cell : MonoBehaviour
{
	[Header("References")]
	public Animator anim;

	void Awake()
	{
		anim.Play("Spawn");
	}
}