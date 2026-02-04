using System;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using SickDev.CommandSystem;

namespace CR.OpenClaw
{
    /// <summary>
    /// 简化版 OpenClaw 控制台命令
    /// 只保留必要的命令：ocl_move, ocl_jump, ocl_crouch
    /// 支持两种输入机制：click（自动释放）和 press/release（手动控制）
    /// </summary>
    public static class OpenClawConsoleCommandsSimplified
    {
        #region 输入设备状态
        
        private static OpenClawInputManager GetInputManager()
        {
            var manager = OpenClawInputManager.GetInstance();
            if (manager == null)
            {
                Debug.LogError("[OpenClawConsole] 未找到 OpenClawInputManager");
                return null;
            }
            return manager;
        }
        
        private static void Log(string message)
        {
            Debug.Log($"[OpenClawConsole] {message}");
        }
        
        #endregion
        
        #region 基础输入命令 (简化版)
        
        [Command(alias = "ocl_move")]
        public static void Move(float horizontal, float vertical)
        {
            var manager = GetInputManager();
            if (manager == null) return;
            
            manager.Move(horizontal, vertical);
            Log($"移动: H={horizontal:F2}, V={vertical:F2}");
        }
        
        [Command(alias = "ocl_jump")]
        public static void Jump(float duration = 0.1f)
        {
            var manager = GetInputManager();
            if (manager == null) return;
            
            manager.Jump(duration);
            Log($"跳跃: Click 机制，自动释放时间: {duration:F2}s");
        }
        
        [Command(alias = "ocl_crouch")]
        public static void Crouch(bool crouch = true)
        {
            var manager = GetInputManager();
            if (manager == null) return;
            
            manager.Crouch(crouch);
            Log($"蹲下: {crouch} (Press/Release 机制)");
        }
        
        [Command(alias = "ocl_reset")]
        public static void ResetInput()
        {
            var manager = GetInputManager();
            if (manager == null) return;
            
            manager.ResetInput();
            Log("重置所有输入");
        }
        
        #endregion
        
        #region 状态和帮助命令
        
        [Command(alias = "ocl_status")]
        public static void Status()
        {
            var manager = GetInputManager();
            if (manager == null) return;
            
            Log("=== OpenClaw 输入设备状态 ===");
            Log($"输入管理器: {(manager != null ? "已找到" : "未找到")}");
            
            // 检查输入设备
            var device = OpenClawInputDevice.Instance;
            if (device != null)
            {
                Log($"输入设备: {device.Name}");
                Log($"设备状态: 已注册到 InputManager");
                Log($"输入机制: 支持 Click 和 Press/Release 两种机制");
            }
            else
            {
                Log("输入设备: 未初始化");
            }
            
            Log("=== 可用命令 ===");
            Log("ocl_move <h> <v>        - 移动 (摇杆)");
            Log("ocl_jump [duration]     - 跳跃 (Click 机制，自动释放)");
            Log("ocl_crouch [true/false] - 蹲下 (Press/Release 机制)");
            Log("ocl_reset               - 重置所有输入");
            Log("ocl_status              - 显示状态");
            Log("ocl_help                - 显示帮助");
            
            Log("=== 输入机制说明 ===");
            Log("Click 机制: 按下后自动释放，适用于跳跃等短暂动作");
            Log("Press/Release 机制: 需要手动释放，适用于蹲下等持续动作");
        }
        
        [Command(alias = "ocl_help")]
        public static void Help()
        {
            Log("=== OpenClaw 控制台命令帮助 (简化版) ===");
            Log("这些命令用于测试 OpenClaw 输入设备");
            Log("在游戏控制台中输入命令（通常按 ~ 键打开）");
            Log("");
            Log("示例:");
            Log("  ocl_move 0.5 0.3   # 向右前方移动");
            Log("  ocl_jump           # 跳跃 (默认0.1秒后自动释放)");
            Log("  ocl_jump 0.2       # 跳跃 (0.2秒后自动释放)");
            Log("  ocl_crouch true    # 蹲下 (按下)");
            Log("  ocl_crouch false   # 站起 (释放)");
            Log("");
            Log("输入机制:");
            Log("  Click 机制: 按下后自动释放，用于短暂动作");
            Log("  Press/Release 机制: 需要手动释放，用于持续动作");
            Log("");
            Log("输入 'ocl_status' 查看所有可用命令");
        }
        
        #endregion
        
        #region 初始化
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            // 确保输入管理器存在
            var manager = OpenClawInputManager.EnsureInstance();
            
            // 注册命令
            RegisterCommands();
            
            Log("OpenClaw 控制台命令已初始化 (简化版)");
            Log("支持 Click 和 Press/Release 两种输入机制");
            Log("输入 'ocl_help' 查看可用命令");
        }
        
