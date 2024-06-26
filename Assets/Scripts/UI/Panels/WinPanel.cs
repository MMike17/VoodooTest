using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static AudioManager;

public class WinPanel : Panel
{
	[Header("Settings")]
	public float durationPerStar;

	[Header("References")]
	public Transform starsHolder;
	public Animator starPrefab;
	public TMP_Text starsCount;
	public Button mainMenuButton;

	Func<int> GetStars;
	Action<SoundTag> PlaySound;

	public void Init(Action ToMainMenu, Func<int> getStars, Action<SoundTag> playSound)
	{
		GetStars = getStars;
		PlaySound = playSound;

		mainMenuButton.onClick.AddListener(() => ToMainMenu());
	}

	public override void Open()
	{
		base.Open();
		StartCoroutine(AnimateStars());
	}

	IEnumerator AnimateStars()
	{
		starsCount.text = "0";

		foreach (Transform star in starsHolder)
			Destroy(star.gameObject);

		yield return new WaitUntil(() =>
		{
			AnimatorStateInfo info = base.anim.GetCurrentAnimatorStateInfo(0);
			return info.IsName("Open") && info.normalizedTime > 0.95f;
		});

		int targetCount = GetStars();
		int count = 0;
		float timer = 0;

		starsCount.text = count.ToString();

		while (count < targetCount)
		{
			timer += Time.deltaTime;

			while (timer > durationPerStar)
			{
				timer -= durationPerStar;
				count++;

				// TODO : Pool this (we're going to have a bunch of them)
				Instantiate(starPrefab, starsHolder);
				PlaySound(SoundTag.Star);
			}

			starsCount.text = count.ToString();
			yield return null;
		}
	}
}