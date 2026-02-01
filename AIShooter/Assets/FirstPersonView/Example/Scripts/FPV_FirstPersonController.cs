using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FirstPersonView.Example
{
	[RequireComponent(typeof(CharacterController))]
	public class FPV_FirstPersonController : MonoBehaviour
	{
		public float m_Speed;
		public float m_JumpSpeed;

		private Camera m_Camera;
		private bool m_Jump;
		private Vector2 m_Input;
		private Vector3 m_MoveDir = Vector3.zero;
		private CharacterController m_CharacterController;
		[SerializeField]
		private FPV_MouseLook m_MouseLook;

		private void Start() {
			m_CharacterController = GetComponent<CharacterController>();
			m_Camera = Camera.main;

			m_MouseLook.Init(transform, m_Camera.transform);
		}

		private void Update() {
			UpdateDirection();
			RotatateCamera();

			if (!m_Jump && m_CharacterController.isGrounded)
			{
				m_Jump = UnityEngine.InputSystem.Keyboard.current[Key.Space].wasPressedThisFrame;
				//Input.GetButtonDown("Jump");
			}
		}

		private void FixedUpdate() {
			m_MoveDir.x = m_Input.x * m_Speed;
			m_MoveDir.z = m_Input.y * m_Speed;

			if (m_CharacterController.isGrounded) {
				m_MoveDir.y = 0.0f;

				if (m_Jump) {
					m_MoveDir.y = m_JumpSpeed;
					m_Jump = false;
				}
			}
			else {
				m_MoveDir += Physics.gravity * Time.fixedDeltaTime;
			}

			m_CharacterController.Move(transform.TransformDirection(m_MoveDir) * Time.fixedDeltaTime);
		}

		private bool GetKey(UnityEngine.InputSystem.Key key)
		{
			return (UnityEngine.InputSystem.Keyboard.current[key].isPressed);
		}
		
		private void UpdateDirection() {
			// Read input
			
			//var mouse = UnityEngine.InputSystem.Mouse.current;

			float horizontal = 0;
				//mouse.delta.ReadValue().x;//Input.GetAxis("Horizontal");
			float vertical = 0;//mouse.delta.ReadValue().y;// Input.GetAxis("Vertical");
			if (GetKey(Key.W))
			{
				vertical = 1;
			}
			if (GetKey(Key.S))
			{
				vertical = -1;
			}
			if (GetKey(Key.A))
			{
				horizontal = -1;
			}
			if (GetKey(Key.D))
			{
				horizontal = 1;
			}

			m_Input = new Vector2(horizontal, vertical);

			// normalize input if it exceeds 1 in combined length:
			if (m_Input.sqrMagnitude > 1) {
				m_Input.Normalize();
			}
		}


		private void RotatateCamera() {
			m_MouseLook.LookRotation(transform, m_Camera.transform);
		}
	}
}