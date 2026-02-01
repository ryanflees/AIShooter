using System;
using UnityEngine;
using KinematicCharacterController;

namespace CR
{
    /// <summary>
    /// Main player controller that integrates FPS camera and character movement
    /// Handles input and coordinates between camera and character systems
    /// Uses RuntimeInputManager for centralized input management
    /// </summary>
    public class FPSPlayerController : MonoBehaviour, KinematicObserver
    {
        public PlayerStatus m_PlayerStatus = new PlayerStatus();
        
        [Header("References")]
        public FPSKinematicController m_KinematicController;
        public FPSCameraController m_CameraController;

        public Transform m_CharacterRoot; //A root transform for Character models and Cameras
        public Transform m_CameraYawTrans;
        public Transform m_CameraPitchTrans;

        #region Move Parameters
        private Vector3 m_TargetMoveInputVector = Vector3.zero;
        private Vector3 m_MoveInputVector = Vector3.zero;
        public float m_MoveInputLerp = 100f;
        private Vector3 m_WorldspaceMoveInputVector = Vector3.zero;
        #endregion

        #region Status
        private float m_CharacterStandHeight;
        public float m_CharacterCrouchHeight = 1f;

        private bool m_AllowCrouch = true;
        private bool m_AllowSprint = true;
        private bool m_AllowSlide = true;
        private bool m_SprintPressed;
        
        private float m_SprintLockMaxTime = 0.07f;
        private float m_SprintLockTimer = 0f;
        
        private float m_JustLandTimer = 0f;
        private float m_JustLandMax = 0.1f;
        private float m_SlidingCDTimer = 0f;
        private float m_SlidingCD = 1f;
        private float m_InAirTimer = 0f;
        #endregion

        #region Fall Check
        private Vector3 m_InAirHighest = Vector3.zero;
        private Vector3 m_InAirGravity = -Vector3.one;
        public float m_FallDamageDistance = 5.5f;
        public float m_FallDamageMaxDistance = 15f;
        #endregion

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            // Lock and hide cursor for FPS
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
        
        #region Init
        public void Init()
        {
            InitKinematic();
            InitPoseParams();
            InitBobCurve();
        }

        private void InitBobCurve()
        {
            // if (m_PlayerBobCurve == null)
            // {
            //     m_PlayerBobCurve = new PlayBobCurve();
            // }
            // m_PlayerBobCurve.m_PlayerController = this;
        }

        private void InitKinematic()
        {
            if (m_KinematicController != null)
            {
                m_KinematicController.Observer = this;
            }
        }

        private void InitPoseParams()
        {
            m_CharacterStandHeight = m_KinematicController.Motor.Capsule.height;

        }
        #endregion

        private void Update()
        {
            float dt = Time.deltaTime;

            UpdateTimers(dt);
            
            if (IsAlive())
            {
                m_CameraController.ManualUpdate(dt);
            
                //todo check is ~ console is open, if not then input is allowed
                HandleInput(dt);
            }
        }

        private void LateUpdate()
        {
            // Update camera after character movement
            float dt = Time.deltaTime;

            UpdatePlayerRoot(dt);
            
            if (IsAlive())
            {
            }
        }

        private void UpdateTimers(float dt)
        {
            if (m_JustLandTimer > 0f)
            {
                m_JustLandTimer -= dt;
            }
            if (m_SlidingCDTimer > 0f)
            {
                m_SlidingCDTimer -= dt;
            }
            if (!m_KinematicController.IsOnGround())
            {
                m_InAirTimer += dt;

                Vector3 inAirHeightOffset = GetPlayerPosition() - m_InAirHighest;
                Vector3 inAirOffsetProj = Vector3.Project(inAirHeightOffset, m_InAirGravity);
                if (Vector3.Dot(inAirOffsetProj, m_InAirGravity) > 0f)
                {
                    inAirHeightOffset = GetPlayerPosition();
                }
            }
        }
        
        private void HandleInput(float dt)
        {
            // Get input from RuntimeInputManager
            PlayerInputActions inputActions = RuntimeInputManager.PlayerActions;
            if (inputActions == null)
                return;
      
            UpdateMovementInput(dt);
            ApplyMovementInput(dt);

            UpdateActionInput(dt);
            CheckSprintTakeEffect();
        }

