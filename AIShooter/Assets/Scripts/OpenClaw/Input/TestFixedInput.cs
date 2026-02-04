using System;
using UnityEngine;

namespace CR.OpenClaw
{
    /// <summary>
    /// 测试修复版输入设备
    /// </summary>
    public class TestFixedInput : MonoBehaviour
    {
        [Header("测试配置")]
        public bool runTest = true;
        public float testDelay = 1f;
        
        private float m_Timer = 0f;
        private int m_TestStep = 0;
        
        private OpenClawInputManager m_InputManager;
        
        private void Start()
        {
            Debug.Log("[TestFixedInput] 开始测试修复版输入设备");
            
            // 确保输入管理器存在
            m_InputManager = OpenClawInputManager.EnsureInstance();
            m_InputManager.enableInput = true;
            m_InputManager.debugLog = true;
            
            // 立即开始测试
            m_Timer = testDelay;
        }
        
        private void Update()
        {
            if (!runTest) return;
            
            m_Timer += Time.deltaTime;
            if (m_Timer >= testDelay)
            {
                RunTestStep();
                m_Timer = 0f;
            }
        }
        
        private void RunTestStep()
        {
            if (m_InputManager == null) return;
            
            Debug.Log($"[TestFixedInput] 测试步骤 {m_TestStep}");
            
            switch (m_TestStep)
            {
                case 0:
                    // 测试移动
                    m_InputManager.Move(0.5f, 0.3f);
                    break;
                    
                case 1:
                    // 测试跳跃 (Click 机制)
                    m_InputManager.Jump(0.15f);
                    break;
                    
                case 2:
                    // 测试蹲下 (Press 机制)
                    m_InputManager.Crouch(true);
                    break;
                    
                case 3:
                    // 测试站起 (Release 机制)
                    m_InputManager.Crouch(false);
                    break;
                    
                case 4:
                    // 测试移动 + 跳跃组合
                    m_InputManager.Move(0f, 1f);
                    m_InputManager.Jump(0.2f);
                    break;
                    
                case 5:
                    // 重置输入
                    m_InputManager.ResetInput();
                    break;
                    
                case 6:
                    // 测试长时间跳跃
                    m_InputManager.Jump(0.5f);
                    break;
                    
                case 7:
                    // 测试连续跳跃
                    m_InputManager.Jump(0.1f);
                    DelayAction(0.3f, () => {
                        m_InputManager.Jump(0.1f);
                    });
                    break;
                    
                case 8:
                    // 结束测试
                    m_InputManager.ResetInput();
                    Debug.Log("[TestFixedInput] 测试完成！");
                    runTest = false;
                    break;
            }
            
            m_TestStep = (m_TestStep + 1) % 9;
        }
        
        private void DelayAction(float delay, Action action)
        {
            StartCoroutine(DelayCoroutine(delay, action));
        }
        
        private System.Collections.IEnumerator DelayCoroutine(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/OpenClaw/运行修复版输入测试")]
        public static void RunFixedInputTest()
        {
            var test = FindObjectOfType<TestFixedInput>();
            if (test == null)
            {
                GameObject testObj = new GameObject("TestFixedInput");
                test = testObj.AddComponent<TestFixedInput>();
                test.runTest = false;
            }
            
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                Debug.LogError("[TestFixedInput] 请在 Play Mode 中运行测试");
                return;
            }
            
            test.RunTestStep();
        }
        #endif
    }
}