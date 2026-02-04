using UnityEngine;

namespace CR.OpenClaw
{
    /// <summary>
    /// OpenClaw 输入管理器 (修复版)
    /// 使用 OpenClawInputDeviceFixed 替代已删除的 OpenClawInputDevice
    /// </summary>
    public class OpenClawInputManager : MonoBehaviour
    {
        [Header("配置")]
        [Tooltip("启用 OpenClaw 输入设备")]
        public bool enableInput = true;
        
        [Tooltip("输入平滑度")]
        [Range(1f, 50f)]
        public float inputSmoothing = 10f;
        
        [Header("调试")]
        [Tooltip("显示输入调试信息")]
        public bool debugLog = false;
        
        [Tooltip("输入调试间隔（秒）")]
        public float debugInterval = 1f;
        
        private float m_DebugTimer = 0f;
        private OpenClawInputDevice m_Device;
        
        #region Unity 生命周期
        
        private void Awake()
        {
            if (enableInput)
            {
                InitializeOpenClawInput();
            }
        }
        
        private void Update()
        {
            if (debugLog)
            {
                m_DebugTimer += Time.deltaTime;
                if (m_DebugTimer >= debugInterval)
                {
                    //LogInputState();
                    m_DebugTimer = 0f;
                }
            }
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化 OpenClaw 输入设备
        /// </summary>
        private void InitializeOpenClawInput()
        {
            // 初始化输入设备
            OpenClawInputDevice.InitializeDevice();
            m_Device = OpenClawInputDevice.Instance;
            
            if (m_Device != null)
            {
                if (debugLog)
                {
                    Debug.Log("[OpenClawInputManager] OpenClaw 输入设备已初始化");
                }
            }
            else
            {
                Debug.LogError("[OpenClawInputManager] 无法初始化 OpenClaw 输入设备");
            }
        }
        
        #endregion
        
        #region 输入控制 API
        
        /// <summary>
        /// 移动玩家
        /// </summary>
        public void Move(float horizontal, float vertical)
        {
            if (!enableInput || m_Device == null) return;
            
            // 应用平滑
            horizontal = Mathf.Clamp(horizontal, -1f, 1f);
            vertical = Mathf.Clamp(vertical, -1f, 1f);
            
            m_Device.SetLeftStick(horizontal, vertical);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 移动: H={horizontal:F2}, V={vertical:F2}");
            }
        }
        
        /// <summary>
        /// 控制视角
        /// </summary>
        public void Look(float horizontal, float vertical)
        {
            if (!enableInput || m_Device == null) return;
            
            // 应用平滑
            horizontal = Mathf.Clamp(horizontal, -1f, 1f);
            vertical = Mathf.Clamp(vertical, -1f, 1f);
            
            m_Device.SetRightStick(horizontal, vertical);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 视角: H={horizontal:F2}, V={vertical:F2}");
            }
        }
        
