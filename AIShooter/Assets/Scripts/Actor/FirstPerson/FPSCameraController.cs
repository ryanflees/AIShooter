using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;
using UnityEngine.Rendering;

namespace CR
{
	public class FPSCameraController : MonoBehaviour
	{
		public FPSPlayerController m_PlayerController;
		
		public Transform m_CameraYawTrans;
		public Transform m_CameraPitchTrans;
		public Camera m_Camera;
		//todo, will use FPV Camera for Weapon Rendering
		public Camera m_FPVWeaponCamera;
		
		[Header("Default Sensitivity")]
		public float m_SensitivityHorizontal = 4f;
		public float m_SensitivityVertical = 4f;

		[Range(0, 90)]
		public float m_MaxLookUpAngle = 80f;
		[Range(0, 90)]
		public float m_MaxLookDownAngle = 80f;

		//Sensitivity
		private float m_CurrentSensitivityHorizontal = 1f;
		private float m_CurrentSensitivityVertical = 1f;

		#region Mouse Smoothing
		public enum MouseSmoothType
		{
			None,
			Average,
			Lerp
		}

		public MouseSmoothType m_MouseSmoothType = MouseSmoothType.None;
		
		//Smooth Averaged Rotations
		private static int m_MaxRotCache = 3;
		private float[] m_RotArrayHor = new float[m_MaxRotCache];
		private float[] m_RotArrayVer = new float[m_MaxRotCache];
		private int m_RotCacheCounter = 0;

		//Smooth By Lerp
		public float m_SmoothLerpStrength = 10f;
		private float m_LastMouseRotHor;
		private float m_LastMouseRotVer;
		private float m_TargetMouseRotHor;
		private float m_TargetMouseRotVer;
		#endregion

		#region  Pose
		public float m_CamStandHeight = 1.5f;
		public float m_CamCrouchHeight = 0.7f;
		public float m_CamSlideHeight = 0.5f;

		public bool m_Sliding = false;
		public float m_CamSlidingZOffset = 0.3f;
		private float m_CamCurSlidingZOffset = 0f;

		private float m_CameraShiftTargetYPos = 1.8f;
		private float m_CameraShiftingSpeed = 5f;
		private bool m_CameraShiftingPos = false;
		public float m_CamCurrentBaseHeight = 1.5f;
		#endregion

		

		#region Bobbing
		private Vector3 m_CameraYawBobbingAmount = Vector3.zero;
		//private Vector3 m_CameraYawBobbingAngle = Vector3.zero;
		public Vector3 m_RunPositionBob = Vector3.zero;
		public float m_RunPositionBobCrouchMultiplier = 0.8f;
		public float m_RunPositionBobADSMultiplier = 0.5f;
		[Range(0.1f, 3f)]
		public float m_RunCameraBobYFreq = 2f;

		public Vector3 m_SprintPositionBob = Vector3.zero;
		public Vector3 m_SprintEulerBob = Vector3.zero;
		#endregion

		//recoil
		private Quaternion m_RecoilTargetRot = Quaternion.identity;
		private Quaternion m_CurrentRecoilRotation = Quaternion.identity;
		private float m_KickToTargetTime = 0.1f;
		private float m_KickTimer = 0f;
		private float m_RestoreLerpStrength = 10f;

		//shake
		private Quaternion m_CurrentShakeRotation = Quaternion.identity;
		private Vector3 m_ShakeAmplitude;
		private float m_ShakeDuration;
		private Vector3 m_ShakeFrequency;
		private float m_ShakeTimer = 0f;
		private bool m_ShakingCamera = false;
		public Vector3 m_DefaultShakeAmplitude = new Vector3(0.2f, 0.5f, 0.5f);
		public Vector3 m_DefaultShakeFrequency = new Vector3(20f, 24.5f, 28f);

		//fov
		public float m_DefaultFov = 75f;
		public float m_DefaultADSFov = 40f;
		public float m_DefaultWeaponFov = 60f;
		private float m_FovLerpStrength = 20f;

		private float m_CamCurrentOffsetForward = 0f;
		private float m_CamCurrentOffsetHeight = 0f;
		private Vector3 m_CamCurOffset;
		private Vector3 m_CamCurDefaultOffset;

