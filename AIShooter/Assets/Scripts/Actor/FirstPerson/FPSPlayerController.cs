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
    public class FPSPlayerController : MonoBehaviour
    {
        public PlayerStatus m_PlayerStatus = new PlayerStatus();
        
        [Header("References")]
        public FPSKinematicController m_KinematicController;
        public FPSCameraController m_CameraController;

        public Transform m_CharacterRoot; //A root transform for Character models and Cameras
        public Transform m_CameraYawTrans;
        public Transform m_CameraPitchTrans;

        [Header("Input Settings")]
        public bool m_UseMouse = true;
        
        //private FPSCharacterInputs _characterInputs;
        
        private Vector3 m_TargetMoveInputVector = Vector3.zero;
        private Vector3 m_MoveInputVector = Vector3.zero;
        public float m_MoveInputLerp = 100f;
        private Vector3 m_WorldspaceMoveInputVector = Vector3.zero;

        private void Start()
        {
            // Lock and hide cursor for FPS
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }

        private void Update()
        {
            float dt = Time.deltaTime;
            
            //todo check is ~ console is open, if not then input is allowed
            HandleInput(dt);
        }

        private void LateUpdate()
        {
            // Update camera after character movement
            float dt = Time.deltaTime;

            if (m_CameraController != null && m_KinematicController != null)
            {
                // m_CameraController.UpdateWithInput(
                //     deltaTime,
                //     _characterInputs.LookInput,
                //     m_MovementController.IsMoving,
                //     m_MovementController.CurrentMoveSpeed,
                //     m_MovementController.IsSprinting
                // );
            }
            // Soup Added for Render Streaming
            //float mouseX = playerInputAction.ReadValue<Vector2>().x;
            //float mouseY = playerInputAction.ReadValue<Vector2>().y;

            UpdatePlayerRoot(dt);
            // m_PlayerBobCurve.UpdateCurve(dt);
            // UpdateStatus(Time.smoothDeltaTime);

            if (IsAlive())
            {
                //m_WeaponController.ManualLateUpdate(dt, mouseX, mouseY);
                //m_CameraController.ManualLateUpdate(dt);
            }
            //
            // if (Observer != null)
            // {
            //     Observer.ManualLateUpdate(dt);
            // }
        }

        private void HandleInput(float dt)
        {
            // Get input from RuntimeInputManager
            PlayerInputActions inputActions = RuntimeInputManager.PlayerActions;
            if (inputActions == null)
                return;
            
            
            m_CameraController.ManualUpdate(dt);

            // Movement input (WASD / Left Stick)
            Vector2 moveInput = inputActions.Move.Value;
            //_characterInputs.MoveInput = moveInput;

            bool canMove = true; //todo, some other status check
            if (canMove)
            {
                m_TargetMoveInputVector.x = inputActions.Move.X;
                m_TargetMoveInputVector.z = inputActions.Move.Y;
            }
            else
            {
                m_TargetMoveInputVector = Vector3.zero;
            }
            m_MoveInputVector = Vector3.Lerp(m_MoveInputVector, m_TargetMoveInputVector,
                dt * m_MoveInputLerp);
            
            // Look input (Mouse / Right Stick)
            Vector2 lookInput = Vector2.zero;

            if (m_UseMouse)
            {
                // Mouse input (already has delta built in)
                lookInput = inputActions.Look.Value;
            }
            // else if (UseGamepad)
            // {
            //     // Gamepad input (continuous, needs time scaling)
            //     lookInput = inputActions.Look.Value * Time.deltaTime * 100f;
            // }

            //_characterInputs.LookInput = lookInput;

            // Jump input (Space / A button)
            //_characterInputs.JumpDown = inputActions.Jump.WasPressed;

            // Crouch input (C / B button)
            if (inputActions.Crouch.WasPressed)
            {
                // _characterInputs.CrouchDown = true;
                // _characterInputs.CrouchUp = false;
            }
            else if (inputActions.Crouch.WasReleased)
            {
                // _characterInputs.CrouchDown = false;
                // _characterInputs.CrouchUp = true;
            }
            else
            {
                // _characterInputs.CrouchDown = false;
                // _characterInputs.CrouchUp = false;
            }

            // Sprint input (Left Shift / Left Stick Click)
            // _characterInputs.SprintHeld = inputActions.Sprint.IsPressed;
            //
            // // Send inputs to character
            // if (m_MovementController != null)
            // {
            //     m_MovementController.SetInputs(ref _characterInputs);
            // }
            ApplyMovementInput(dt);

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
    }
}
