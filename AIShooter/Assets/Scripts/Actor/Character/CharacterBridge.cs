using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CR
{
	public class CharacterBridge : MonoBehaviour, FPSPlayerControllerObserver
	{
		public FPSPlayerController m_Player;
		public DummyCharacter m_DummyCharacter;

		#region Direction
		//public float m_LookAtTargetHeight = 1.5f;
		private Vector3 m_LookDirection = Vector3.forward;
		private Vector3 m_LookDirectionProj = Vector3.forward;
		private Transform m_LookAtTarget;
		private static float LOOK_AT_TARGET_RANGE = 30f;
		private LookSpeedChecker m_LookSpeedChecker;
		
		private bool m_IsSnappingToView = false;
		public float m_SnapSmoothing = 4f;
		public float m_TurnThreholdAngle = 70f;

		private Queue<Vector3> m_LookDirProjBuffer = new Queue<Vector3>();
		#endregion
		
		
		void Awake()
		{
			Init();
		}

		void Start()
		{
		}

		public void Init()
		{
			if (m_Player)
			{
				m_Player.Observer = this;
				m_Player.OnCrouch = OnCallCrouch;
				m_Player.OnStand = OnCallStand;
				m_Player.OnJumpStart = OnJumpStarted;	
				m_Player.OnJumpEnd = OnJumpEnd;
				// m_Player.OnSlideStart = OnSlideStart;
				// m_Player.OnSlideEnd = OnSlideEnd;
				//
			}

			InitLookAtDirection();
		}
		
		private void InitLookAtDirection()
		{
			if (m_Player && m_DummyCharacter != null)
			{
				Vector3 velocity = m_Player.m_KinematicController.Motor.Velocity;
				Vector3 forwardEuler = m_Player.GetPlayerRotationEuler();
				Vector3 forwardDir = Quaternion.Euler(forwardEuler) * Vector3.forward;
				
				m_LookDirection = forwardDir;
				m_LookDirectionProj = forwardDir;
				
				m_LookDirProjBuffer.Enqueue(m_LookDirectionProj);
				
				GameObject lookAtTargetObj = new GameObject();
				lookAtTargetObj.name = "LookAtTarget";
				m_LookAtTarget = lookAtTargetObj.transform;
				m_LookAtTarget.transform.parent = m_Player.transform;
				
				m_DummyCharacter.m_CharacterIK.SetLookAtIK(m_LookAtTarget);
				m_DummyCharacter.m_CharacterIK.EnableLookAtIK();
				
				m_LookSpeedChecker = new LookSpeedChecker();
			}
			
			// if (m_Player && m_DummyCharacter != null)
			// {
			// 	Vector3 euler = m_Player.GetPlayerRotationEuler();
			// 	euler.x = 0f;
			// 	euler.z = 0f;
			// 	m_DummyCharacter.transform.rotation = Quaternion.Euler(euler);
			//
			// 	Vector3 velocity = m_Player.m_KinematicController.Motor.Velocity;
			// 	Vector3 forwardEuler = m_Player.GetPlayerRotationEuler();
			// 	Vector3 forwardDir = Quaternion.Euler(forwardEuler) * Vector3.forward;
			//
			// 	m_LookDirection = forwardDir;
			// 	m_LookDirectionTrails.Enqueue(m_LookDirection);
			//
			// 	GameObject lookAtTargetObj = new GameObject();
			// 	lookAtTargetObj.name = "LookAtTarget";
			// 	m_LookAtTarget = lookAtTargetObj.transform;
			// 	m_LookAtTarget.transform.parent = m_Player.transform;
			// 	UpdateLookAtTargetPosition();
			//
			// 	m_DummyCharacter.m_IKController.SetLookAtIK(m_LookAtTarget);
			// 	m_DummyCharacter.m_IKController.EnableLookAtIK();
			//
			// 	GameObject aimTargetObj = new GameObject();
			// 	aimTargetObj.name = "AimTarget";
			// 	m_AimTarget = aimTargetObj.transform;
			// 	m_AimTarget.transform.parent = m_Player.transform;
			// 	UpdateAimTargetPosition();
			// 	m_DummyCharacter.m_IKController.SetAimIKTarget(m_AimTarget);
			// }
			// m_LookSpeedChecker = new LookSpeedChecker();
			// if (m_DummyCharacter)
			// {
			// 	m_DummyCharacter.m_IKController.EnableLookAtIK();
			// }
		}


		#region Updates
		private void UpdateLookAtTargetPosition()
		{
			if (m_LookAtTarget != null)
			{
				Vector3 offset = 1.5f * Vector3.up;
				offset = Quaternion.FromToRotation(Vector3.down, m_Player.GetGravity()) * offset;
				Vector3 playerCenter = m_Player.GetPlayerPosition() + offset;
				Vector3 pos = m_LookDirection.normalized * LOOK_AT_TARGET_RANGE + playerCenter;
				m_LookAtTarget.transform.position = pos;
			}
		}

		private void InternalUpdate(float dt)
		{
		}


		private void InternalLateUpdate(float dt)
		{
			UpdateLookAtTargetPosition();
			
			if (m_LookSpeedChecker != null)
			{
				m_LookSpeedChecker.UpdateLookSpeed(dt, m_LookDirection);
			}
			
			if (m_Player)
			{
				Vector3 playerDirProj = Vector3.ProjectOnPlane(m_Player.m_CameraPitchTrans.forward,
					-m_Player.m_KinematicController.m_Gravity);
				m_LookDirectionProj = playerDirProj;
				m_LookDirection = m_Player.m_CameraPitchTrans.forward;
				
				m_LookDirProjBuffer.Enqueue(m_LookDirectionProj);
				if (m_LookDirProjBuffer.Count > 3)
				{
					m_LookDirProjBuffer.Dequeue();
				}
			}
			UpdateDummyCharacter(dt);
		}

		private void UpdateDummyCharacter(float dt)
		{
			if (m_DummyCharacter && m_Player)
			{
				Vector3 gravityUp = -m_Player.GetGravity();

				Vector3 velocity = m_Player.m_KinematicController.Motor.BaseVelocity;
				velocity = Vector3.ProjectOnPlane(velocity, gravityUp);

				m_DummyCharacter.transform.position = m_Player.GetPlayerPosition();

				// if (velocity.sqrMagnitude > 0.01f)
				// {
				// 	Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, gravityUp);
				// 	m_DummyCharacter.transform.rotation = Quaternion.Slerp(
				// 		m_DummyCharacter.transform.rotation,
				// 		targetRotation,
				// 		dt * 10f
				// 	);
				// }
				//
				// float speed = velocity.magnitude;
				// bool isGrounded = m_Player.m_KinematicController.Motor.GroundingStatus.IsStableOnGround;
				// bool isCrouching = m_Player.m_PlayerStatus.m_Crouch;
				// bool isSprinting = m_Player.m_PlayerStatus.m_IsSprinting;
				//
				// m_DummyCharacter.UpdateAnimationState(speed, isGrounded, isCrouching, isSprinting);
				//
		
				////////////////
				///update rotation
				if (CheckPlayerBeingStillOnGround())
				{
					if (m_IsSnappingToView)
					{
						Vector3 forward = GetCharacterForwardDirection();
						Vector3 targetDir = m_LookDirProjBuffer.Peek(); //m_LookDirectionProj
						
						Quaternion forwardRot = Quaternion.LookRotation(forward, gravityUp);
						Quaternion lookRot = Quaternion.LookRotation(targetDir, gravityUp);
						float lerpStrength = m_SnapSmoothing;
						float lerpMultiplier = m_LookSpeedChecker.AverageLookSpeed / 180f;
						lerpStrength = lerpStrength * Mathf.Max(lerpMultiplier, 1f);

						float lerp = Utils.SmoothLerp(dt, lerpStrength);
						Quaternion resRot = Quaternion.Lerp(forwardRot, lookRot, lerp);

						m_DummyCharacter.transform.rotation = resRot;

						float resAngle = Quaternion.Angle(resRot, lookRot);
						if (resAngle < 1f)
						{
							m_IsSnappingToView = false;
							m_DummyCharacter.SetTurnTagOff();
						}
						else if (resAngle > 16f || m_LookSpeedChecker.AverageLookSpeed > 10f)
						{
							float signedAngle = Vector3.SignedAngle(m_LookDirection, forward, -gravityUp);
							m_DummyCharacter.PlayTurn(signedAngle);
						}
					}
					else
					{
						Vector3 lookProj = Vector3.ProjectOnPlane(m_LookDirectionProj, gravityUp).normalized;
						Vector3 forward = GetCharacterForwardDirection().normalized;
						//check is face direction and character for
						
						if (Vector3.Angle(lookProj, forward) > m_TurnThreholdAngle)
						{
							m_IsSnappingToView = true;

							float signedAngle = Vector3.SignedAngle(lookProj, forward, -gravityUp);
							m_DummyCharacter.PlayTurn(signedAngle);
						}
					}
				}
				else
				{
					Vector3 forward = GetCharacterForwardDirection().normalized;
					Vector3 targetFaceDir = m_LookDirectionProj;
					Quaternion forwardRot = Quaternion.LookRotation(forward, gravityUp);
					Quaternion faceRot = Quaternion.LookRotation(targetFaceDir, gravityUp);

					float lerpStrength = 30f;

					//if (m_Player.m_KinematicController.IsSprinting())
					{
						// Vector3 velocityProj = Vector3.ProjectOnPlane(velocity.normalized, gravityUp).normalized;
						// Quaternion vRot = Quaternion.LookRotation(velocityProj, gravityUp);
						// Quaternion resRot = Quaternion.Lerp(forwardRot, vRot, dt * lerpStrength);
						// m_DummyCharacter.transform.rotation = resRot;
					}
					// else
					// {
					Quaternion resRot = Quaternion.Lerp(forwardRot, faceRot, dt * lerpStrength);
					m_DummyCharacter.transform.rotation = resRot;
					// }
					// m_DummyCharacter.DisableDetailLayerLerp(dt);
					// m_DummyCharacter.BreakTurn();
					// m_DummyCharacter.BreakSwitchCrouchStand();
					// if (m_DummyCharacter.IsPlayingJumpEnd())
					// {
					// 	m_DummyCharacter.PlayLocomotion();
					// }
					// if (m_DummyCharacter.IsPlayingShowHideWeaponBase())
					// {
					// 	m_DummyCharacter.PlayLocomotion();
					// }
				}
				////////////////
				
				
			}
		}
		#endregion

		#region Observer

		public void ManualUpdate(float dt)
		{
			InternalUpdate(dt);
		}

		public void ManualLateUpdate(float dt)
		{
			InternalLateUpdate(dt);
		}

		public Transform GetCharacterRoot()
		{
			return null;
		}

		#endregion
		
		#region Status

		private bool CheckPlayerBeingStillOnGround()
		{
			Vector3 velocity = m_Player.m_KinematicController.Motor.BaseVelocity;
			velocity.y = 0f;
			bool stillOnGround = false;
			if (velocity.magnitude < 0.0001f && m_Player.IsOnGround())
			{
				stillOnGround = true;
			}
			return stillOnGround;
		}

		private Vector3 GetCharacterForwardDirection()
		{
			if (m_DummyCharacter != null)
			{
				Vector3 forward = m_DummyCharacter.transform.rotation * Vector3.forward;
				Vector3 forwardProj = Vector3.ProjectOnPlane(forward, -m_Player.GetGravity());
				return forwardProj;
			}
			return Vector3.forward;
		}
		
		#endregion

		#region Animator Events

		public void EvtPlayTurnFinished()
		{
			if (m_DummyCharacter)
			{
				m_DummyCharacter.SetTurnTagOff();
			}
		}

		#endregion
		
		#region Player Events

		private void OnCallCrouch()
		{
			// // if (m_NetworkObserver != null)
			// // {
			// // 	m_NetworkObserver.CallCrouch();
			// // }
			if (m_DummyCharacter)
			{
				bool playDetail = false; //CheckPlayerBeingStillOnGround();
				m_DummyCharacter.Play2Crouch(playDetail, m_Player.IsOnGround());
			}
		}

		private void OnCallStand()
		{
			// if (m_NetworkObserver != null)
			// {
			// 	m_NetworkObserver.CallStand();
			// }
			if (m_DummyCharacter)
			{
				bool playDetail = false;// CheckPlayerBeingStillOnGround();
				m_DummyCharacter.Play2Stand(playDetail);
			}
		}

		private void OnJumpStarted()
		{
			// if (m_DummyCharacter == null) return;
			// m_DummyCharacter.PlayJumpStart();
			// // if (m_NetworkObserver != null)
			// // {
			// // 	m_NetworkObserver.CallJumpStart();
			// // }
		}

		private void OnJumpEnd()
		{
			// if (m_DummyCharacter == null) return;
			// 	
			// if (!m_DummyCharacter.IsPlayingCrouchLocomotion())
			// {
			// 	if (CheckPlayerBeingStillOnGround())
			// 	{
			//
			// 		m_DummyCharacter.PlayJumpEnd();
			// 	}
			// 	else
			// 	{
			// 		m_DummyCharacter.PlayLocomotion();
			// 	}
			// }
			// 		
			// // if (m_NetworkObserver != null)
			// // {
			// // 	m_NetworkObserver.CallJumpEnd();
			// // }
		}
		#endregion
	}


}