		//todo art driven animations
		//
		
		public enum RotateDirectionStatus
        {
			None,
			Left,
			Right
        }

		public RotateDirectionStatus m_LastRotateDirectionStatus = RotateDirectionStatus.None;

		#region Offset
		[System.Serializable]
		public class CameraOffsetData
		{
			public Vector3 m_CamDefaultOffset = new Vector3(0f, 0f, 0f);
			public Vector3 m_CamOffsetStand = new Vector3(0f, -0.14f, 0.24f);
			public Vector3 m_CamOffsetStandInAir = new Vector3(0f, -0.14f, 0.24f);
			public Vector3 m_CamOffsetRun = new Vector3(0f, -0.14f, 0.24f);
			public Vector3 m_CamOffsetRunInAir = new Vector3(0f, -0.14f, 0.24f);
			public Vector3 m_CamOffsetSprint = new Vector3(0f, -0.14f, 0.24f);
			public Vector3 m_CamOffsetCrouch = new Vector3(0f, -0.14f, 0.24f);
			public Vector3 m_CamOffsetCrouchRun = new Vector3(0f, -0.14f, 0.24f);
		}
	
		public CameraOffsetData m_CamOffsetUnarmed = new CameraOffsetData();
		public CameraOffsetData m_CamOffsetRifle = new CameraOffsetData();
		public CameraOffsetData m_CamOffsetPistol = new CameraOffsetData();

		private Vector3 m_CamTargetOffset = Vector3.zero;
		private Vector3 m_CamTargetDefaultOffset = Vector3.zero;
		//private Vector3 m_CamCurOffset = Vector3.zero;
		#endregion

		#region Post Effect

		//0~1
		private float m_DamageRatio;
		private float m_TargetDamageRatio;
		private float m_LerpStrength = 1f;

		#endregion
		

		void Awake()
		{
		}

		void Start()
		{
			HideCursor();
			Invoke("CallHideCursor", 0.1f);
		}

		#region Mouse Look
		private void InitRotCache()
        {
			for (int i = 0; i < m_MaxRotCache; i ++)
            {
				m_RotArrayHor[i] = 0f;
				m_RotArrayVer[i] = 0f;
			}
        }

		private float GetAverageRotateHor(float rotateHor)
		{
			//int index = m_RotCacheCounter % m_MaxRotCache;

			m_RotArrayHor[m_RotCacheCounter] = rotateHor;
			//m_RotArrayHor.Add(rotateHor);
			float result = 0f;
			for (int i = 0; i < m_RotArrayHor.Length; i ++)
            {
				result += m_RotArrayHor[i];
			}
			return result / m_RotArrayHor.Length;
		}

		private float GetAverageRotateVer(float rotateVer)
		{
			m_RotArrayVer[m_RotCacheCounter] = rotateVer;
			float result = 0f;
			for (int i = 0; i < m_RotArrayVer.Length; i++)
			{
				result += m_RotArrayVer[i];
			}
			return result / m_RotArrayVer.Length;
		}

		private void IncreateRotCacheCounter()
        {
			m_RotCacheCounter++;
			m_RotCacheCounter %= m_MaxRotCache;
        }
		#endregion

		#region Updates
		public void ManualUpdate(float dt)
        {
	        
	        if (UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.Escape].isPressed)
            {
				ShowCursor();
			}

	  //       if (!MainPlayerController.ConsoleOpen()
	  //           && !(UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			// 	&& UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
			// 	
			// {
			// 	HideCursor();
	  //       }
	        
			if (UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.K].isPressed)      
			{
				HideCursor();
			}
		
			UpdateCameraBobbing(dt);
			UpdateCameraFov(Time.smoothDeltaTime);

			UpdateCameraRotationData(dt);

			UpdateSensitivity(dt);

			if (!Cursor.visible)
			{
				UpdateMouse(dt);
			}

			UpdateCameraHeight(dt);
			UpdateCameraOffset(dt);
			ApplyCameraHeight();

			UpdateCheckIteract(dt);