        #region Movement Input
        private void UpdateMovementInput(float dt)
        {
            PlayerInputActions inputActions = RuntimeInputManager.PlayerActions;
            if (inputActions == null)
                return;
            Vector2 moveInput = inputActions.Move.Value;

            bool canMove = true; //todo, some other status check
            if (canMove)
            {
                m_TargetMoveInputVector.x = moveInput.x;
                m_TargetMoveInputVector.z = moveInput.y;
            }
            else
            {
                m_TargetMoveInputVector = Vector3.zero;
            }
            m_MoveInputVector = Vector3.Lerp(m_MoveInputVector, m_TargetMoveInputVector,
                dt * m_MoveInputLerp);
        }
        
        private void ApplyMovementInput(float dt)
        {
            if (m_KinematicController == null || m_CameraYawTrans == null || m_CameraPitchTrans == null)
            {
                return;
            }

            Vector3 moveInputVector = Vector3.ClampMagnitude(m_MoveInputVector, 1f);

            Vector3 faceDirection = Vector3.ProjectOnPlane(m_CameraYawTrans.forward, -m_KinematicController.m_Gravity).normalized;
            Vector3 rightDirection = Vector3.ProjectOnPlane(m_CameraYawTrans.right, -m_KinematicController.m_Gravity).normalized;
            Vector3 targetDirection = faceDirection * m_MoveInputVector.z +
                                      rightDirection * m_MoveInputVector.x;

            if (targetDirection.sqrMagnitude > 1f)
            {
                targetDirection.Normalize();
            }

            Vector3 targetVelocity = targetDirection;

            m_WorldspaceMoveInputVector = Vector3.ProjectOnPlane(targetVelocity, m_KinematicController.Motor.CharacterUp).normalized * targetVelocity.magnitude;
            m_KinematicController.SetInputs(m_WorldspaceMoveInputVector, m_MoveInputVector);

            // Vector3 playerEuler = GetPlayerRotationEuler();
            // Vector3 lookDirection = Quaternion.Euler(playerEuler) * Vector3.forward;
            // m_KinematicController.SetLookDirection(lookDirection);
        }
        

        #endregion

        #region Action Input

        private void UpdateActionInput(float dt)
        {
            // if (ConsoleOpen() || IsInputSelected() || IsPauseMenuOpen())
            // {
            //     return;
            // }
            
            var playerActions = RuntimeInputManager.PlayerActions;

            if (playerActions.Jump.WasPressed)
            {
                JumpPressed();
            }

            if (m_AllowSprint)
            {
                if (playerActions.Sprint.IsPressed)
                {
                    m_SprintPressed = true;
                }
                else
                {
                    m_SprintPressed = false;
                }
            }
            else
            {
                m_SprintPressed = false;
            }

            if (playerActions.Crouch.WasPressed)
            {
                Debug.Log("crouch key down");
                if (m_KinematicController.IsSprinting())
                {
                    if (m_AllowSlide)
                    {
                        Slide();
                    }
                }
                else
                {
                    if (m_AllowCrouch)
                    {
                        CrouchPressed();
                    }
                }
            }
            if (playerActions.Crouch.IsPressed && m_AllowSlide)
            {
                if (m_JustLandTimer > 0f)
                {
                    if (m_KinematicController.IsOnGround())
                    {
                        Slide();
                    }
                }
            }

            if (playerActions.ToggleWalk.WasPressed)
            {
                m_KinematicController.ToggleWalk();
            }

            // if (playerActions.Use.WasPressed)
            // {
            //     UsePressed();
            // }
        }

        #endregion

        #region Actions

         private void JumpPressed()
        {
            if (m_KinematicController != null)
            {
                if (m_KinematicController.IsSliding())
                {
                    BreakSlide();
                    //decide whether could jump when sliding
                    m_KinematicController.RequestJump();
                }
                else
                {

                    if (m_KinematicController.IsOnGround())
                    {
                        if (m_KinematicController.IsCrouched())
                        {
                            SwitchPoseStand(false);
                        }
                        else if (m_KinematicController.IsStanding())
                        {
                            m_KinematicController.RequestJump();
                        }
                    }
                }
            }
        }

