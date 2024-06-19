using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>UI panel that can be displayed by the PanelsManager</summary>
public class Panel : MonoBehaviour
{
	[Header("References")]
	public Animator anim;
	public Button closeButton;

	// TODO : Make "Open" animationv
	// TODO : Make "Close" animation

	public void Init(Action OnClose)
	{
		closeButton.onClick.AddListener(() =>
		{
			anim.Play("Close");
			OnClose?.Invoke();
		});
	}

	public void Open() => anim.Play("Open");
}