			UpdatePostProcess(dt);
		}

		private void UpdateSensitivity(float dt)
        {
	        //todo sensitivity might be updated with lerp
	        m_CurrentSensitivityHorizontal = m_SensitivityHorizontal;
			m_CurrentSensitivityVertical = m_SensitivityVertical;
		}

		private void UpdateMouse(float dt)
        {
			var playerActions = RuntimeInputManager.PlayerActions;
			float rotateHor = playerActions.Look.X;
			float rotateVer = -playerActions.Look.Y;

			if (playerActions.ActiveDevice != null
			    && playerActions.ActiveDevice.DeviceClass == InputDeviceClass.Controller)
			{
				//can't find where to set controller sensitivity of InControl
				//so manually apply more sensitivity for controller here
				rotateHor *= 2f;
				rotateVer *= 2f;
			}
			
			if (m_MouseSmoothType == MouseSmoothType.Average)
            {
				rotateHor = GetAverageRotateHor(rotateHor);
				rotateVer = GetAverageRotateVer(rotateVer);
				IncreateRotCacheCounter();
			}
			else if (m_MouseSmoothType == MouseSmoothType.Lerp)
			{
				m_TargetMouseRotHor = rotateHor;
				m_TargetMouseRotVer = rotateVer;
				m_LastMouseRotHor = Mathf.Lerp(m_LastMouseRotHor, m_TargetMouseRotHor, dt * m_SmoothLerpStrength);
				m_LastMouseRotVer = Mathf.Lerp(m_LastMouseRotVer, m_TargetMouseRotVer, dt * m_SmoothLerpStrength);

				rotateHor = m_LastMouseRotHor;
				rotateVer = m_LastMouseRotVer;
			}
			
			rotateHor *= Time.timeScale;
			rotateVer *= Time.timeScale;

			if (rotateHor == 0f && rotateVer == 0f)
			{
				//return;
				//dont do anything
			}
			else
			{
				float hor = m_CurrentSensitivityHorizontal * rotateHor;
				float ver = m_CurrentSensitivityVertical * rotateVer;

				if (m_CameraPitchTrans != null)
				{
					var currentX = m_CameraPitchTrans.localEulerAngles.x;
					if (currentX > 180f)
					{
						currentX -= 360f;
					}
					if (ver > 0f)
					{

						if (currentX < m_MaxLookDownAngle)
						{
							currentX = Mathf.Min(currentX + ver, m_MaxLookUpAngle);
						}
					}
					else if (ver < 0f)
					{
						if (currentX > -m_MaxLookUpAngle)
						{
							currentX = Mathf.Max(currentX + ver, -m_MaxLookUpAngle);
						}
					}

					Vector3 euler = Vector3.zero;
					euler.x = currentX;
					euler.y = m_CameraPitchTrans.localEulerAngles.y;
					euler.z = m_CameraPitchTrans.localEulerAngles.z;
					m_CameraPitchTrans.localRotation = Quaternion.Euler(euler);
				}

				if (m_CameraYawTrans != null)
				{
					m_CameraYawTrans.Rotate(Vector3.up, hor);

					//Debug.Log(hor);
					if (Mathf.Abs(hor) < float.Epsilon)
                    {
						//m_RotateDirectionStatus = RotateDirectionStatus.None;
					}
					else
                    {
						m_LastRotateDirectionStatus = hor > 0 ? RotateDirectionStatus.Right : RotateDirectionStatus.Left;
					}
				}
			}
		}

		public void ManualLateUpdate(float dt)
		{
	        bool isIdle = false;
	        var playerStatus = m_PlayerController.m_PlayerStatus;
	        if (playerStatus.m_IsOnGround &&
	            !playerStatus.m_IsSprinting && playerStatus.m_CharacterRunSpeedNormalized < 0.001f)
	        {
		        isIdle = true;
	        }

	        // if (m_PlayerController.m_WeaponController.CurrentWeapon != null)
	        // {
		       //  if (playerStatus.m_IsADSMode)
		       //  {
			      //   m_FullbodyCameraADT.m_Strength =
				     //    Mathf.Lerp(m_FullbodyCameraADT.m_Strength, 0f,
					    //     Utils.SmoothLerp(dt, 20f));   
		       //  }
		       //  else
		       //  {
			      //   m_FullbodyCameraADT.m_Strength =
				     //    Mathf.Lerp(m_FullbodyCameraADT.m_Strength, m_ArtDrivenWeaponStrength,
					    //     Utils.SmoothLerp(dt, 20f));
		       //  }
	        // }
	        //else
	        // {
		       //  if (playerStatus.m_Crouch && !playerStatus.m_IsSliding)
		       //  {
			      //   m_FullbodyCameraADT.m_Strength =
				     //    Mathf.Lerp(m_FullbodyCameraADT.m_Strength, 0.0f, Utils.SmoothLerp(dt, 20f));
		       //  }
		       //  else
		       //  {
			      //   if (isIdle)
			      //   {
				     //    m_FullbodyCameraADT.m_Strength =
					    //     Mathf.Lerp(m_FullbodyCameraADT.m_Strength, m_ArtDrivenIdleStrength, Utils.SmoothLerp(dt, 20f));
			      //   }
			      //   else
			      //   {
				     //    m_FullbodyCameraADT.m_Strength =
					    //     Mathf.Lerp(m_FullbodyCameraADT.m_Strength, m_ArtDrivenWalkStrength, Utils.SmoothLerp(dt, 20f));
			      //   }
		       //  }
	        // }
	       
	        //m_FullbodyCameraADT.ManualUpdate(dt);
			// if (m_WeaponRootBone != null && m_WeaponCameraBone != null)
   //          {
			// 	Quaternion camBoneRot = m_WeaponCameraBone.rotation;
   //
			// 	m_TargetCameraDrivenRotation = Quaternion.Inverse(m_WeaponRootBone.rotation) * camBoneRot;
			// 	m_CurrentCameraDrivenRotation = Quaternion.Slerp(m_CurrentCameraDrivenRotation, m_TargetCameraDrivenRotation, dt * 10f);
			// }
			// else
   //          {
			// 	m_CurrentCameraDrivenRotation = Quaternion.Slerp(m_CurrentCameraDrivenRotation, Quaternion.identity, dt * 10f);
			// }
			UpdateApplyCameraRotation(dt);
		}

		private float m_CheckInteractTimer = 0f;
		private float m_CheckInteractInterval = 0.2f;
		private void UpdateCheckIteract(float dt)
        {
			m_CheckInteractTimer -= dt;
			if (m_CheckInteractTimer <= 0f)
			{
				m_CheckInteractTimer = m_CheckInteractInterval;

				//todo raycast to check camera pointing to something that can interact
			}
		}

		#endregion

		#region Cursor
		public void CallHideCursor()
        {
			HideCursor();
        }

		public static void HideCursor()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		public static void ShowCursor()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		#endregion

		#region Height & Offset
        public void ShiftCameraHeightPos(float targetYPos)
        {
            m_CameraShiftingPos = true;
            m_CameraShiftTargetYPos = targetYPos;
			//Debug.Log("shift camera to " + targetYPos);
        }

        private void UpdateCameraHeight(float deltaTime)
        {
            if (m_CameraShiftingPos)
            {
				//Debug.Log(m_CameraShiftTargetYPos);
                if (m_CameraShiftTargetYPos < m_CamCurrentBaseHeight)
                {
                    float expectYPos = m_CamCurrentBaseHeight - m_CameraShiftingSpeed * deltaTime;
                    if (expectYPos <= m_CameraShiftTargetYPos)
                    {
                        m_CameraShiftingPos = false;
                        expectYPos = m_CameraShiftTargetYPos;
                    }
                    m_CamCurrentBaseHeight = expectYPos;
                }
                else
                {
                    float expectYPos = m_CamCurrentBaseHeight + m_CameraShiftingSpeed * deltaTime;
                    if (expectYPos >= m_CameraShiftTargetYPos)
                    {
                        m_CameraShiftingPos = false;
                        expectYPos = m_CameraShiftTargetYPos;
                    }
                    m_CamCurrentBaseHeight = expectYPos;
                }
            }
        }

		private void UpdateCameraOffset(float dt)
        {
			if (m_PlayerController.m_CameraPitchTrans != null)
			{
				float pitchAngle = m_PlayerController.m_CameraPitchTrans.localEulerAngles.x;
				

				SetLookDownCamOffset();

				float lerp = 20f;
				m_CamCurDefaultOffset = Vector3.Lerp(m_CamCurDefaultOffset, m_CamTargetDefaultOffset, Utils.SmoothLerp(dt, lerp));
				m_CamCurOffset = Vector3.Lerp(m_CamCurOffset, m_CamTargetOffset,  Utils.SmoothLerp(dt, lerp));

				m_CamCurrentOffsetForward = GetRelaitveZOffset(pitchAngle);
				m_CamCurrentOffsetHeight = GetRelativeYOffset(pitchAngle);
			}
		}

		public void ApplyCameraHeight()
        {
            float expectYPos = m_CamCurrentBaseHeight + m_CamCurrentOffsetHeight;
            //m_PlayerController.m_CameraPitchTrans.localPosition = new Vector3(0f, expectYPos, 0f);
            //float expectYPos = m_CamCurrentBaseHeight + m_CamCurrentOffsetHeight;
            m_PlayerController.m_CameraPitchTrans.localPosition = new Vector3(0f, expectYPos, m_CamCurrentOffsetForward + m_CamCurSlidingZOffset);
        }

        public void ForceSetHeightAsCrouch()
        {
            m_CamCurrentBaseHeight = m_CamCrouchHeight;
            m_CameraShiftingPos = false;
        }

        public void ForceSetHeightAsStand()
        {
            m_CamCurrentBaseHeight = m_CamStandHeight;
            m_CameraShiftingPos = false;
        }

		public void EnabelSliding()
		{
			m_Sliding = true;
		}

		public void DisableSliding()
		{
			m_Sliding = false;
		}

		private float GetRelaitveZOffset(float pitchAngle)
		{
			float res = 0;
			if (pitchAngle > 180f)
            {
				pitchAngle -= 360f;
			}

			if (pitchAngle > 0 && pitchAngle <= 90f)
			{
				float percentage = pitchAngle / 90f;
				res = Mathf.Lerp(m_CamCurDefaultOffset.z,  m_CamCurOffset.z, percentage );
			}
			else if (pitchAngle <= 0f && pitchAngle >= -90f)
            {
				res = m_CamCurDefaultOffset.z;
			}
			return res;
		}

		private float GetRelativeYOffset(float pitchAngle)
		{
			float res = 0;
			if (pitchAngle > 180f)
			{
				pitchAngle -= 360f;
			}

			if (pitchAngle >= 0 && pitchAngle <= 90f)
			{
				float percentage = pitchAngle / 90f;
				//res = percentage * m_CamCurOffset.y;
				res = Mathf.Lerp(m_CamCurDefaultOffset.y, m_CamCurOffset.y, percentage);
			}
			else if (pitchAngle <= 0f && pitchAngle >= -90f)
			{
				res = m_CamCurDefaultOffset.y;
			}
			return res;
		}

		#endregion

		#region Bobbing

		private void UpdateCameraBobbing(float dt)
        {
			var playerStatus = m_PlayerController.m_PlayerStatus;
			if (playerStatus != null && playerStatus.m_IsOnGround)
			{
				float curveAngle = playerStatus.m_BuiltinCurveAngle;
				DebugGraph.Log(curveAngle);
				if (playerStatus.m_CharacterRunSpeedNormalized > 0.01f)
				{
					if (playerStatus.m_Crouch)
					{
						float multiplier = 0.01f * m_RunPositionBobCrouchMultiplier;
						if (playerStatus.m_IsADSMode)
						{
							multiplier *= m_RunPositionBobADSMultiplier;
						}
						Vector3 newPos = new Vector3(
							Mathf.Sin(curveAngle) * m_RunPositionBob.x * multiplier,
							(Mathf.Cos(curveAngle * m_RunCameraBobYFreq)) * m_RunPositionBob.y * multiplier,
							Mathf.Sin(curveAngle) * m_RunPositionBob.z * multiplier);
						m_CameraYawBobbingAmount = Vector3.Lerp(m_CameraYawBobbingAmount, newPos * 0.5f, dt * 3);

						//m_CameraYawBobbingAngle = Vector3.Lerp(m_CameraYawBobbingAngle, Vector3.zero, dt * 3f);
					}
					else
					{
						if (playerStatus.m_IsSprinting)
						{
							float multiplier = 0.01f;

							Vector3 newPos = new Vector3(
								Mathf.Sin(curveAngle) * m_SprintPositionBob.x * multiplier,
								(Mathf.Cos(curveAngle * m_RunCameraBobYFreq)) * m_SprintPositionBob.y * multiplier,
								Mathf.Sin(curveAngle) * m_SprintPositionBob.z * multiplier);
							m_CameraYawBobbingAmount = Vector3.Lerp(m_CameraYawBobbingAmount, newPos, dt * 3);

							float angleMultiplier = 1f;
							Vector3 newEuler = new Vector3(
								Mathf.Sin(curveAngle) * m_SprintEulerBob.x * angleMultiplier,
								(Mathf.Cos(curveAngle * m_RunCameraBobYFreq)) * m_SprintEulerBob.y * angleMultiplier,
								Mathf.Sin(curveAngle) * m_SprintEulerBob.z * angleMultiplier
								);

							//m_CameraYawBobbingAngle = Vector3.Lerp(m_CameraYawBobbingAngle, newEuler, dt * 3f);
						}
						else
						{
							float multiplier = 0.01f;
							if (playerStatus.m_IsADSMode)
							{
								multiplier *= m_RunPositionBobADSMultiplier;
							}
							Vector3 newPos = new Vector3(
								Mathf.Sin(curveAngle) * m_RunPositionBob.x * multiplier,
								(Mathf.Cos(curveAngle * m_RunCameraBobYFreq)) * m_RunPositionBob.y * multiplier,
								Mathf.Sin(curveAngle) * m_RunPositionBob.z * multiplier);
							m_CameraYawBobbingAmount = Vector3.Lerp(m_CameraYawBobbingAmount, newPos, dt * 3);
							//m_CameraYawBobbingAngle = Vector3.Lerp(m_CameraYawBobbingAngle, Vector3.zero, dt * 3f);
						}
					}
				}
				else
				{
					m_CameraYawBobbingAmount = Vector3.Lerp(m_CameraYawBobbingAmount, Vector3.zero, dt * 10);
				}
			}
			else
			{
				m_CameraYawBobbingAmount = Vector3.Lerp(m_CameraYawBobbingAmount, Vector3.zero, dt * 3);
			}
			DebugGraph.Log(m_CameraYawBobbingAmount);
			m_CameraYawTrans.localPosition = m_CameraYawBobbingAmount;
			//m_CameraYawBobbingAngle.y = m_CameraYawTrans.localEulerAngles.y;
			//m_CameraYawTrans.localRotation = Quaternion.Euler(m_CameraYawBobbingAngle);
		}

		#endregion

		#region FOV
        private void UpdateCameraFov(float dt)
		{
			// if (m_PlayerController.m_WeaponController.m_ADSPressed
			// 	&& m_PlayerController.m_WeaponController.CanUseADS())
			// {
			// 	FirstPersonWeaponMecanim weapon = m_PlayerController.m_WeaponController.CurrentWeapon;
			// 	float adsFov = m_DefaultADSFov;
			// 	float adsWeaponFov = m_DefaultWeaponFov;
			// 	if (weapon.HasOverrideSight())
   //              {
			// 		adsFov = m_DefaultFov * weapon.GetADSCamFovMultiplier();
			// 		adsWeaponFov = weapon.GetADSWeaponFov();
			// 		if (adsWeaponFov < 0)
			// 		{
			// 			adsWeaponFov = m_DefaultWeaponFov;
			// 		}
			// 	}
			// 	m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, adsFov, dt * m_FovLerpStrength);
			// 	if (m_FPVWeaponCamera)
			// 	{
			// 		m_FPVWeaponCamera.fieldOfView = 
			// 			Mathf.Lerp(m_FPVWeaponCamera.fieldOfView, adsWeaponFov, dt * m_FovLerpStrength);
			// 	}
			// }
			// else
			// {
			// 	m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, m_DefaultFov, dt * m_FovLerpStrength);
			// 	if (m_FPVWeaponCamera)
			// 	{
			// 		m_FPVWeaponCamera.fieldOfView =
			// 			Mathf.Lerp(m_FPVWeaponCamera.fieldOfView, m_DefaultWeaponFov, dt * m_FovLerpStrength);
			// 	}
			// }
		}
		#endregion

		#region Shake & Recoil
        public void StartCameraShake(float strengthScale, float freqScale, float duration)
		{
			m_ShakeDuration = duration;
			m_ShakeTimer = m_ShakeDuration;
			m_ShakeAmplitude = m_DefaultShakeAmplitude * strengthScale;
			m_ShakeFrequency = m_DefaultShakeFrequency * freqScale;
			m_ShakingCamera = true;
		}

		private void UpdateApplyCameraRotation(float dt)
        {
			Quaternion rotation = m_CurrentRecoilRotation * m_CurrentShakeRotation;
			//rotation = rotation * m_FullbodyCameraADT.m_CurrentRotation;
			m_Camera.transform.localRotation = rotation;
        }

		private void UpdateCameraRotationData(float dt)
		{
			float kickLerp = GetRecoilLerp(dt);
			m_CurrentRecoilRotation = Quaternion.Lerp(m_CurrentRecoilRotation, m_RecoilTargetRot, kickLerp);
			float restoreLerp = GetRestoreLerp(dt);
			m_CurrentRecoilRotation = Quaternion.Lerp(m_CurrentRecoilRotation, Quaternion.identity, restoreLerp);
			
			if (m_ShakeDuration > 0 && m_ShakingCamera)
			{
				float curTime = Time.time;
				float angularX = Mathf.Sin(m_ShakeFrequency.x * curTime);
				float angularY = Mathf.Sin(m_ShakeFrequency.y * curTime);
				float angularZ = Mathf.Sin(m_ShakeFrequency.z * curTime);

				m_ShakeTimer -= dt;
				float shakeStrength = m_ShakeTimer / m_ShakeDuration;
				float shakeX = shakeStrength * angularX * m_ShakeAmplitude.x;
				float shakeY = shakeStrength * angularY * m_ShakeAmplitude.y;
				float shakeZ = shakeStrength * angularZ * m_ShakeAmplitude.z;
				Vector3 euler = new Vector3(shakeX, shakeY, shakeZ);
				m_CurrentShakeRotation = Quaternion.Euler(euler);
				if (m_ShakeTimer <= 0f)
				{
					m_ShakingCamera = false;
				}
			}
			else
			{
				m_CurrentShakeRotation = Quaternion.Lerp(m_CurrentShakeRotation, Quaternion.identity, dt * 20f);
			}

		}

		private float GetRecoilLerp(float dt)
		{
			if (m_KickTimer <= m_KickToTargetTime)
			{
				m_KickTimer += dt;
				float ratio = m_KickTimer / m_KickToTargetTime;
				return ratio;
			}
			return 0f;
		}

		private float GetRestoreLerp(float dt)
		{
			return m_RestoreLerpStrength * dt;
		}
		// public void ExecuteRecoil(FirstPersonWeaponRecoilStatus recoilStatus)
		// {
		// 	m_KickTimer = 0f;
		//
		// 	m_KickToTargetTime = Mathf.Max(recoilStatus.m_KickTime, 0.01f);
		// 	m_RestoreLerpStrength = recoilStatus.m_RestoreLerp;
		//
		// 	float recoilIntensity = recoilStatus.m_RecoilStrength;
		// 	//Quaternion res = GetEndRotation(recoilIntensity, m_Camera.transform.localRotation, recoilStatus.m_LeftSwingStrength, recoilStatus.m_RightSwingStrength);
		// 	Quaternion res = GetEndRotation(recoilIntensity, m_CurrentRecoilRotation, recoilStatus.m_LeftSwingStrength, recoilStatus.m_RightSwingStrength);
		// 	m_RecoilTargetRot = res;
		// }

		private Quaternion GetEndRotation(float recoilIntensity, Quaternion oldRot, float swingLeft, float swingRight)
		{
			Quaternion res = Quaternion.identity;
			res = oldRot * Quaternion.Euler(new Vector3(-recoilIntensity, Random.Range(-swingLeft, swingRight), 0));

			return res;
		}
		#endregion

		#region Animation Driven

		// public void StartFullbodyAnimationDriven(Transform drivenBone, Transform root)
		// {
		// 	m_FullbodyCameraADT.m_Enabled = true;
		// 	m_FullbodyCameraADT.m_CameraBone = drivenBone;
		// 	m_FullbodyCameraADT.m_RootBone = root;
		// }
		//
		// public void SetFullbodyAnimationDrivenUpConstraint(bool enable)
		// {
		// 	m_FullbodyCameraADT.m_EnableUpConstraint = enable;
		// }
		//
		// public void StopFullbodyAnimationDriven()
		// {
		// 	m_FullbodyCameraADT.m_Enabled = false;
		// 	m_FullbodyCameraADT.m_CameraBone = null;
		// 	m_FullbodyCameraADT.m_RootBone = null;
		// }
		//
		// public void StartWeaponAnimationDriven(Transform drivenBone, Transform weaponRoot)
		// {
		// 	m_WeaponCameraADT.m_Enabled = true;
		// 	m_WeaponCameraADT.m_CameraBone = drivenBone;
		// 	m_WeaponCameraADT.m_RootBone = weaponRoot;
		// }
		//
		// public void StopWeaponAnimationDriven()
		// {
		// 	m_WeaponCameraADT.m_Enabled = false;
		// 	m_WeaponCameraADT.m_CameraBone = null;
		// 	m_WeaponCameraADT.m_RootBone = null;
		// }

		#endregion

		#region Camera Offset
		private void SetLookDownCamOffset()
		{
			CameraOffsetData offsetData = GetCurrentCameraOffsetData();
			if (offsetData != null)
			{
				m_CamTargetDefaultOffset = offsetData.m_CamDefaultOffset;

				Vector3 addOffset = Vector3.zero;
				var playerStatus = m_PlayerController.m_PlayerStatus;
				if (playerStatus.m_IsOnGround)
				{
					if (playerStatus.m_IsSprinting)
					{
						addOffset = offsetData.m_CamOffsetSprint;

						//sprint will override the default offset
						//m_CamTargetDefaultOffset = offsetData.m_CamOffsetSprint;
					}
					else
					{
						if (playerStatus.m_CharacterRunSpeedNormalized > 0.001f)
						{
							if (playerStatus.m_Crouch)
							{
								addOffset = offsetData.m_CamOffsetCrouchRun;
							}
							else
							{
								addOffset = offsetData.m_CamOffsetRun;
							}
						}
						else
						{
							if (playerStatus.m_Crouch)
							{
								addOffset = offsetData.m_CamOffsetCrouch;
							}
							else
							{
								addOffset = offsetData.m_CamOffsetStand;
							}
						}
					}
				}
				else
				{
					if (playerStatus.m_CharacterRunSpeedNormalized > 0.001f)
					{
						if (playerStatus.m_Crouch)
						{
							addOffset = offsetData.m_CamOffsetCrouchRun;
						}
						addOffset = offsetData.m_CamOffsetRunInAir;
					}
					else
					{
						if (playerStatus.m_Crouch)
						{
							addOffset = offsetData.m_CamOffsetCrouch;
						}
						addOffset = offsetData.m_CamOffsetStandInAir;
					}
				}

				m_CamTargetOffset = addOffset;
			}
		}

		private CameraOffsetData GetCurrentCameraOffsetData()
		{
			// if (m_PlayerController.Observer != null)
   //          {
			// 	var locomotionType = m_PlayerController.Observer.GetDummyLocomotion(m_PlayerController);
   //
			// 	if (locomotionType == DummyCharacter.LocomotionType.Unarmed)
			// 	{
			// 		return m_CamOffsetUnarmed;
			// 	}
			// 	else if (locomotionType == DummyCharacter.LocomotionType.Pistol)
			// 	{
			// 		return m_CamOffsetPistol;
			// 	}
			// 	else if (locomotionType == DummyCharacter.LocomotionType.Rifle)
			// 	{
			// 		return m_CamOffsetRifle;
			// 	}
			// }
			return null;
		}
        #endregion

        #region Post Process

        #region Post Process

        private void UpdatePostProcess(float dt)
        {
        }
        #endregion

        #endregion
	}
}
