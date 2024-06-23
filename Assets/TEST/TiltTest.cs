using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TEST
{
	public class TiltTest : MonoBehaviour
	{
		[Header("References")]
		public RectTransform point;
		public RectTransform zone;
		public TMP_Text text;
		public Button resetTilt;
		[Space]
		public InputManager inputManager;

		void Awake()
		{
			GameManager.save = new Save();

			inputManager.Init();
			resetTilt.onClick.AddListener(() => inputManager.ResetTilt());

			inputManager.ResetTilt();
		}

		void Update()
		{
			point.position = zone.position + (Vector3)InputManager.GetTilt();
			text.text = InputManager.GetTilt().ToString();
		}
	}
}