        private void CrouchPressed()
        {
            if (m_KinematicController.IsOnGround())
            {
                if (!m_KinematicController.IsSprinting())
                {
                    if (m_KinematicController.IsCrouched())
                    {
                        SwitchPoseStand(false);
                    }
                    else
                    {
                        SwitchPoseCrouch(false);
                    }
                }

            }
            else
            {
                if (!m_KinematicController.IsSprinting())
                {
                    if (m_KinematicController.IsCrouched())
                    {
                        SwitchPoseStand(true);
                    }
                    else
                    {
                        SwitchPoseCrouch(true);
                    }
                }
            }
        }

        private void SwitchPoseCrouch(bool isInAir)
        {
            if (isInAir)
            {
                m_KinematicController.SwitchPoseCrouch();
                m_KinematicController.Motor.SetCapsuleDimensions(m_KinematicController.Motor.Capsule.radius, m_CharacterCrouchHeight, m_CharacterCrouchHeight / 2);

                if (m_CameraPitchTrans != null)
                {
                    Vector3 offset = new Vector3(0f, m_CharacterStandHeight - m_CharacterCrouchHeight, 0f);
                    offset = Quaternion.FromToRotation(Vector3.down, m_KinematicController.m_Gravity) * offset;
                    var targetPos = m_KinematicController.Motor.transform.position + offset;
                    m_KinematicController.Motor.SetPosition(targetPos);
                    m_CharacterRoot.position = targetPos;
                    m_CameraController.ForceSetHeightAsCrouch();
                    m_CameraController.ApplyCameraHeight();
                }
            }
            else
            {
                m_KinematicController.SwitchPoseCrouch();
                m_KinematicController.Motor.SetCapsuleDimensions(m_KinematicController.Motor.Capsule.radius, m_CharacterCrouchHeight, m_CharacterCrouchHeight / 2);
                m_CameraController.ShiftCameraHeightPos(m_CameraController.m_CamCrouchHeight);
            }

            // if (OnCrouchObserver != null)
            // {
            //     OnCrouchObserver();
            // }
        }

        private bool SwitchPoseStand(bool isInAir)
        {
            if (m_KinematicController.IsStanding())
            {
                return false;
            }

            bool res = false;
            if (isInAir)
            {
                Collider[] probedColliders = new Collider[8];
                m_KinematicController.Motor.SetCapsuleDimensions(m_KinematicController.Motor.Capsule.radius, m_CharacterStandHeight, m_CharacterStandHeight / 2);
                if (m_KinematicController.Motor.CharacterOverlap(
                        m_KinematicController.Motor.TransientPosition,
                        m_KinematicController.Motor.TransientRotation,
                    probedColliders,
                    m_KinematicController.Motor.CollidableLayers,
                    QueryTriggerInteraction.Ignore) > 0)
                {
                    m_KinematicController.Motor.SetCapsuleDimensions(m_KinematicController.Motor.Capsule.radius, m_CharacterCrouchHeight, m_CharacterCrouchHeight / 2);
                    return false;
                }
                else
                {
                    m_KinematicController.SwitchPoseStand();
                    m_KinematicController.Motor.SetCapsuleDimensions(m_KinematicController.Motor.Capsule.radius, m_CharacterStandHeight, m_CharacterStandHeight / 2);
                    if (m_CameraPitchTrans != null && m_CharacterRoot != null)
                    {
                        Vector3 offset = new Vector3(0f, m_CharacterCrouchHeight - m_CharacterStandHeight, 0f);
                        offset = Quaternion.FromToRotation(Vector3.down, m_KinematicController.m_Gravity) * offset;
                        Vector3 targetPos = m_KinematicController.Motor.transform.position + offset;
                        m_KinematicController.Motor.SetPosition(targetPos);

                        m_CameraController.ForceSetHeightAsStand();
                        m_CameraController.ApplyCameraHeight();
                    }
                    res = true;
                }
            }
            else
            {
                Collider[] probedColliders = new Collider[8];
                m_KinematicController.Motor.SetCapsuleDimensions(m_KinematicController.Motor.Capsule.radius, m_CharacterStandHeight, m_CharacterStandHeight / 2);
                if (m_KinematicController.Motor.CharacterOverlap(
                        m_KinematicController.Motor.TransientPosition,
                        m_KinematicController.Motor.TransientRotation,
                    probedColliders,
                    m_KinematicController.Motor.CollidableLayers,
                    QueryTriggerInteraction.Ignore) > 0)
                {
                    m_KinematicController.Motor.SetCapsuleDimensions(m_KinematicController.Motor.Capsule.radius, m_CharacterCrouchHeight, m_CharacterCrouchHeight / 2);
                    return false;
                }
                else
                {
                    m_KinematicController.SwitchPoseStand();
                    m_KinematicController.Motor.SetCapsuleDimensions(m_KinematicController.Motor.Capsule.radius, m_CharacterStandHeight, m_CharacterStandHeight / 2);
                    m_CameraController.ShiftCameraHeightPos(m_CameraController.m_CamStandHeight);
                    res = true;
                }
            }

            if (res)
            {
                // if (OnStandObserver != null)
                // {
                //     OnStandObserver();
                // }
            }

            return res;
        }

