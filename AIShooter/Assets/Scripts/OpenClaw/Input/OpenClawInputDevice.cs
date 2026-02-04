using System;
using System.Collections.Generic;
using UnityEngine;
using InControl;

namespace CR.OpenClaw
{
    /// <summary>
    /// OpenClaw 输入设备
    /// 参考 UIInputDevice.cs 的实现方式
    /// 使用 UpdateWithState 更新按钮状态
    /// 支持两种输入机制：click（自动释放）和 press/release（手动控制）
    /// </summary>
    public class OpenClawInputDevice : InputDevice
    {
        private static OpenClawInputDevice m_Instance;
        public static OpenClawInputDevice Instance => m_Instance;
        
        // 按钮状态字典
        private Dictionary<InputControlType, bool> m_ButtonStates = new Dictionary<InputControlType, bool>();
        
        // 自动释放计时器
        private Dictionary<InputControlType, float> m_AutoReleaseTimers = new Dictionary<InputControlType, float>();
        private const float DEFAULT_AUTO_RELEASE_TIME = 0.1f; // 默认自动释放时间
        
        // 摇杆值
        private Vector2 m_LeftStickValue = Vector2.zero;
        private Vector2 m_RightStickValue = Vector2.zero;
        
        public OpenClawInputDevice()
            : base("OpenClaw Virtual Input")
        {
            m_Instance = this;
            
            // 参考 UIInputDevice 添加控制
            // 左摇杆方向控制
            AddControl(InputControlType.LeftStickLeft, "Left Stick Left");
            AddControl(InputControlType.LeftStickRight, "Left Stick Right");
            AddControl(InputControlType.LeftStickUp, "Left Stick Up");
            AddControl(InputControlType.LeftStickDown, "Left Stick Down");
            
            // 右摇杆方向控制
            AddControl(InputControlType.RightStickLeft, "Right Stick Left");
            AddControl(InputControlType.RightStickRight, "Right Stick Right");
            AddControl(InputControlType.RightStickUp, "Right Stick Up");
            AddControl(InputControlType.RightStickDown, "Right Stick Down");
            
            // 按钮控制
            AddControl(InputControlType.Action1, "Fire");
            AddControl(InputControlType.Action2, "Aim");
            AddControl(InputControlType.Action3, "Jump");
            AddControl(InputControlType.Action4, "Inspect");
            AddControl(InputControlType.Action5, "Reload");
            AddControl(InputControlType.Action6, "Crouch");
            AddControl(InputControlType.Action7, "Sprint");
            AddControl(InputControlType.Action8, "SwitchWeapon");
            AddControl(InputControlType.Action9, "Slot1");
            AddControl(InputControlType.Action10, "Slot2");
            AddControl(InputControlType.Action11, "Slot3");
            
            // 初始化按钮状态
            InitializeButtonStates();
            
            Debug.Log("[OpenClawInputDevice] OpenClaw 虚拟输入设备已创建");
            Debug.Log("[OpenClawInputDevice] 使用 UpdateWithState 更新按钮状态");
        }
        
        private void InitializeButtonStates()
        {
            m_ButtonStates[InputControlType.Action1] = false;
            m_ButtonStates[InputControlType.Action2] = false;
            m_ButtonStates[InputControlType.Action3] = false;
            m_ButtonStates[InputControlType.Action4] = false;
            m_ButtonStates[InputControlType.Action5] = false;
            m_ButtonStates[InputControlType.Action6] = false;
            m_ButtonStates[InputControlType.Action7] = false;
            m_ButtonStates[InputControlType.Action8] = false;
            m_ButtonStates[InputControlType.Action9] = false;
            m_ButtonStates[InputControlType.Action10] = false;
            m_ButtonStates[InputControlType.Action11] = false;
        }
        
        public override void Update(ulong updateTick, float deltaTime)
        {
            // 处理自动释放计时器
            ProcessAutoReleaseTimers(deltaTime);
            
            // 更新所有按钮状态
            UpdateButtonStates(updateTick, deltaTime);
            
            // 更新摇杆状态
            UpdateLeftStickWithValue(m_LeftStickValue, updateTick, deltaTime);
            UpdateRightStickWithValue(m_RightStickValue, updateTick, deltaTime);
        }
        
