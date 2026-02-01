using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

namespace CR
{
    public interface KinematicObserver
    {
        void OnJumpStarted();
        void OnJumpLanded();
        void OnFallInAir();
        void OnSlideFinished();
    }

    public class FPSKinematicController : MonoBehaviour, ICharacterController
	{
		public KinematicCharacterMotor Motor;

        public enum MoveState
        {
            None,
            Run,
            Sprint,
            Slide,
            //Climb
        }
        
        public enum PoseState
        {
            Stand,
            Crouched
        }

        public enum ADSMode
        {
            Off,
            On
        }

        public KinematicObserver Observer { set; get; }

        public bool m_ToggleWalk;
        public float m_RunSpeed = 3f;
        public float m_WalkSpeedMultiplier = 0.5f;
        public float m_GroundMoveSharpness = 10f;
        public float m_JumpSpeed = 10f;
        private bool m_JumpPending = false;

        public Vector3 m_Gravity = new Vector3(0, -30f, 0);
        
        public float m_MaxAirMoveSpeed = 3f;
        public float m_AirAccelerationSpeed = 1f;
        public float m_AirMoveDrag = 0.1f;

        private Vector3 m_LocalMoveInputV3 = Vector3.zero;
        private Vector3 m_WorldMoveInputV3 = Vector3.zero;
        private Vector3 m_LookDirection = Vector3.forward;

        public float m_ClimbSpeed = 2f;
        public float m_SprintMultiplier = 1.8f;
        public float m_CrouchMultiplier = 0.3f;
        public float m_ADSMultiplier = 0.5f;
        public float m_SlideMaxTime = 0.7f;
        public float m_SlideMultiplier = 2f;
        private MoveState m_MoveState = MoveState.Run;
        private PoseState m_PoseState = PoseState.Stand;
        private ADSMode m_ADSMode = ADSMode.Off;
        private bool m_SprintHeld = false;
        private bool m_WasOnGroundLastFrame = true;

        private Vector3 m_SlideDirection;
        private float m_SlideTimer = 0f;
        private float SLIDE_MAX_TIME = 0.8f;
        private float SLIDE_MULTIPLIER = 2f;

        public LayerMask CollidableLayers = -1;
        private List<Collider> m_IgnoredColliders = new List<Collider>();
        
        private void Awake()
        {
			Motor.CharacterController = this;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (m_MoveState == MoveState.Slide)
            {
                m_SlideTimer += dt;
                if (m_SlideTimer >= SLIDE_MAX_TIME)
                {
                    m_MoveState = MoveState.Run;
                    FinishedSlide();
                }
            }
        }

        #region Inputs
        public void SetInputs(Vector3 worldspaceInput, Vector3 localInput)
        {
            m_LocalMoveInputV3 = localInput;
            m_WorldMoveInputV3 = worldspaceInput;

        }

        public void SetLookDirection(Vector3 dir)
        {
            m_LookDirection = dir;
        }

        public void RequestJump()
        {
            m_JumpPending = true;
        }

        #endregion

        #region Ignore Collider
        public void AddIgnoreCollider(Collider col)
        {
            m_IgnoredColliders.Add(col);
        }

        public void ClearIgnoreColliders()
        {
            m_IgnoredColliders.Clear();
        }
        #endregion

