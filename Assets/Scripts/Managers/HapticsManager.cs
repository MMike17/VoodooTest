using UnityEngine;

/// <summary>Manages vibrations triggering on the device</summary>
public class HapticsManager : MonoBehaviour
{
	// Thanks Unity : https://docs.unity3d.com/ScriptReference/Handheld.Vibrate.html#:~:text=To%20configure%20advanced%20vibration%20settings%2C%20use%20platform%20specific%20libraries.
	public void Vibrate()
	{
		if (GameManager.save.vibrationsOn)
			Handheld.Vibrate();
	}
}