        /// <summary>
        /// 跳跃 (Click 机制)
        /// </summary>
        public void Jump(float autoReleaseTime = 0.1f)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Jump(autoReleaseTime);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 跳跃: Click 机制，自动释放时间: {autoReleaseTime:F2}s");
            }
        }
        
        /// <summary>
        /// 跳跃按下 (Press 机制)
        /// </summary>
        public void JumpPress()
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.PressButton(InControl.InputControlType.Action3);
            
            if (debugLog)
            {
                Debug.Log("[OpenClawInput] 跳跃按下 (Press 机制)");
            }
        }
        
        /// <summary>
        /// 跳跃释放 (Release 机制)
        /// </summary>
        public void JumpRelease()
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.ReleaseButton(InControl.InputControlType.Action3);
            
            if (debugLog)
            {
                Debug.Log("[OpenClawInput] 跳跃释放 (Release 机制)");
            }
        }
        
        /// <summary>
        /// 射击 (Press/Release 机制)
        /// </summary>
        public void Fire(bool fire)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Fire(fire);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 射击: {fire} (Press/Release 机制)");
            }
        }
        
        /// <summary>
        /// 瞄准 (Press/Release 机制)
        /// </summary>
        public void Aim(bool aim)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Aim(aim);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 瞄准: {aim} (Press/Release 机制)");
            }
        }
        
        /// <summary>
        /// 蹲下 (Press/Release 机制)
        /// </summary>
        public void Crouch(bool crouch)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Crouch(crouch);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 蹲下: {crouch} (Press/Release 机制)");
            }
        }
        
        /// <summary>
        /// 疾跑 (Press/Release 机制)
        /// </summary>
        public void Sprint(bool sprint)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Sprint(sprint);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 疾跑: {sprint} (Press/Release 机制)");
            }
        }
        
        /// <summary>
        /// 换弹 (Click 机制)
        /// </summary>
        public void Reload(float autoReleaseTime = 0.1f)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Reload(autoReleaseTime);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 换弹: Click 机制，自动释放时间: {autoReleaseTime:F2}s");
            }
        }
        
        /// <summary>
        /// 切换武器 (Click 机制)
        /// </summary>
        public void SwitchWeapon(float autoReleaseTime = 0.1f)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.SwitchWeapon(autoReleaseTime);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 切换武器: Click 机制，自动释放时间: {autoReleaseTime:F2}s");
            }
        }
        
        /// <summary>
        /// 检查 (Click 机制)
        /// </summary>
        public void Inspect(float autoReleaseTime = 0.1f)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Inspect(autoReleaseTime);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 检查: Click 机制，自动释放时间: {autoReleaseTime:F2}s");
            }
        }
        
        /// <summary>
        /// 重置所有输入
        /// </summary>
        public void ResetInput()
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.ResetAll();
            
            if (debugLog)
            {
                Debug.Log("[OpenClawInput] 重置所有输入");
            }
        }
        
        #endregion
        
        #region 调试工具
        
        /// <summary>
        /// 记录输入状态
        /// </summary>
        private void LogInputState()
        {
            if (m_Device == null) return;
            
            Debug.Log("=== OpenClaw 输入状态 ===");
            Debug.Log($"设备状态: {(m_Device != null ? "已连接" : "未连接")}");
            Debug.Log("=== 输入机制 ===");
            Debug.Log("Click 机制: 按下后自动释放 (跳跃、换弹等)");
            Debug.Log("Press/Release 机制: 需要手动释放 (蹲下、瞄准等)");
        }
        
        /// <summary>
        /// 获取输入设备实例
        /// </summary>
        public OpenClawInputDevice GetInputDevice()
        {
            return m_Device;
        }
        
        /// <summary>
        /// 检查输入设备是否可用
        /// </summary>
        public bool IsInputDeviceAvailable()
        {
            return m_Device != null;
        }
        
        #endregion
        
        #region 静态访问
        
        /// <summary>
        /// 获取输入管理器实例
        /// </summary>
        public static OpenClawInputManager GetInstance()
        {
            return FindObjectOfType<OpenClawInputManager>();
        }
        
        /// <summary>
        /// 确保输入管理器存在
        /// </summary>
        public static OpenClawInputManager EnsureInstance()
        {
            var instance = GetInstance();
            if (instance == null)
            {
                GameObject obj = new GameObject("OpenClawInputManagerFixed");
                instance = obj.AddComponent<OpenClawInputManager>();
                instance.enableInput = true;
                instance.debugLog = true;
            }
            return instance;
        }
        
        #endregion
        
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/OpenClaw/初始化输入管理器")]
        public static void InitializeInputManager()
        {
            var manager = GetInstance();
            if (manager == null)
            {
                GameObject obj = new GameObject("OpenClawInputManager");
                manager = obj.AddComponent<OpenClawInputManager>();
                manager.enableInput = true;
                manager.debugLog = true;
            }
            
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                Debug.LogError("[OpenClawInputManager] 请在 Play Mode 中初始化");
                return;
            }
            
            manager.InitializeOpenClawInput();
            Debug.Log("[OpenClawInputManager] 输入管理器已初始化");
        }
        #endif
    }
}