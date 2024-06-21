using UnityEngine;

/// <summary>UI panel that can be displayed by the PanelsManager</summary>
public abstract class Panel : MonoBehaviour
{
	[Header("References")]
	public Animator anim;

	// TODO : Make "Open" animation
	// TODO : Make "Close" animation

	public void Open() => anim.Play("Open");
}