        private void CheckSprintTakeEffect()
        {
            if (m_KinematicController.IsSliding())
            {
                return;
            }
            if (m_KinematicController.IsSprinting())
            {
                if (m_SprintPressed && m_KinematicController.IsOnGround())
                {
                    if (m_MoveInputVector.z <= 0.1f)
                    {
                        m_KinematicController.SwitchToRun();
                    }
                }
                else
                {
                    m_KinematicController.SwitchToRun();
                }
            }
            else
            {
                if (m_SprintPressed && m_KinematicController.IsOnGround() && m_MoveInputVector.z > 0f
                   //&& !m_WeaponController.m_ADSPressed
                   //&& m_KinematicController.IsNotClimbing()
                   )
                {
                    if (Mathf.Abs(m_MoveInputVector.z) > 0.1f)
                    {
                        if (m_KinematicController.IsCrouched())
                        {
                            bool succ = SwitchPoseStand(false);
                            if (succ)
                            {
                                m_KinematicController.SwitchToSprint();
                            }
                        }
                        else
                        {
                            m_KinematicController.SwitchToSprint();
                        }
                    }
                }
            }
        }

        private void SwtichPoseSlide()
        {
            m_KinematicController.SwitchPoseCrouch();

            m_KinematicController.Motor.SetCapsuleDimensions(m_KinematicController.Motor.Capsule.radius, m_CharacterCrouchHeight, m_CharacterCrouchHeight);
            if (m_CameraPitchTrans != null)
            {
                m_KinematicController.SwitchPoseCrouch();
                m_KinematicController.Motor.SetCapsuleDimensions(m_KinematicController.Motor.Capsule.radius, m_CharacterCrouchHeight, m_CharacterCrouchHeight / 2);
                m_CameraController.ShiftCameraHeightPos(m_CameraController.m_CamSlideHeight);
            }
            
            if (m_CameraController != null)
            {
                m_CameraController.EnabelSliding();
            }
            // if (OnSlideStart != null)
            // {
            //     OnSlideStart();
            // }
        }

        private void Slide()
        {
            Vector3 currentVelocity = m_KinematicController.Motor.Velocity;
            if (m_KinematicController.IsOnGround()
                && currentVelocity.sqrMagnitude > 1f
                && m_SlidingCDTimer <= 0f)
            {
                m_KinematicController.StartSlide(m_KinematicController.Motor.Velocity.normalized);
                SwtichPoseSlide();

                m_SlidingCDTimer = m_SlidingCD;

                // if (m_AudioController != null)
                // {
                //     m_AudioController.PlaySliding();
                // }
            }
        }

        private void BreakSlide()
        {
            m_KinematicController.BreakSlide();
            bool succ = SwitchPoseStand(false);
            if (succ)
            {
                if (m_CameraController != null)
                {
                    m_CameraController.DisableSliding();
                }
             
                if (OnSlideEnd != null)
                {
                    OnSlideEnd();
                }
            }
        }

        #endregion
      
        #region Update Character

        private void UpdatePlayerRoot(float dt)
        {
            m_CharacterRoot.transform.position = m_KinematicController.transform.position;
            m_CharacterRoot.transform.rotation = m_KinematicController.transform.rotation;
        }
        //
        // public float BuiltinCurveAngle()
        // {
        //     return m_PlayerBobCurve.BuiltinCurveAngle;
        // }

        #endregion

        #region Status

        public bool IsAlive()
        {
            return true;
        }

        #endregion

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        #region Events

        public delegate void VoidDelegate();
        public delegate void FloatDelegate(float value);
        public delegate void IntDelegate(int value);

        public VoidDelegate OnSlideEnd;
        #endregion

        #region Player Transform