        private static void RegisterCommands()
        {
            try
            {
                // 这里应该使用 CommandSystem 的注册方法
                Log("简化版命令系统已准备就绪");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[OpenClawConsole] 命令注册失败: {ex.Message}");
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 简化的命令系统（如果 SickDev.CommandSystem 不可用）
    /// </summary>
    public class SimpleCommandSystemSimplified : MonoBehaviour
    {
        [Header("配置")]
        public bool enableCommandSystem = true;
        public KeyCode consoleKey = KeyCode.BackQuote; // ~ 键
        
        [Header("调试")]
        public bool showConsole = false;
        public string inputText = "";
        
        private List<string> m_CommandHistory = new List<string>();
        private int m_HistoryIndex = 0;
        
        private void Update()
        {
            if (!enableCommandSystem) return;
            
            // 切换控制台显示
            if (Input.GetKeyDown(consoleKey))
            {
                showConsole = !showConsole;
                if (showConsole)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    inputText = "";
                }
            }
            
            // 处理控制台输入
            if (showConsole)
            {
                HandleConsoleInput();
            }
        }
        
        private void HandleConsoleInput()
        {
            // 处理键盘输入
            foreach (char c in Input.inputString)
            {
                if (c == '\b') // 退格
                {
                    if (inputText.Length > 0)
                        inputText = inputText.Substring(0, inputText.Length - 1);
                }
                else if (c == '\n' || c == '\r') // 回车
                {
                    ExecuteCommand(inputText);
                    inputText = "";
                }
                else
                {
                    inputText += c;
                }
            }
            
            // 上下箭头浏览历史
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (m_HistoryIndex < m_CommandHistory.Count)
                {
                    m_HistoryIndex++;
                    inputText = m_CommandHistory[m_CommandHistory.Count - m_HistoryIndex];
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (m_HistoryIndex > 0)
                {
                    m_HistoryIndex--;
                    if (m_HistoryIndex > 0)
                        inputText = m_CommandHistory[m_CommandHistory.Count - m_HistoryIndex];
                    else
                        inputText = "";
                }
            }
        }
        
        private void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;
            
            Debug.Log($"[Command] 执行: {command}");
            
            // 添加到历史
            m_CommandHistory.Add(command);
            if (m_CommandHistory.Count > 50)
                m_CommandHistory.RemoveAt(0);
            m_HistoryIndex = 0;
            
            // 解析和执行命令
            string[] parts = command.Split(' ');
            if (parts.Length == 0) return;
            
            string cmd = parts[0].ToLower();
            
            try
            {
                switch (cmd)
                {
                    case "ocl_move":
                        if (parts.Length >= 3)
                        {
                            float h = float.Parse(parts[1]);
                            float v = float.Parse(parts[2]);
                            OpenClawConsoleCommandsSimplified.Move(h, v);
                        }
                        break;
                        
                    case "ocl_jump":
                        float duration = 0.1f;
                        if (parts.Length >= 2)
                            float.TryParse(parts[1], out duration);
                        OpenClawConsoleCommandsSimplified.Jump(duration);
                        break;
                        
                    case "ocl_crouch":
                        bool crouch = true;
                        if (parts.Length >= 2)
                            bool.TryParse(parts[1], out crouch);
                        OpenClawConsoleCommandsSimplified.Crouch(crouch);
                        break;
                        
                    case "ocl_reset":
                        OpenClawConsoleCommandsSimplified.ResetInput();
                        break;
                        
                    case "ocl_status":
                        OpenClawConsoleCommandsSimplified.Status();
                        break;
                        
                    case "ocl_help":
                        OpenClawConsoleCommandsSimplified.Help();
                        break;
                        
                    default:
                        Debug.LogWarning($"[Command] 未知命令: {cmd}");
                        Debug.Log("输入 'ocl_help' 查看可用命令");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Command] 执行命令失败: {ex.Message}");
            }
        }
        
        private void OnGUI()
        {
            if (!showConsole) return;
            
            // 绘制控制台窗口
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, 200), GUI.skin.box);
            
            GUILayout.Label("OpenClaw 控制台 (简化版，按 ~ 键关闭)");
            GUILayout.Space(5);
            
            // 显示历史
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("命令历史:");
            for (int i = Mathf.Max(0, m_CommandHistory.Count - 5); i < m_CommandHistory.Count; i++)
            {
                GUILayout.Label($"  {m_CommandHistory[i]}");
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(5);
            
            // 输入框
            GUI.SetNextControlName("ConsoleInput");
            inputText = GUILayout.TextField(inputText, GUILayout.Height(30));
            GUI.FocusControl("ConsoleInput");
            
            GUILayout.Space(5);
            
            // 帮助文本
            GUILayout.Label("输入 'ocl_help' 查看可用命令", GUI.skin.label);
            
            GUILayout.EndArea();
        }
        
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/OpenClaw/初始化简化版控制台命令")]
        public static void InitializeConsoleCommands()
        {
            var system = FindObjectOfType<SimpleCommandSystemSimplified>();
            if (system == null)
            {
                GameObject obj = new GameObject("SimpleCommandSystemSimplified");
                system = obj.AddComponent<SimpleCommandSystemSimplified>();
                system.enableCommandSystem = true;
            }
            
            OpenClawConsoleCommandsSimplified.Initialize();
            Debug.Log("[OpenClawConsole] 简化版控制台命令系统已初始化");
        }
        #endif
    }
}