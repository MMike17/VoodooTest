using UnityEngine;

/// <summary>UI panel that can be displayed by the PanelsManager</summary>
public abstract class Panel : MonoBehaviour
{
	[Header("References")]
	public Animator anim;

	public virtual void Open() => anim.Play("Open");
	public void Close() => anim.Play("Close");
}