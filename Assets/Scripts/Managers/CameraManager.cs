using UnityEngine;

/// <summary>Manages procedural placement of the camera depending on tilt</summary>
public class CameraManager : MonoBehaviour
{
	[Header("Settings")]
	public float minTiltMagnitude;
	public float maxTiltMagnitude;

	[Header("References")]
	public Camera mainCamera;
	[Space]
	public Transform centerTarget;
	public Transform upTarget;
	public Transform downTarget;
	public Transform leftTarget;
	public Transform rightTarget;

	bool canTilt;

	void OnDrawGizmos()
	{
		GizmosSettings settings = GizmosSettings.Get();

		if (!settings.enableCamera)
			return;

		Gizmos.color = settings.camTargetColor;

		if (centerTarget != null)
		{
			Gizmos.DrawSphere(centerTarget.position, settings.camTargetSize);
			Gizmos.DrawLine(centerTarget.position, centerTarget.position + centerTarget.forward);

			if (leftTarget != null)
			{
				Gizmos.DrawSphere(leftTarget.position, settings.camTargetSize);
				Gizmos.DrawLine(centerTarget.position, leftTarget.position);
				Gizmos.DrawLine(leftTarget.position, leftTarget.position + leftTarget.forward);
			}

			if (rightTarget != null)
			{
				Gizmos.DrawSphere(rightTarget.position, settings.camTargetSize);
				Gizmos.DrawLine(centerTarget.position, rightTarget.position);
				Gizmos.DrawLine(rightTarget.position, rightTarget.position + rightTarget.forward);
			}

			if (upTarget != null)
			{
				Gizmos.DrawSphere(upTarget.position, settings.camTargetSize);
				Gizmos.DrawLine(centerTarget.position, upTarget.position);
				Gizmos.DrawLine(upTarget.position, upTarget.position + upTarget.forward);
			}

			if (downTarget != null)
			{
				Gizmos.DrawSphere(downTarget.position, settings.camTargetSize);
				Gizmos.DrawLine(centerTarget.position, downTarget.position);
				Gizmos.DrawLine(downTarget.position, downTarget.position + downTarget.forward);
			}
		}

		if (
			centerTarget != null &&
			upTarget != null &&
			downTarget != null &&
			leftTarget != null &&
			rightTarget != null
		)
		{
			(Vector3 pos, Vector3 dir) posAndDir = GetCameraPosAndDir(
				new Vector2(settings.simulatedXTilt,
				settings.simulatedYTilt)
			);

			Gizmos.color = settings.camTargetSimColor;
			Gizmos.DrawSphere(posAndDir.pos, settings.camTargetSimSize);
		}
	}

	public void Init()
	{
		SetCanTilt(false);
	}

	void Update()
	{
		if (!canTilt)
			return;

		// mapping tilt to min/max (will be clamped)
		Vector2 currentTilt = InputManager.GetTilt();
		currentTilt = new Vector2(
			Mathf.Lerp(0, 1, Mathf.InverseLerp(minTiltMagnitude, maxTiltMagnitude, currentTilt.x)),
			Mathf.Lerp(0, 1, Mathf.InverseLerp(minTiltMagnitude, maxTiltMagnitude, currentTilt.y))
		);

		(Vector3 pos, Vector3 dir) posAndDir = GetCameraPosAndDir(currentTilt);
		mainCamera.transform.position = posAndDir.pos;
		mainCamera.transform.forward = posAndDir.dir;
	}

	// TODO : This gives weird values and is probably broken
	(Vector3, Vector3) GetCameraPosAndDir(Vector2 currentTilt)
	{
		// determine which side of the grid are we going towards
		Transform xTarget;
		Transform yTarget;

		if (currentTilt.x > 0)
			xTarget = rightTarget;
		else
		{
			currentTilt.x *= -1;
			xTarget = leftTarget;
		}

		if (currentTilt.y > 0)
			yTarget = upTarget;
		else
		{
			currentTilt.y *= -1;
			yTarget = downTarget;
		}

		// this is weird, it's made to get a form of spherized Lerp on position
		float tiltRatio = currentTilt.y / (currentTilt.x + currentTilt.y);

		return (
			Vector3.Lerp(xTarget.position, yTarget.position, tiltRatio),
			Vector3.LerpUnclamped(xTarget.forward, yTarget.forward, tiltRatio)
		);
	}

	// this is really simple and I shouldn't need this but I like to make my code extra explicit on this kind of things
	public void SetCanTilt(bool state)
	{
		canTilt = state;
		InputManager.ResetTilt();
	}
}