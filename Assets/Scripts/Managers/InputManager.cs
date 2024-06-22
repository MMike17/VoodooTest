using UnityEngine;

/// <summary>Manages the emulation of mobile inputs on PC</summary>
public class InputManager : MonoBehaviour
{
	static InputManager instance;

	[Header("Settings")]
	public float mobileTiltSensitivity;
	public float pcTiltSensitivity;
	public float pcTiltGravity;
	public KeyCode pcTiltUp = KeyCode.UpArrow;
	public KeyCode pcTiltDown = KeyCode.DownArrow;
	public KeyCode pcTiltLeft = KeyCode.LeftArrow;
	public KeyCode pcTiltRight = KeyCode.RightArrow;

	public enum TiltType
	{
		Gyroscope,
		Drag
	}

	Vector2 deviceTilt;

	void Awake()
	{
		instance = this;
	}

	public static Vector2 GetTilt()
	{
		if (Application.isEditor)
		{
			if (Input.GetKey(instance.pcTiltRight))
				instance.deviceTilt += Vector2.right * instance.pcTiltSensitivity * Time.deltaTime;
			else if (Input.GetKey(instance.pcTiltLeft))
				instance.deviceTilt -= Vector2.right * instance.pcTiltSensitivity * Time.deltaTime;
			else
			{
				instance.deviceTilt.x = Mathf.MoveTowards(
					instance.deviceTilt.x,
					0,
					instance.pcTiltGravity * Time.deltaTime
				);
			}

			if (Input.GetKey(instance.pcTiltUp))
				instance.deviceTilt += Vector2.up * instance.pcTiltSensitivity * Time.deltaTime;
			else if (Input.GetKey(instance.pcTiltDown))
				instance.deviceTilt -= Vector2.up * instance.pcTiltSensitivity * Time.deltaTime;
			else
			{
				instance.deviceTilt.y = Mathf.MoveTowards(
					instance.deviceTilt.y,
					0,
					instance.pcTiltGravity * Time.deltaTime
				);
			}
		}
		else
		{
			switch (GameManager.save.tiltType)
			{
				case TiltType.Gyroscope:
					// TODO : Do I need this ?
					Input.gyro.enabled = true;

					// I'm explicitely casting this to Vector2 to remember that I'm getting a Vector3
					instance.deviceTilt = (Vector2)Input.gyro.attitude.eulerAngles;
					break;

				case TiltType.Drag:
					if (Input.touchCount == 0)
						instance.deviceTilt = Vector2.zero;
					else
						instance.deviceTilt += Input.GetTouch(0).deltaPosition * instance.mobileTiltSensitivity;
					break;
			}
		}

		return instance.deviceTilt;
	}

	// TODO : Call this from the UI
	public void ResetTilt() => deviceTilt = Vector2.zero;

	// what do we need here ?

	// continuous press
	// is released this frame
	// is pressed this frame
}