using UnityEngine;

// TODO : Add description here
public class InputManager : MonoBehaviour
{
	static InputManager instance;

	[Header("Settings")]
	public float pcTiltSensitivity;
	public float pcTiltGravity;
	public KeyCode pcTiltUp = KeyCode.UpArrow;
	public KeyCode pcTiltDown = KeyCode.DownArrow;
	public KeyCode pcTiltLeft = KeyCode.LeftArrow;
	public KeyCode pcTiltRight = KeyCode.RightArrow;

	static Vector2 deviceTilt;

	void Awake()
	{
		instance = this;
	}

	public static Vector2 GetTilt()
	{
		if (Application.isEditor)
		{
			if (Input.GetKey(instance.pcTiltRight))
				deviceTilt += Vector2.right * instance.pcTiltSensitivity * Time.deltaTime;
			else if (Input.GetKey(instance.pcTiltLeft))
				deviceTilt -= Vector2.right * instance.pcTiltSensitivity * Time.deltaTime;
			else
				deviceTilt.x = Mathf.MoveTowards(deviceTilt.x, 0, instance.pcTiltGravity * Time.deltaTime);

			if (Input.GetKey(instance.pcTiltUp))
				deviceTilt += Vector2.up * instance.pcTiltSensitivity * Time.deltaTime;
			else if (Input.GetKey(instance.pcTiltDown))
				deviceTilt -= Vector2.up * instance.pcTiltSensitivity * Time.deltaTime;
			else
				deviceTilt.y = Mathf.MoveTowards(deviceTilt.y, 0, instance.pcTiltGravity * Time.deltaTime);
		}
		else
		{
			// TODO : Do I need this ?
			Input.gyro.enabled = true;

			// I'm explicitely casting this to Vector2 to remember that I'm getting a Vector3
			deviceTilt = (Vector2)Input.gyro.attitude.eulerAngles;
		}

		return deviceTilt;
	}

	// TODO : Call this from the UI
	public static void ResetTilt() => deviceTilt = Vector2.zero;

	// what do we need here ?

	// continuous press
	// is released this frame
	// is pressed this frame
}