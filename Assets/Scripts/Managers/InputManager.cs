using UnityEngine;

/// <summary>Manages the emulation of mobile inputs on PC</summary>
public class InputManager : MonoBehaviour
{
	static InputManager instance;

	[Header("Settings")]
	public float mobileTiltGyroMult;
	public float mobileTiltDragMult;
	public float mobileTiltDragGravity;
	[Space]
	public float pcTiltMult;
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

	public void Init()
	{
		instance = this;
		ResetTilt();
	}

	public static Vector2 GetTilt()
	{
		if (Application.isEditor)
		{
			if (Input.GetKey(instance.pcTiltRight))
				instance.deviceTilt += Vector2.right * instance.pcTiltMult * Time.deltaTime;
			else if (Input.GetKey(instance.pcTiltLeft))
				instance.deviceTilt -= Vector2.right * instance.pcTiltMult * Time.deltaTime;
			else
			{
				instance.deviceTilt.x = Mathf.MoveTowards(
					instance.deviceTilt.x,
					0,
					instance.pcTiltGravity * Time.deltaTime
				);
			}

			if (Input.GetKey(instance.pcTiltUp))
				instance.deviceTilt -= Vector2.up * instance.pcTiltMult * Time.deltaTime;
			else if (Input.GetKey(instance.pcTiltDown))
				instance.deviceTilt += Vector2.up * instance.pcTiltMult * Time.deltaTime;
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
					Input.gyro.enabled = true;
					Vector2 currentRot = Input.gyro.rotationRate; // consider this an unwrapped version of eulerAngles

					instance.deviceTilt += new Vector2(currentRot.y, currentRot.x) * instance.mobileTiltGyroMult;
					break;

				case TiltType.Drag:
					if (Input.touchCount == 0)
					{
						instance.deviceTilt = Vector2.MoveTowards(
							instance.deviceTilt,
							Vector2.zero,
							Time.deltaTime * instance.mobileTiltDragGravity
						);
					}
					else
					{
						Vector2 delta = Input.GetTouch(0).deltaPosition;
						delta.x *= -1; // have to invert X axis
						instance.deviceTilt += delta * instance.mobileTiltDragMult;
					}
					break;
			}
		}

		return instance.deviceTilt;
	}

	public static bool GetPointerUp()
	{
		if (Application.isEditor)
			return Input.GetMouseButtonUp(0);
		else
			return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
	}

	public void ResetTilt()
	{
		deviceTilt = Vector2.zero;
	}
}