using System;
using UnityEngine;

namespace FirstPersonView.Example
{
	[Serializable]
	public class FPV_MouseLook
	{
		public float XSensitivity = 0.2f;
		public float YSensitivity = 0.2f;
		public bool clampVerticalRotation = true;
		public float MinimumX = -90F;
		public float MaximumX = 90F;

		private Quaternion m_CharacterTargetRot;
		private Quaternion m_CameraTargetRot;

		public void Init(Transform character, Transform camera) {
			m_CharacterTargetRot = character.localRotation;
			m_CameraTargetRot = camera.localRotation;
		}

		public void LookRotation(Transform character, Transform camera) {
			
			var mouse = UnityEngine.InputSystem.Mouse.current;
		
			float yRot = mouse.delta.ReadValue().x * XSensitivity;
			float xRot = mouse.delta.ReadValue().y * YSensitivity;
			//
			// float yRot = Input.GetAxis("Mouse X") * XSensitivity;
			// float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

			m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
			m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

			if (clampVerticalRotation)
				m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

			character.localRotation = m_CharacterTargetRot;
			camera.localRotation = m_CameraTargetRot;
		}

		Quaternion ClampRotationAroundXAxis(Quaternion q) {
			q.x /= q.w;
			q.y /= q.w;
			q.z /= q.w;
			q.w = 1.0f;

			float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

			angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

			q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

			return q;
		}

	}
}