using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CR.OpenClaw
{
    public class SimpleOpenClawTest : MonoBehaviour
    {
        [Header("测试配置")]
        public int port = 8091;
        public bool enableTest = true;
        
        [Header("调试")]
        public bool debugLogging = true;
        
        private HttpListener m_Listener;
        private Thread m_ServerThread;
        private bool m_IsRunning = false;

        #region Unity 生命周期
        private void Start()
        {
            if (enableTest) StartTestServer();
        }

        private void OnDestroy()
        {
            StopTestServer();
        }
        #endregion

        #region 服务器核心逻辑
        public void StartTestServer()
        {
            if (m_IsRunning) return;
            try
            {
                m_Listener = new HttpListener();
                m_Listener.Prefixes.Add($"http://localhost:{port}/");
                m_Listener.Prefixes.Add($"http://127.0.0.1:{port}/");
                m_Listener.Start();
                m_IsRunning = true;
                
                m_ServerThread = new Thread(ServerThread) { IsBackground = true };
                m_ServerThread.Start();
                Log($"✅ 服务器启动: http://localhost:{port}");
            }
            catch (Exception ex) { LogError($"启动失败: {ex.Message}"); }
        }

        public void StopTestServer()
        {
            m_IsRunning = false;
            m_Listener?.Stop();
            m_Listener?.Close();
        }

        private void ServerThread()
        {
            while (m_IsRunning && m_Listener.IsListening)
            {
                try
                {
                    var context = m_Listener.GetContext();
                    ProcessRequest(context);
                }
                catch { break; }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            try
            {
                string responseJson = RouteRequest(request);
                byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex) { LogError($"处理请求出错: {ex.Message}"); response.Close(); }
        }
        #endregion

        #region 路由分配 (修复了之前的方法嵌套问题)
        private string RouteRequest(HttpListenerRequest request)
        {
            string path = request.Url.LocalPath.ToLower();
            string method = request.HttpMethod;

            // 基础端点
            if (path == "/hello") return HandleHello();
            if (path == "/ping") return HandlePing();
            if (path == "/unity/info") return HandleUnityInfo();

            // 输入指令端点 (POST)
            if (method == "POST")
            {
                if (path == "/input/jump") return HandleJump(request);
                if (path == "/input/move") return HandleMove(request);
                if (path == "/input/crouch") return HandleCrouch(request);
                if (path == "/input/reset") return HandleResetInput(request);
            }
            
            if (path == "/input/status") return HandleInputStatus();

            return "{\"error\": \"Endpoint not found\"}";
        }
        #endregion

        #region 具体端点处理器 (移至类内部)
        
        private string ReadRequestBody(HttpListenerRequest request)
        {
            using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        private string HandleHello() => "{\"success\": true, \"message\": \"Hello from Unity!\"}";
        private string HandlePing() => "{\"success\": true, \"status\": \"alive\"}";

        private string HandleUnityInfo() => $@"{{
            ""version"": ""{Application.unityVersion}"",
            ""fps"": { (1.0f / Time.smoothDeltaTime):F1}
        }}";

        private string HandleJump(HttpListenerRequest request)
        {
            string body = ReadRequestBody(request);
            var data = JsonUtility.FromJson<JumpRequest>(body) ?? new JumpRequest();
            
            // 注意：这里需要确保你有 UnityMainThreadDispatcher 类
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log($"[Agent Action] Jump for {data.duration}s");
                // 调用你实际的游戏角色脚本
                
                
                OpenClawConsoleCommandsSimplified.Jump(0.2f);
            });

            return "{\"success\": true, \"action\": \"jump\"}";
        }

        private string HandleMove(HttpListenerRequest request)
        {
            string body = ReadRequestBody(request);
            var data = JsonUtility.FromJson<MoveRequest>(body);
            
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log($"[Agent Action] Move: H={data.horizontal}, V={data.vertical}");
            });

            return "{\"success\": true}";
        }

        private string HandleCrouch(HttpListenerRequest request)
        {
            string body = ReadRequestBody(request);
            var data = JsonUtility.FromJson<CrouchRequest>(body);
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log($"[Agent Action] Crouch: {data.crouch}");
            });
            return "{\"success\": true}";
        }

        private string HandleResetInput(HttpListenerRequest request)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log("[Agent Action] Reset Inputs");
            });
            return "{\"success\": true}";
        }

        private string HandleInputStatus()
        {
            return "{\"success\": true, \"status\": \"ready\"}";
        }

        #endregion

        #region 工具
        private void Log(string m) { if (debugLogging) Debug.Log(m); }
        private void LogError(string m) => Debug.LogError(m);
        public bool IsServerRunning => m_IsRunning;
        public string ServerUrl => $"http://localhost:{port}";
        #endregion

        // 定义数据类在类内部或外部均可，只要能被访问
        [System.Serializable] public class JumpRequest { public float duration = 0.1f; }
        [System.Serializable] public class MoveRequest { public float horizontal; public float vertical; }
        [System.Serializable] public class CrouchRequest { public bool crouch; }
    }
}