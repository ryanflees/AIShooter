using System;
using InControl;
using UnityEngine;

namespace CR
{
    public class FPSCameraController : MonoBehaviour
    {
        [Header("Camera References")]
        public Camera m_Camera;
        public Transform m_CameraYawTrans;
        public Transform m_CameraPitchTrans;
       
        [Header("Default Sensitivity")]
        public float m_SensitivityHorizontal = 2f;
        public float m_SensitivityVertical = 2f;

        [Range(-90f, 90f)]
        public float m_MinPitchAngle = -80f;
        [Range(-90f, 90f)]
        public float m_MaxPitchAngle = 80f;

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
        
        
        
        [Range(1f, 30f)]
        public float RotationSharpness = 15f;


        private void Awake()
        {
          
        }

        #region Mouse Smoothing
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
	        m_RotArrayHor[m_RotCacheCounter] = rotateHor;
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
            UpdateLookInputs(dt);
        }

        private void UpdateLookInputs(float dt)
        {
			var playerActions = RuntimeInputManager.PlayerActions;
			float rotateHor = playerActions.Look.X;
			float rotateVer = -playerActions.Look.Y;

			if (playerActions.ActiveDevice != null
			    && playerActions.ActiveDevice.DeviceClass == InputDeviceClass.Controller)
			{
				//can't find where to set controller sensitivity of InControl
				//so manually apply more sensitivity for controller here
				rotateHor *= 1f;
				rotateVer *= 1f;
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
				float hor = m_SensitivityHorizontal * rotateHor;
				float ver = m_SensitivityVertical * rotateVer;

				if (m_CameraPitchTrans != null)
				{
					var currentX = m_CameraPitchTrans.localEulerAngles.x;
					if (currentX > 180f)
					{
						currentX -= 360f;
					}
					if (ver > 0f)
					{

						if (currentX < m_MaxPitchAngle)
						{
							currentX = Mathf.Min(currentX + ver, m_MaxPitchAngle);
						}
					}
					else if (ver < 0f)
					{
						if (currentX > m_MinPitchAngle)
						{
							currentX = Mathf.Max(currentX + ver, m_MinPitchAngle);
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
						//m_LastRotateDirectionStatus = hor > 0 ? RotateDirectionStatus.Right : RotateDirectionStatus.Left;
					}
				}
			}
		}
        
        #endregion
        
        /// <summary>
        /// Set camera position to follow target
        /// </summary>
        // public void SetPosition(Vector3 position)
        // {
        //     Transform.position = position;
        // }

        /// <summary>
        /// Normalize angle to -180 to 180 range
        /// </summary>
        private float NormalizeAngle(float angle)
        {
            while (angle > 180f)
                angle -= 360f;
            while (angle < -180f)
                angle += 360f;
            return angle;
        }

        /// <summary>
        /// Get camera forward direction projected on horizontal plane
        /// </summary>
        public Vector3 GetPlanarForward()
        {
            Vector3 forward = m_CameraYawTrans.forward;
            forward.y = 0f;
            return forward.normalized;
        }

        /// <summary>
        /// Get camera right direction projected on horizontal plane
        /// </summary>
        public Vector3 GetPlanarRight()
        {
            Vector3 right = m_CameraYawTrans.right;
            right.y = 0f;
            return right.normalized;
        }
    }
}
