using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages sound sources for the game</summary>
public class AudioManager : MonoBehaviour
{
	[Header("Settings")]
	public List<AudioSettings> settings;
	public int prewarmSize;

	[Header("References")]
	public AudioSource sourcePrefab;

	public enum SoundTag
	{
		Element_Destruction,
		Element_Selection,
		Win,
		Lose
	}

	List<AudioSource> pool;

	public void Init()
	{
		pool = new List<AudioSource>();

		for (int i = 0; i < prewarmSize; i++)
		{
			pool.Add(GetSource());
			pool[^1].enabled = false;
		}
	}

	AudioSource GetSource()
	{
		AudioSource source = pool.Find(item => item.enabled == false);

		if (source == null)
		{
			source = Instantiate(sourcePrefab, transform);
			pool.Add(source);
		}

		source.enabled = true;
		return source;
	}

	IEnumerator DisableOnDone(AudioSource source)
	{
		yield return new WaitForSeconds(source.clip.length);
		source.enabled = false;
	}

	public void PlaySound(SoundTag tag)
	{
		if (!GameManager.save.soundOn)
			return;

		AudioSettings selected = settings.Find(item => item.tag == tag);

		if (selected == null)
		{
			Debug.LogError("Couldn't find settings for tag \"" + tag + "\"");
			return;
		}

		AudioSource source = GetSource();

		source.clip = selected.clip;
		source.volume = selected.volume;

		source.Play();
		StartCoroutine(DisableOnDone(source));
	}

	[Serializable]
	public class AudioSettings
	{
		public SoundTag tag;
		public AudioClip clip;
		public float volume;
	}
}