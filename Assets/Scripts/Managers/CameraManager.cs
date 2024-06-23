using System;
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

	Action ResetTilt;
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

	public void Init(Action resetTilt)
	{
		ResetTilt = resetTilt;
		SetCanTilt(false);
	}

	void Update()
	{
		if (!canTilt)
			return;

		// mapping tilt to min/max while keeping +/- side (will be clamped)
		Vector2 currentTilt = InputManager.GetTilt();
		int xSide = currentTilt.x > 0 ? 1 : -1;
		int ySide = currentTilt.y > 0 ? 1 : -1;

		currentTilt = new Vector2(
			Mathf.InverseLerp(minTiltMagnitude, maxTiltMagnitude, currentTilt.x * xSide) * xSide,
			Mathf.InverseLerp(minTiltMagnitude, maxTiltMagnitude, currentTilt.y * ySide) * ySide
		);

		(Vector3 pos, Vector3 dir) posAndDir = GetCameraPosAndDir(currentTilt);
		mainCamera.transform.position = posAndDir.pos;
		mainCamera.transform.forward = posAndDir.dir;
	}

	(Vector3, Vector3) GetCameraPosAndDir(Vector2 currentTilt)
	{
		// no input
		// if (currentTilt == Vector2.zero)
		// 	return (centerTarget.position, centerTarget.forward);

		// determine which side of the grid are we going towards
		Transform xTarget = centerTarget;
		Transform yTarget = centerTarget;

		if (currentTilt.x > 0)
			xTarget = rightTarget;
		else if (currentTilt.x < 0)
		{
			currentTilt.x *= -1;
			xTarget = leftTarget;
		}

		if (currentTilt.y > 0)
			yTarget = downTarget;
		else if (currentTilt.y < 0)
		{
			currentTilt.y *= -1;
			yTarget = upTarget;
		}

		Vector3 pos = Vector3.Lerp(
			Vector3.Lerp(centerTarget.position, xTarget.position, currentTilt.x),
			Vector3.Lerp(centerTarget.position, yTarget.position, currentTilt.y),
			0.5f
		);

		Vector3 dir = Vector3.Lerp(
			Vector3.LerpUnclamped(centerTarget.forward, xTarget.forward, currentTilt.x),
			Vector3.LerpUnclamped(centerTarget.forward, yTarget.forward, currentTilt.y),
			0.5f
		);

		return (2 * pos - centerTarget.position, 2 * dir - centerTarget.forward);
	}

	// this is really simple and I shouldn't need this but I like to make my code extra explicit on this kind of things
	public void SetCanTilt(bool state)
	{
		canTilt = state;
		ResetTilt();
	}
}