        private void ProcessAutoReleaseTimers(float deltaTime)
        {
            // 使用 for 循环遍历键列表，避免在 foreach 中修改集合
            var buttons = new List<InputControlType>(m_AutoReleaseTimers.Keys);
            
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                if (!m_AutoReleaseTimers.ContainsKey(button)) continue;
                
                var timer = m_AutoReleaseTimers[button] - deltaTime;
                
                if (timer <= 0f)
                {
                    // 时间到，自动释放按钮
                    m_AutoReleaseTimers.Remove(button);
                    m_ButtonStates[button] = false;
                    Debug.Log($"[OpenClawInput] 自动释放按钮: {button}");
                }
                else
                {
                    // 更新计时器
                    m_AutoReleaseTimers[button] = timer;
                }
            }
        }
        
        private void UpdateButtonStates(ulong updateTick, float deltaTime)
        {
            // 更新每个按钮的状态
            foreach (var kvp in m_ButtonStates)
            {
                UpdateWithState(kvp.Key, kvp.Value, updateTick, deltaTime);
            }
        }
        
        private void UpdateLeftStickWithValue(Vector2 value, ulong updateTick, float deltaTime)
        {
            // 参考 UIInputDevice 的实现方式
            // 将 Vector2 值分解为四个方向控制
            UpdateWithValue(InputControlType.LeftStickLeft, Mathf.Max(0.0f, -value.x), updateTick, deltaTime);
            UpdateWithValue(InputControlType.LeftStickRight, Mathf.Max(0.0f, value.x), updateTick, deltaTime);
            UpdateWithValue(InputControlType.LeftStickUp, Mathf.Max(0.0f, value.y), updateTick, deltaTime);
            UpdateWithValue(InputControlType.LeftStickDown, Mathf.Max(0.0f, -value.y), updateTick, deltaTime);
        }
        
        private void UpdateRightStickWithValue(Vector2 value, ulong updateTick, float deltaTime)
        {
            // 参考 UIInputDevice 的实现方式
            UpdateWithValue(InputControlType.RightStickLeft, Mathf.Max(0.0f, -value.x), updateTick, deltaTime);
            UpdateWithValue(InputControlType.RightStickRight, Mathf.Max(0.0f, value.x), updateTick, deltaTime);
            UpdateWithValue(InputControlType.RightStickUp, Mathf.Max(0.0f, value.y), updateTick, deltaTime);
            UpdateWithValue(InputControlType.RightStickDown, Mathf.Max(0.0f, -value.y), updateTick, deltaTime);
        }
        
        #region 公共 API - 两种输入机制
        
        /// <summary>
        /// Click 机制：按下按钮，在指定时间后自动释放
        /// 适用于跳跃、换弹等短暂动作
        /// </summary>
        public void ClickButton(InputControlType button, float autoReleaseTime = DEFAULT_AUTO_RELEASE_TIME)
        {
            if (m_ButtonStates.ContainsKey(button))
            {
                // 按下按钮
                m_ButtonStates[button] = true;
                
                // 设置自动释放计时器
                m_AutoReleaseTimers[button] = autoReleaseTime;
                
                Debug.Log($"[OpenClawInput] Click 按钮: {button} (自动释放时间: {autoReleaseTime:F2}s)");
            }
        }
        
        /// <summary>
        /// Press 机制：按下按钮，需要手动释放
        /// 适用于瞄准、射击等需要持续按下的动作
        /// </summary>
        public void PressButton(InputControlType button)
        {
            if (m_ButtonStates.ContainsKey(button))
            {
                m_ButtonStates[button] = true;
                
                // 如果之前有自动释放计时器，移除它
                if (m_AutoReleaseTimers.ContainsKey(button))
                {
                    m_AutoReleaseTimers.Remove(button);
                }
                
                Debug.Log($"[OpenClawInput] Press 按钮: {button}");
            }
        }
        