        #region Interface
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            Vector3 currentUp = (currentRotation * Vector3.up);
            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -m_Gravity.normalized, 1 - Mathf.Exp(-10 * deltaTime));
            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
        }

        public void UpdateVelocityNormal(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 targetMovementVelocity = Vector3.zero;
            float adsModifier = 1f;
            if (m_ADSMode == ADSMode.On)
            {
                adsModifier *= m_ADSMultiplier;
            }
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                if (m_MoveState == MoveState.Run)
                {
                    currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                    Vector3 inputRight = Vector3.Cross(m_WorldMoveInputV3, Motor.CharacterUp);
                    Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * m_WorldMoveInputV3.magnitude;

                    if (m_PoseState == PoseState.Crouched)
                    {
                        targetMovementVelocity = reorientedInput * m_RunSpeed * m_CrouchMultiplier * adsModifier;
                    }
                    else if (m_PoseState == PoseState.Stand)
                    {
                        targetMovementVelocity = reorientedInput * m_RunSpeed * adsModifier;
                        if (m_ToggleWalk)
                        {
                            targetMovementVelocity *= m_WalkSpeedMultiplier;
                        }
                    }
                    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                        1 - Mathf.Exp(-m_GroundMoveSharpness * deltaTime));
                }
                else if (m_MoveState == MoveState.Slide)
                {
                    currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                    Vector3 inputRight = Vector3.Cross(m_SlideDirection, Motor.CharacterUp);
                    Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * m_SlideDirection.magnitude;
                    targetMovementVelocity = reorientedInput * m_RunSpeed * SLIDE_MULTIPLIER;
                    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-m_GroundMoveSharpness * deltaTime));

                }
                else if (m_MoveState == MoveState.Sprint)
                {
                    currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                    Vector3 inputRight = Vector3.Cross(m_WorldMoveInputV3, Motor.CharacterUp);
                    Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * m_WorldMoveInputV3.magnitude;
                    targetMovementVelocity = reorientedInput * m_RunSpeed * m_SprintMultiplier;
                    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-m_GroundMoveSharpness * deltaTime));

                }

            }
            else
            {
                if (m_WorldMoveInputV3.sqrMagnitude > 0f)
                {
                    targetMovementVelocity = m_WorldMoveInputV3 * m_MaxAirMoveSpeed;
                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, m_Gravity);
                    currentVelocity += velocityDiff * m_AirAccelerationSpeed * deltaTime;
                }

                currentVelocity += m_Gravity * deltaTime;
                currentVelocity *= (1f / (1f + (m_AirMoveDrag * deltaTime)));
            }

            if (m_WasOnGroundLastFrame)
            {
                if (!Motor.GroundingStatus.IsStableOnGround)
                {
                    if (Observer != null)
                    {
                        Observer.OnFallInAir();
                    }
                }
            }
            else
            {
                if (Motor.GroundingStatus.IsStableOnGround)
                {
                    if (Observer != null)
                    {
                        Observer.OnJumpLanded();
                    }
                }
            }

            m_WasOnGroundLastFrame = Motor.GroundingStatus.IsStableOnGround;
            if (m_JumpPending)
            {
                if (Motor.GroundingStatus.IsStableOnGround)
                {
                    Vector3 jumpDirection = -m_Gravity.normalized;
                    Motor.ForceUnground();
                    m_WasOnGroundLastFrame = false;
                    currentVelocity += (jumpDirection * m_JumpSpeed) - Vector3.Project(currentVelocity, jumpDirection);
                    m_JumpPending = false;
                    if (Observer != null)
                    {
                        Observer.OnJumpStarted();
                    }
                }
            }

        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            UpdateVelocityNormal(ref currentVelocity, deltaTime);

            Vector3 plannarVelcoity = Vector3.ProjectOnPlane(currentVelocity, -m_Gravity.normalized);
            CurrentSpeed = plannarVelcoity.magnitude;
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
         
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            // if (coll.gameObject.layer == LayerTools.GetRagdollLayer())
            // {
            //     return false;
            // }


            if (m_IgnoredColliders.Count == 0)
            {
                return true;
            }

            if (m_IgnoredColliders.Contains(coll))
            {
                return false;
            }

            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {         
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {         
        }
        #endregion

        #region Get/Set Values
        public float CurrentSpeed
        {
            set; get;
        }

        public float CurrentRunSpeedNormalized
        {
            get
            {
                return CurrentSpeed / m_RunSpeed;
            }
        }

        public float CurrentCrouchSpeedNormalized
        {
            get
            {
                return CurrentSpeed / (m_RunSpeed * m_CrouchMultiplier);
            }
        }

        public bool IsOnGround()
        {
            return Motor.GroundingStatus.IsStableOnGround;
        }

        public bool IsCrouched()
        {
            return m_PoseState == PoseState.Crouched;
        }

        public bool IsStanding()
        {
            return m_PoseState == PoseState.Stand;
        }

        public float GetCharacterSpeed()
        {
            return Motor.BaseVelocity.magnitude;
        }

        public bool IsSprinting()
        {
            return m_MoveState == MoveState.Sprint;
        }

        public bool IsSliding()
        {
            return m_MoveState == MoveState.Slide;
        }

        public bool IsWalkEnabled()
        {
            return m_ToggleWalk;
        }
        #endregion


        #region Pose
        public void SwitchPoseStand()
        {
            m_PoseState = PoseState.Stand;
        }

        public void SwitchPoseCrouch()
        {
            m_PoseState = PoseState.Crouched;
        }

        public void SwitchToSprint()
        {
            if (m_MoveState == MoveState.Slide) return;
            m_MoveState = MoveState.Sprint;
        }

        public void ToggleWalk()
        {
            m_ToggleWalk = !m_ToggleWalk;
        }

        public void SwitchToRun()
        {
            if (m_MoveState == MoveState.Slide) return;
            m_MoveState = MoveState.Run;
        }

        public void SwitchADSModeOn()
        {
            m_ADSMode = ADSMode.On;
        }

        public void SwitchADSModeOff()
        {
            m_ADSMode = ADSMode.Off;
        }

        public void StartSlide(Vector3 dir)
        {
            if (m_MoveState == MoveState.Slide) return;
            m_MoveState = MoveState.Slide;
            m_SlideTimer = 0f;
            m_SlideDirection = dir.normalized;
        }

        public void BreakSlide()
        {
            if (m_MoveState == MoveState.Slide)
            {
                m_MoveState = MoveState.Run;
            }
        }

        public void FinishedSlide()
        {
            if (Observer != null)
            {
                Observer.OnSlideFinished();
            }
        }
        #endregion

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Vector3 center = Motor.transform.position + Vector3.up * 0.2f;
            Gizmos.DrawLine(center, center + m_WorldMoveInputV3 * 3f);
        }
    }
}
