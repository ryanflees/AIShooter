using System;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CR.OpenClaw
{
    /// <summary>
    /// 最简单的 OpenClaw 测试服务
    /// 只做握手和基本通信测试
    /// </summary>
    public class SimpleOpenClawTest : MonoBehaviour
    {
        [Header("测试配置")]
        [Tooltip("服务器端口")]
        public int port = 8080;
        
        [Tooltip("启用测试服务器")]
        public bool enableTest = true;
        
        [Header("调试")]
        [Tooltip("显示详细日志")]
        public bool debugLogging = true;
        
        private HttpListener m_Listener;
        private Thread m_ServerThread;
        private bool m_IsRunning = false;
        
        #region Unity 生命周期
        
        private void Start()
        {
            if (enableTest)
            {
                StartTestServer();
            }
        }
        
        private void OnDestroy()
        {
            StopTestServer();
        }
        
        #endregion
        
        #region 服务器控制
        
        /// <summary>
        /// 启动测试服务器
        /// </summary>
        public void StartTestServer()
        {
            if (m_IsRunning)
            {
                Log("测试服务器已经在运行");
                return;
            }
            
            try
            {
                m_Listener = new HttpListener();
                m_Listener.Prefixes.Add($"http://localhost:{port}/");
                m_Listener.Prefixes.Add($"http://127.0.0.1:{port}/");
                
                m_Listener.Start();
                m_IsRunning = true;
                
                // 启动服务器线程
                m_ServerThread = new Thread(ServerThread);
                m_ServerThread.IsBackground = true;
                m_ServerThread.Start();
                
                Log($"✅ OpenClaw 测试服务器已启动");
                Log($"   地址: http://localhost:{port}");
                Log($"   测试端点:");
                Log($"     GET  /hello          - 基本握手");
                Log($"     GET  /ping           - 心跳检测");
                Log($"     GET  /unity/info     - Unity 信息");
                Log($"     POST /echo           - 回声测试");
                Log($"     GET  /openclaw/test  - OpenClaw 专用测试");
                
                // 显示测试命令
                Debug.Log("🎮 测试命令:");
                Debug.Log($"curl http://localhost:{port}/hello");
                Debug.Log($"curl http://localhost:{port}/ping");
                Debug.Log($"curl -X POST http://localhost:{port}/echo -H \"Content-Type: application/json\" -d '\"{{\\\"message\\\": \\\"Hello from OpenClaw\\\"}}\"'");
            }
            catch (Exception ex)
            {
                LogError($"❌ 启动服务器失败: {ex.Message}");
                m_IsRunning = false;
            }
        }
        
        /// <summary>
        /// 停止测试服务器
        /// </summary>
        public void StopTestServer()
        {
            if (!m_IsRunning)
                return;
            
            m_IsRunning = false;
            
            try
            {
                m_Listener?.Stop();
                m_Listener?.Close();
                m_Listener = null;
                
                if (m_ServerThread != null && m_ServerThread.IsAlive)
                {
                    m_ServerThread.Join(1000);
                }
                
                Log("测试服务器已停止");
            }
            catch (Exception ex)
            {
                LogError($"停止服务器时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 服务器线程
        /// </summary>
        private void ServerThread()
        {
            while (m_IsRunning && m_Listener != null)
            {
                try
                {
                    // 等待请求
                    var context = m_Listener.GetContext();
                    ProcessRequest(context);
                }
                catch (HttpListenerException)
                {
                    // 监听器被停止
                    break;
                }
                catch (Exception ex)
                {
                    LogError($"服务器线程错误: {ex.Message}");
                }
            }
        }
        
        #endregion
        
        #region 请求处理
        
        /// <summary>
        /// 处理请求
        /// </summary>
        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            
            try
            {
                Log($"收到请求: {request.HttpMethod} {request.Url.LocalPath}");
                
                // 路由请求
                string responseJson = RouteRequest(request);
                
                // 发送响应
                byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                LogError($"处理请求时出错: {ex.Message}");
                
                // 发送错误响应
                string errorJson = $"{{\"error\": \"{ex.Message}\", \"success\": false}}";
                byte[] buffer = Encoding.UTF8.GetBytes(errorJson);
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
        }
        
        /// <summary>
        /// 路由请求
        /// </summary>
        private string RouteRequest(HttpListenerRequest request)
        {
            string path = request.Url.LocalPath.ToLower();
            
            // 基本握手
            if (path == "/hello" && request.HttpMethod == "GET")
            {
                return HandleHello();
            }
            
            // 心跳检测
            if (path == "/ping" && request.HttpMethod == "GET")
            {
                return HandlePing();
            }
            
            // Unity 信息
            if (path == "/unity/info" && request.HttpMethod == "GET")
            {
                return HandleUnityInfo();
            }
            
            // OpenClaw 专用测试
            if (path == "/openclaw/test" && request.HttpMethod == "GET")
            {
                return HandleOpenClawTest();
            }
            
            // 回声测试
            if (path == "/echo" && request.HttpMethod == "POST")
            {
                return HandleEcho(request);
            }
            
            // 未知端点
            return $"{{\"error\": \"未知端点: {path}\", \"success\": false, \"available_endpoints\": [\"/hello\", \"/ping\", \"/unity/info\", \"/openclaw/test\", \"/echo\"]}}";
        }
        
        #endregion
        
        #region 测试端点处理
        
        /// <summary>
        /// 基本握手
        /// </summary>
        private string HandleHello()
        {
            return $@"{{
                ""success"": true,
                ""message"": ""Hello from Unity! 🎮"",
                ""service"": ""SimpleOpenClawTest"",
                ""timestamp"": ""{DateTime.Now:yyyy-MM-dd HH:mm:ss}"",
                ""instructions"": ""通信测试成功！现在可以开始设计你的 OpenClaw + Unity 集成方案了。""
            }}";
        }
        
        /// <summary>
        /// 心跳检测
        /// </summary>
        private string HandlePing()
        {
            return $@"{{
                ""success"": true,
                ""status"": ""alive"",
                ""ping"": ""pong 🏓"",
                ""time"": ""{DateTime.Now:HH:mm:ss}"",
                ""uptime"": ""{Time.time:F1} seconds""
            }}";
        }
        
        /// <summary>
        /// Unity 信息
        /// </summary>
        private string HandleUnityInfo()
        {
            return $@"{{
                ""success"": true,
                ""unity"": {{
                    ""version"": ""{Application.unityVersion}"",
                    ""platform"": ""{Application.platform}"",
                    ""productName"": ""{Application.productName}"",
                    ""fps"": {(1.0f / Time.deltaTime):F1},
                    ""scene"": ""{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}""
                }},
                ""message"": ""Unity 运行正常，准备与 OpenClaw 通信""
            }}";
        }
        
        /// <summary>
        /// OpenClaw 专用测试
        /// </summary>
        private string HandleOpenClawTest()
        {
            return $@"{{
                ""success"": true,
                ""test"": ""openclaw_integration"",
                ""status"": ""ready"",
                ""message"": ""OpenClaw 可以开始控制这个 Unity 游戏了！"",
                ""next_steps"": [
                    ""1. 在 OpenClaw 中测试这个端点"",
                    ""2. 设计你需要的控制接口"",
                    ""3. 扩展这个测试服务器"",
                    ""4. 创建 OpenClaw 技能""
                ],
                ""example_commands"": [
                    ""curl http://localhost:{port}/hello"",
                    ""curl http://localhost:{port}/ping"",
                    ""curl -X POST http://localhost:{port}/echo -d '{{\""text\"": \""Hello\""}}'""
                ]
            }}";
        }
        
        /// <summary>
        /// 回声测试
        /// </summary>
        private string HandleEcho(HttpListenerRequest request)
        {
            try
            {
                // 读取请求体
                string body = "{}";
                if (request.HasEntityBody)
                {
                    using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        body = reader.ReadToEnd();
                    }
                }
                
                return $@"{{
                    ""success"": true,
                    ""echo"": {body},
                    ""received_at"": ""{DateTime.Now:HH:mm:ss.fff}"",
                    ""message"": ""消息已收到并返回""
                }}";
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"回声测试失败: {ex.Message}\", \"success\": false}}";
            }
        }
        
        #endregion
        
        #region 工具方法
        
        /// <summary>
        /// 记录日志
        /// </summary>
        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log($"[OpenClawTest] {message}");
            }
        }
        
        /// <summary>
        /// 记录错误
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[OpenClawTest] {message}");
        }
        
        #endregion
        
        #region 公共 API
        
        /// <summary>
        /// 检查服务器是否运行
        /// </summary>
        public bool IsServerRunning => m_IsRunning;
        
        /// <summary>
        /// 获取服务器 URL
        /// </summary>
        public string ServerUrl => $"http://localhost:{port}";
        
        /// <summary>
        /// 切换服务器状态
        /// </summary>
        public void ToggleServer()
        {
            if (m_IsRunning)
            {
                StopTestServer();
            }
            else
            {
                StartTestServer();
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 编辑器工具
    /// </summary>
    #if UNITY_EDITOR
    //using UnityEditor;
    
    public static class SimpleOpenClawTestEditor
    {
        [MenuItem("GameObject/OpenClaw/添加测试服务", false, 0)]
        public static void AddSimpleTestService()
        {
            // 检查是否已存在
            var existing = UnityEngine.Object.FindObjectOfType<SimpleOpenClawTest>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("OpenClaw 测试", "测试服务已存在于场景中！", "确定");
                Selection.activeObject = existing.gameObject;
                return;
            }
            
            // 创建新的 GameObject
            GameObject testObj = new GameObject("OpenClawTestService");
            
            // 添加组件
            var service = testObj.AddComponent<SimpleOpenClawTest>();
            
            // 配置默认设置
            service.port = 8080;
            service.enableTest = true;
            service.debugLogging = true;
            
            // 选中新对象
            Selection.activeGameObject = testObj;
            
            Debug.Log("✅ OpenClaw 测试服务已添加到场景");
            Debug.Log("   进入 Play Mode 后服务器会自动启动");
            Debug.Log("   测试命令: curl http://localhost:8080/hello");
        }
        
        [MenuItem("Tools/OpenClaw/快速测试连接")]
        public static void QuickTestConnection()
        {
            var service = UnityEngine.Object.FindObjectOfType<SimpleOpenClawTest>();
            
            if (service == null)
            {
                EditorUtility.DisplayDialog("OpenClaw 测试", 
                    "场景中没有测试服务。请先添加：GameObject → OpenClaw → 添加测试服务", 
                    "确定");
                return;
            }
            
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("OpenClaw 测试", 
                    "请进入 Play Mode 进行测试", 
                    "确定");
                return;
            }
            
            if (!service.IsServerRunning)
            {
                EditorUtility.DisplayDialog("OpenClaw 测试", 
                    "测试服务器未运行。请检查组件设置。", 
                    "确定");
                return;
            }
            
            // 显示测试信息
            string message = $@"✅ OpenClaw 测试服务器运行正常！

服务器地址: {service.ServerUrl}

快速测试命令:
1. 基本握手: curl {service.ServerUrl}/hello
2. 心跳检测: curl {service.ServerUrl}/ping  
3. Unity 信息: curl {service.ServerUrl}/unity/info
4. OpenClaw 测试: curl {service.ServerUrl}/openclaw/test
5. 回声测试: curl -X POST {service.ServerUrl}/echo -H ""Content-Type: application/json"" -d '{{""text"": ""Hello""}}'

在 OpenClaw 中测试:
exec command:""curl {service.ServerUrl}/hello""
exec command:""curl {service.ServerUrl}/ping""";

            EditorUtility.DisplayDialog("OpenClaw 连接测试", message, "确定");
        }
    }
    #endif
}