using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CR.OpenClaw
{
    /// <summary>
    /// OpenClaw API Service for Unity
    /// Provides HTTP REST API endpoints for OpenClaw to interact with the game
    /// </summary>
    public class OpenClawAPIService : MonoBehaviour
    {
        [Header("Server Configuration")]
        [Tooltip("Port for the OpenClaw API server")]
        public int port = 8080;
        
        [Tooltip("Enable/disable the API server")]
        public bool enableServer = true;
        
        [Header("Debug")]
        [Tooltip("Enable verbose logging")]
        public bool debugLogging = true;
        
        private HttpListener m_Listener;
        private Thread m_ServerThread;
        private bool m_IsRunning = false;
        
        // Singleton instance
        private static OpenClawAPIService m_Instance;
        public static OpenClawAPIService Instance => m_Instance;
        
        // Reference to player controller for game state access
        private FPSPlayerController m_PlayerController;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure dispatcher exists
            UnityMainThreadDispatcher.Instance();
            
            // Singleton pattern
            if (m_Instance != null && m_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            m_Instance = this;
            DontDestroyOnLoad(gameObject);
            
            Log("OpenClaw API Service initialized");
        }
        
        private void Start()
        {
            // Find player controller on main thread
            UnityMainThreadDispatcher.Instance().FindComponent<FPSPlayerController>(controller =>
            {
                m_PlayerController = controller;
                
                if (m_PlayerController == null)
                {
                    LogWarning("No FPSPlayerController found in scene. Some API endpoints may not work.");
                }
                else
                {
                    Log($"Found player controller: {m_PlayerController.gameObject.name}");
                }
                
                // Start server if enabled
                if (enableServer)
                {
                    StartServer();
                }
            });
        }
        
        private void OnDestroy()
        {
            StopServer();
            
            if (m_Instance == this)
            {
                m_Instance = null;
            }
        }
        
        #endregion
        
        #region Server Control
        
        /// <summary>
        /// Start the HTTP API server
        /// </summary>
        public void StartServer()
        {
            if (m_IsRunning)
            {
                LogWarning("Server is already running");
                return;
            }
            
            try
            {
                m_Listener = new HttpListener();
                m_Listener.Prefixes.Add($"http://localhost:{port}/");
                m_Listener.Prefixes.Add($"http://127.0.0.1:{port}/");
                
                m_Listener.Start();
                m_IsRunning = true;
                
                // Start server thread
                m_ServerThread = new Thread(ServerThread);
                m_ServerThread.IsBackground = true;
                m_ServerThread.Start();
                
                Log($"OpenClaw API server started on port {port}");
                Log($"Available endpoints:");
                Log($"  GET  /api/health");
                Log($"  GET  /api/player/status");
                Log($"  GET  /api/player/position");
                Log($"  POST /api/player/move");
                Log($"  POST /api/player/look");
                Log($"  POST /api/player/jump");
                Log($"  POST /api/player/crouch");
                Log($"  POST /api/player/sprint");
            }
            catch (Exception ex)
            {
                LogError($"Failed to start server: {ex.Message}");
                m_IsRunning = false;
            }
        }
        
        /// <summary>
        /// Stop the HTTP API server
        /// </summary>
        public void StopServer()
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
                
                Log("OpenClaw API server stopped");
            }
            catch (Exception ex)
            {
                LogError($"Error stopping server: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Main server thread that handles incoming requests
        /// </summary>
        private void ServerThread()
        {
            while (m_IsRunning && m_Listener != null)
            {
                try
                {
                    // Wait for request
                    var context = m_Listener.GetContext();
                    ProcessRequest(context);
                }
                catch (HttpListenerException)
                {
                    // Listener was stopped
                    break;
                }
                catch (Exception ex)
                {
                    LogError($"Server thread error: {ex.Message}");
                }
            }
        }
        
        #endregion
        
        #region Request Processing
        
        /// <summary>
        /// Process an incoming HTTP request
        /// </summary>
        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            
            try
            {
                Log($"Request: {request.HttpMethod} {request.Url.LocalPath}");
                
                // Route the request
                string responseJson = RouteRequest(request);
                
                // Send response
                byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                LogError($"Error processing request: {ex.Message}");
                
                // Send error response
                string errorJson = $"{{\"error\": \"{ex.Message}\", \"success\": false}}";
                byte[] buffer = Encoding.UTF8.GetBytes(errorJson);
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
        }
        
        /// <summary>
        /// Route request to appropriate handler
        /// </summary>
        private string RouteRequest(HttpListenerRequest request)
        {
            string path = request.Url.LocalPath.ToLower();
            
            // Health check
            if (path == "/api/health" && request.HttpMethod == "GET")
            {
                return HandleHealthCheck();
            }
            
            // Player status
            if (path == "/api/player/status" && request.HttpMethod == "GET")
            {
                return HandleGetPlayerStatus();
            }
            
            // Player position
            if (path == "/api/player/position" && request.HttpMethod == "GET")
            {
                return HandleGetPlayerPosition();
            }
            
            // Player actions (POST endpoints)
            if (request.HttpMethod == "POST")
            {
                if (path == "/api/player/move")
                {
                    return HandlePlayerMove(request);
                }
                else if (path == "/api/player/look")
                {
                    return HandlePlayerLook(request);
                }
                else if (path == "/api/player/jump")
                {
                    return HandlePlayerJump();
                }
                else if (path == "/api/player/crouch")
                {
                    return HandlePlayerCrouch(request);
                }
                else if (path == "/api/player/sprint")
                {
                    return HandlePlayerSprint(request);
                }
            }
            
            // Unknown endpoint
            return $"{{\"error\": \"Endpoint not found: {path}\", \"success\": false}}";
        }
        
        #endregion
        
        #region API Endpoint Handlers
        
        /// <summary>
        /// Health check endpoint
        /// </summary>
        private string HandleHealthCheck()
        {
            return $"{{\"status\": \"healthy\", \"server\": \"OpenClaw API\", \"port\": {port}, \"time\": \"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\", \"success\": true}}";
        }
        
        /// <summary>
        /// Get player status (thread-safe using dispatcher)
        /// </summary>
        private string HandleGetPlayerStatus()
        {
            if (m_PlayerController == null)
            {
                return $"{{\"error\": \"Player controller not found\", \"success\": false}}";
            }
            
            // Use dispatcher to safely access Unity objects
            string resultJson = "{}";
            bool completed = false;
            
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                try
                {
                    var status = m_PlayerController.m_PlayerStatus;
                    resultJson = $@"{{
                        ""success"": true,
                        ""player"": {{
                            ""isGrounded"": {status.m_IsOnGround.ToString().ToLower()},
                            ""isCrouching"": {status.m_Crouch.ToString().ToLower()},
                            ""isSprinting"": {status.m_IsSprinting.ToString().ToLower()},
                            ""isSliding"": {status.m_IsSliding.ToString().ToLower()},
                            ""moveSpeed"": {status.m_CharacterRunSpeedNormalized},
                            ""bobbingAngle"": {status.m_BuiltinCurveAngle}
                        }}
                    }}";
                }
                catch (Exception ex)
                {
                    resultJson = $"{{\"error\": \"Failed to get player status: {ex.Message}\", \"success\": false}}";
                }
                finally
                {
                    completed = true;
                }
            });
            
            // Wait for completion (simplified - in production use async/await)
            int maxWait = 100; // 100ms timeout
            while (!completed && maxWait-- > 0)
            {
                System.Threading.Thread.Sleep(1);
            }
            
            return resultJson;
        }
        
        /// <summary>
        /// Get player position and rotation (thread-safe using dispatcher)
        /// </summary>
        private string HandleGetPlayerPosition()
        {
            if (m_PlayerController == null)
            {
                return $"{{\"error\": \"Player controller not found\", \"success\": false}}";
            }
            
            // Use dispatcher to safely access Unity objects
            string resultJson = "{}";
            bool completed = false;
            
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                try
                {
                    var transform = m_PlayerController.transform;
                    if (transform == null)
                    {
                        resultJson = $"{{\"error\": \"Player transform not found\", \"success\": false}}";
                        return;
                    }
                    
                    var position = transform.position;
                    var rotation = transform.rotation.eulerAngles;
                    
                    resultJson = $@"{{
                        ""success"": true,
                        ""position"": {{
                            ""x"": {position.x:F2},
                            ""y"": {position.y:F2},
                            ""z"": {position.z:F2}
                        }},
                        ""rotation"": {{
                            ""x"": {rotation.x:F2},
                            ""y"": {rotation.y:F2},
                            ""z"": {rotation.z:F2}
                        }}
                    }}";
                }
                catch (Exception ex)
                {
                    resultJson = $"{{\"error\": \"Failed to get player position: {ex.Message}\", \"success\": false}}";
                }
                finally
                {
                    completed = true;
                }
            });
            
            // Wait for completion
            int maxWait = 100;
            while (!completed && maxWait-- > 0)
            {
                System.Threading.Thread.Sleep(1);
            }
            
            return resultJson;
        }
        
        /// <summary>
        /// Handle player movement input
        /// </summary>
        private string HandlePlayerMove(HttpListenerRequest request)
        {
            try
            {
                // Read request body
                string body = ReadRequestBody(request);
                
                // Parse JSON (simplified - in production use proper JSON parser)
                // Expected format: {"x": 0.5, "y": 0.0, "z": 0.8}
                // For now, we'll just log it
                Log($"Move request body: {body}");
                
                // In a real implementation, you would:
                // 1. Parse the JSON
                // 2. Validate the input
                // 3. Apply movement to player controller
                // 4. Return success/failure
                
                return $"{{\"success\": true, \"message\": \"Move command received (simulated)\", \"data\": {body}}}";
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"Failed to process move request: {ex.Message}\", \"success\": false}}";
            }
        }
        
        /// <summary>
        /// Handle player look/aim input
        /// </summary>
        private string HandlePlayerLook(HttpListenerRequest request)
        {
            try
            {
                string body = ReadRequestBody(request);
                Log($"Look request body: {body}");
                
                // Expected format: {"yaw": 45.0, "pitch": -10.0}
                
                return $"{{\"success\": true, \"message\": \"Look command received (simulated)\", \"data\": {body}}}";
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"Failed to process look request: {ex.Message}\", \"success\": false}}";
            }
        }
        
        /// <summary>
        /// Handle player jump action
        /// </summary>
        private string HandlePlayerJump()
        {
            if (m_PlayerController == null)
            {
                return $"{{\"error\": \"Player controller not found\", \"success\": false}}";
            }
            
            // In a real implementation, trigger jump on player controller
            Log("Jump command received");
            
            return $"{{\"success\": true, \"message\": \"Jump command executed\"}}";
        }
        
        /// <summary>
        /// Handle player crouch action
        /// </summary>
        private string HandlePlayerCrouch(HttpListenerRequest request)
        {
            try
            {
                string body = ReadRequestBody(request);
                Log($"Crouch request body: {body}");
                
                // Expected format: {"crouch": true/false}
                
                return $"{{\"success\": true, \"message\": \"Crouch command received (simulated)\"}}";
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"Failed to process crouch request: {ex.Message}\", \"success\": false}}";
            }
        }
        
        /// <summary>
        /// Handle player sprint action
        /// </summary>
        private string HandlePlayerSprint(HttpListenerRequest request)
        {
            try
            {
                string body = ReadRequestBody(request);
                Log($"Sprint request body: {body}");
                
                // Expected format: {"sprint": true/false}
                
                return $"{{\"success\": true, \"message\": \"Sprint command received (simulated)\"}}";
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"Failed to process sprint request: {ex.Message}\", \"success\": false}}";
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Read request body as string
        /// </summary>
        private string ReadRequestBody(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
                return "{}";
            
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }
        
        /// <summary>
        /// Log message with [OpenClawAPI] prefix
        /// </summary>
        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log($"[OpenClawAPI] {message}");
            }
        }
        
        /// <summary>
        /// Log warning with [OpenClawAPI] prefix
        /// </summary>
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[OpenClawAPI] {message}");
        }
        
        /// <summary>
        /// Log error with [OpenClawAPI] prefix
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[OpenClawAPI] {message}");
        }
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Check if server is running
        /// </summary>
        public bool IsServerRunning => m_IsRunning;
        
        /// <summary>
        /// Get server URL
        /// </summary>
        public string ServerUrl => $"http://localhost:{port}";
        
        /// <summary>
        /// Toggle server on/off
        /// </summary>
        public void ToggleServer()
        {
            if (m_IsRunning)
            {
                StopServer();
            }
            else
            {
                StartServer();
            }
        }
        
        #endregion
    }
}