        /// <summary>
        /// Release 机制：释放按钮
        /// 与 Press 配对使用
        /// </summary>
        public void ReleaseButton(InputControlType button)
        {
            if (m_ButtonStates.ContainsKey(button))
            {
                m_ButtonStates[button] = false;
                
                // 移除自动释放计时器（如果有）
                if (m_AutoReleaseTimers.ContainsKey(button))
                {
                    m_AutoReleaseTimers.Remove(button);
                }
                
                Debug.Log($"[OpenClawInput] Release 按钮: {button}");
            }
        }
        
        /// <summary>
        /// 设置左摇杆值
        /// </summary>
        public void SetLeftStick(float horizontal, float vertical)
        {
            m_LeftStickValue = new Vector2(horizontal, vertical);
            m_LeftStickValue = Vector2.ClampMagnitude(m_LeftStickValue, 1.0f);
            
            Debug.Log($"[OpenClawInput] 左摇杆: ({m_LeftStickValue.x:F2}, {m_LeftStickValue.y:F2})");
        }
        
        /// <summary>
        /// 设置右摇杆值
        /// </summary>
        public void SetRightStick(float horizontal, float vertical)
        {
            m_RightStickValue = new Vector2(horizontal, vertical);
            m_RightStickValue = Vector2.ClampMagnitude(m_RightStickValue, 1.0f);
            
            Debug.Log($"[OpenClawInput] 右摇杆: ({m_RightStickValue.x:F2}, {m_RightStickValue.y:F2})");
        }
        
        /// <summary>
        /// 重置所有输入
        /// </summary>
        public void ResetAll()
        {
            // 重置摇杆
            m_LeftStickValue = Vector2.zero;
            m_RightStickValue = Vector2.zero;
            
            // 重置所有按钮
            foreach (var key in m_ButtonStates.Keys)
            {
                m_ButtonStates[key] = false;
            }
            
            // 清除所有自动释放计时器
            m_AutoReleaseTimers.Clear();
            
            Debug.Log("[OpenClawInput] 重置所有输入");
        }
        
        #endregion
        
        #region 便捷方法
        
        /// <summary>
        /// 跳跃 - 使用 Click 机制（短暂按下后自动释放）
        /// </summary>
        public void Jump(float autoReleaseTime = DEFAULT_AUTO_RELEASE_TIME)
        {
            ClickButton(InputControlType.Action3, autoReleaseTime);
        }
        
        /// <summary>
        /// 射击 - 使用 Press/Release 机制
        /// </summary>
        public void Fire(bool fire)
        {
            if (fire)
                PressButton(InputControlType.Action1);
            else
                ReleaseButton(InputControlType.Action1);
        }
        
        /// <summary>
        /// 瞄准 - 使用 Press/Release 机制
        /// </summary>
        public void Aim(bool aim)
        {
            if (aim)
                PressButton(InputControlType.Action2);
            else
                ReleaseButton(InputControlType.Action2);
        }
        
        /// <summary>
        /// 蹲下 - 使用 Press/Release 机制
        /// </summary>
        public void Crouch(bool crouch)
        {
            if (crouch)
                ClickButton(InputControlType.Action6);
            else
                ClickButton(InputControlType.Action6);
        }
        
        /// <summary>
        /// 疾跑 - 使用 Press/Release 机制
        /// </summary>
        public void Sprint(bool sprint)
        {
            if (sprint)
                PressButton(InputControlType.Action7);
            else
                ReleaseButton(InputControlType.Action7);
        }
        
        /// <summary>
        /// 换弹 - 使用 Click 机制
        /// </summary>
        public void Reload(float autoReleaseTime = DEFAULT_AUTO_RELEASE_TIME)
        {
            ClickButton(InputControlType.Action5, autoReleaseTime);
        }
        
        /// <summary>
        /// 切换武器 - 使用 Click 机制
        /// </summary>
        public void SwitchWeapon(float autoReleaseTime = DEFAULT_AUTO_RELEASE_TIME)
        {
            ClickButton(InputControlType.Action8, autoReleaseTime);
        }
        
        /// <summary>
        /// 检查 - 使用 Click 机制
        /// </summary>
        public void Inspect(float autoReleaseTime = DEFAULT_AUTO_RELEASE_TIME)
        {
            ClickButton(InputControlType.Action4, autoReleaseTime);
        }
        
        #endregion
        
        #region 静态访问
        