        public Vector3 GetPlayerTransientPosition()
        {
            return m_KinematicController.Motor.TransientPosition;
        }

        public Vector3 GetPlayerPosition()
        {
            if (m_KinematicController != null && m_KinematicController.Motor)
            {
                return m_KinematicController.Motor.transform.position;
            }

            return transform.position;
        }

        public Vector3 GetPlayerCenterPoint()
        {
            Vector3 offset = m_KinematicController.Motor.Capsule.center;
            offset = Quaternion.FromToRotation(Vector3.down, m_KinematicController.m_Gravity) * offset;
            return offset + GetPlayerPosition();
        }

        public Vector3 GetPlayerRotationEuler()
        {
            return new Vector3(m_CameraPitchTrans.eulerAngles.x, m_CameraYawTrans.eulerAngles.y, 0f);
        }

        // public Vector3 GetPlannarFaceDirection()
        // {
        //     Vector3 dir = m_CameraYawTrans.forward;
        //     return Vector3.ProjectOnPlane(dir, -GetGravity());
        // }

        public void SetPlayerRotationEuler(Vector3 euler)
        {
            m_CameraPitchTrans.localEulerAngles = new Vector3(euler.x, 0f, 0f);
            m_CameraYawTrans.localEulerAngles = new Vector3(0f, euler.y, 0f);
        }

        public bool IsOnGround()
        {
            return m_KinematicController.IsOnGround();
        }

        public Vector3 GetGravity()
        {
            return m_KinematicController.m_Gravity;
        }

        public Vector3 GetCharacterUp()
        {
            return m_KinematicController.Motor.CharacterUp;
        }

        #endregion
        
        #region Kinematic Observer

        public void OnJumpStarted()
        {
            // if (m_WeaponController != null)
            // {
            //     m_WeaponController.OnJumpStarted();
            // }
            m_InAirTimer = 0f;
            m_InAirHighest = GetPlayerPosition();
            m_InAirGravity = m_KinematicController.m_Gravity;
            // if (OnJumpStart != null)
            // {
            //     OnJumpStart();
            // }
        }

        public void OnJumpLanded()
        {
            if (m_InAirTimer > 0.3f)
            {
                // if (m_AudioController != null)
                // {
                //     m_AudioController.PlayLand();
                // }
                // if (m_WeaponController != null)
                // {
                //     m_WeaponController.OnJumpEnd();
                // }
            }
            
            m_JustLandTimer = m_JustLandMax;

            Vector3 fallVec = m_InAirHighest - GetPlayerPosition();
            Vector3 fallProjected = Vector3.Project(fallVec, m_InAirGravity);
            float fallDistance = fallProjected.magnitude;
            bool takingDamage = false;

            // if (OnJumpEnd != null)
            // {
            //     OnJumpEnd();
            // }

            if (fallDistance >= m_FallDamageMaxDistance)
            {
                //Debug.Log("take fall damage " + 100);
                // if (!IsGod)
                // {
                //     ExecuteDamage(100);
                // }                
                takingDamage = true;
            }
            else if (fallDistance > m_FallDamageDistance)
            {
                float ratio = (fallDistance - m_FallDamageDistance) / (m_FallDamageMaxDistance - m_FallDamageDistance);
                int damage = (int)(ratio * 100);
                Debug.Log("take fall damage " + damage);
               //ExecuteDamage(damage);
                takingDamage = true;
            }
            if (takingDamage)
            {
                // if (IsAlive())
                // {
                //     if (m_AudioController != null)
                //     {
                //         m_AudioController.PlayHurt();
                //     }
                //     
                // }
                // else
                // {
                //     if (m_AudioController != null)
                //     {
                //         m_AudioController.PlayDeath(GetPlayerPosition());
                //     }
                //     Die();
                // }
            }
            else
            {
            }
        }

        public void OnFallInAir()
        {
            m_InAirTimer = 0f;
            m_InAirHighest = GetPlayerPosition();
            m_InAirGravity = m_KinematicController.m_Gravity;
            Debug.Log("on fall in air");
        }


        public void OnSlideFinished()
        {
            bool succ = SwitchPoseStand(false);
            if (succ)
            {
                //FullbodySwitchToStand();
            }
            else
            {
                SwitchPoseCrouch(false);
            }

            if (OnSlideEnd != null)
            {
                OnSlideEnd();
            }
        }

        #endregion

    }
}
