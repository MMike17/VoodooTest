using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages the animations of the CPU in the background</summary>
public class EnvironmentManager : MonoBehaviour
{
	[Header("Settings")]
	public Color normalColor;
	public Color normalGlintColor;
	[Space]
	public Color failColor;
	public Color failGlintColor;
	[Space]
	public Color winColor;
	public Color winGlintColor;
	[Space]
	public float switchDuration;

	[Header("References")]
	public List<Circuit> circuits;
	public SpriteRenderer skullHead;

	public enum ColorTag
	{
		Normal,
		Win,
		Lose
	}

	void Awake() => Restart();

	public void Restart()
	{
		circuits.ForEach(item => item.SetColor(normalColor, normalGlintColor));
		skullHead.color = Color.clear;
	}

	public void SwitchColor(ColorTag tag)
	{
		Color background = tag switch
		{
			ColorTag.Normal => normalColor,
			ColorTag.Win => winColor,
			ColorTag.Lose => failColor
		};

		Color glint = tag switch
		{
			ColorTag.Normal => normalGlintColor,
			ColorTag.Win => winGlintColor,
			ColorTag.Lose => failGlintColor
		};

		StartCoroutine(AnimateSwitch(tag, background, glint));
	}

	IEnumerator AnimateSwitch(ColorTag tag, Color background, Color glint)
	{
		Color.RGBToHSV(
			normalColor,
			out float initBackgroundlH,
			out float initBackgroundS,
			out float initBackgroundV
		);
		Color.RGBToHSV(
			normalGlintColor,
			out float initGlintlH,
			out float initGlintS,
			out float initGlintV
		);
		Color.RGBToHSV(
			background,
			out float tgtBackgroundlH,
			out float tgtBackgroundS,
			out float tgtBackgroundV
		);
		Color.RGBToHSV(
			glint,
			out float tgtGlintlH,
			out float tgtGlintS,
			out float tgtGlintV
		);
		float timer = 0;

		while (timer < switchDuration)
		{
			timer += Time.deltaTime;
			float percent = timer / switchDuration;

			Color currentBackground = Color.HSVToRGB(
				Mathf.Lerp(initBackgroundlH, tgtBackgroundlH, percent),
				Mathf.Lerp(initBackgroundS, tgtBackgroundS, percent),
				Mathf.Lerp(initBackgroundV, tgtBackgroundV, percent)
			);
			Color currentGlint = Color.HSVToRGB(
				Mathf.Lerp(initGlintlH, tgtGlintlH, percent),
				Mathf.Lerp(initGlintS, tgtGlintS, percent),
				Mathf.Lerp(initGlintV, tgtGlintV, percent)
			);

			circuits.ForEach(item => item.SetColor(currentBackground, currentGlint));

			if (tag == ColorTag.Win)
				skullHead.color = Color.Lerp(Color.clear, glint, percent);

			yield return null;
		}

		circuits.ForEach(item => item.SetEnd(background, glint));
		skullHead.color = glint;
	}

	[Serializable]
	public class Circuit
	{
		public Animator anim;
		public SpriteRenderer glint;
		public SpriteRenderer background;

		public void SetColor(Color background, Color glint)
		{
			this.background.color = background;
			this.glint.color = glint;

			if (!anim.GetNextAnimatorStateInfo(0).IsName("Glint"))
				anim.Play("Glint");
		}

		public void SetEnd(Color background, Color glint)
		{
			SetColor(background, glint);
			anim.CrossFade("Finish", 0.5f);
		}
	}
}