        public static bool IsAvailable => m_Instance != null;
        
        public static void InitializeDevice()
        {
            if (m_Instance == null)
            {
                var device = new OpenClawInputDevice();
                InputManager.AttachDevice(device);
                Debug.Log("[OpenClawInputDevice] 设备已注册到 InputManager");
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 修复版的输入管理器
    /// </summary>
    public class OpenClawInputManagerFixed : MonoBehaviour
    {
        [Header("配置")]
        public bool enableInput = true;
        public bool debugLog = true;
        
        private OpenClawInputDevice m_Device;
        
        private void Start()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            if (!enableInput) return;
            
            // 初始化设备
            OpenClawInputDevice.InitializeDevice();
            m_Device = OpenClawInputDevice.Instance;
            
            if (m_Device != null)
            {
                if (debugLog)
                {
                    Debug.Log("[OpenClawInputManagerFixed] 输入管理器已初始化");
                    Debug.Log("[OpenClawInputManagerFixed] 使用修复版的输入设备");
                }
            }
            else
            {
                Debug.LogError("[OpenClawInputManagerFixed] 无法初始化输入设备");
            }
        }
        
        #region 公共 API
        
        public void Move(float horizontal, float vertical)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.SetLeftStick(horizontal, vertical);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 移动: H={horizontal:F2}, V={vertical:F2}");
            }
        }
        
        public void Look(float horizontal, float vertical)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.SetRightStick(horizontal, vertical);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 视角: H={horizontal:F2}, V={vertical:F2}");
            }
        }

        private const float DEFAULT_AUTO_RELEASE_TIME = 0.1f;
        public void Jump(float autoReleaseTime = DEFAULT_AUTO_RELEASE_TIME)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Jump(autoReleaseTime);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 跳跃 (Action3) - Click 机制，自动释放时间: {autoReleaseTime:F2}s");
            }
        }
        
        public void JumpPress()
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.PressButton(InputControlType.Action3);
            
            if (debugLog)
            {
                Debug.Log("[OpenClawInput] 跳跃按下 (Action3) - Press 机制");
            }
        }
        
        public void JumpRelease()
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.ReleaseButton(InputControlType.Action3);
            
            if (debugLog)
            {
                Debug.Log("[OpenClawInput] 跳跃释放 (Action3) - Release 机制");
            }
        }
        
        public void Fire(bool fire)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Fire(fire);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 射击: {fire} (Action1)");
            }
        }
        
        public void Aim(bool aim)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Aim(aim);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 瞄准: {aim} (Action2)");
            }
        }
        
        public void Crouch(bool crouch)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Crouch(crouch);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 蹲下: {crouch} (Action6)");
            }
        }
        
        public void Sprint(bool sprint)
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Sprint(sprint);
            
            if (debugLog)
            {
                Debug.Log($"[OpenClawInput] 疾跑: {sprint} (Action7)");
            }
        }
        
        public void Reload()
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Reload();
            
            if (debugLog)
            {
                Debug.Log("[OpenClawInput] 换弹 (Action5)");
            }
        }
        
        public void SwitchWeapon()
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.SwitchWeapon();
            
            if (debugLog)
            {
                Debug.Log("[OpenClawInput] 切换武器 (Action8)");
            }
        }
        
        public void Inspect()
        {
            if (!enableInput || m_Device == null) return;
            
            m_Device.Inspect();
            
            if (debugLog)
            {
                Debug.Log("[OpenClawInput] 检查 (Action4)");
            }
        }
        
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
        
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/OpenClaw/初始化修复版输入设备")]
        public static void InitializeFixedInput()
        {
            var manager = FindObjectOfType<OpenClawInputManagerFixed>();
            if (manager == null)
            {
                GameObject obj = new GameObject("OpenClawInputManagerFixed");
                manager = obj.AddComponent<OpenClawInputManagerFixed>();
                manager.enableInput = true;
                manager.debugLog = true;
            }
            
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                Debug.LogError("[OpenClawInputManagerFixed] 请在 Play Mode 中初始化");
                return;
            }
            
            manager.Initialize();
            Debug.Log("[OpenClawInputManagerFixed] 修复版输入设备已初始化");
        }
        #endif
    }
}