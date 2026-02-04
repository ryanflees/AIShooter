using System.Net;
using UnityEngine;
using System;

namespace CR.OpenClaw
{
    #region Player Command Models (玩家命令)
    
    [Serializable, AgentReq]
    public class MoveCommandRequest
    {
        public float x;
        public float y;
    }

    [Serializable, AgentReq]
    public class LookCommandRequest
    {
        public float yaw;
        public float pitch;
    }

    [Serializable, AgentReq]
    public class CrouchCommandRequest
    {
        public bool crouch;
    }

    [Serializable, AgentReq]
    public class SprintCommandRequest
    {
        public bool sprint;
    }

    [Serializable, AgentRes]
    public class CommandResponse
    {
        public bool executed;
        public string message;
    }

    #endregion
    
    /// <summary>
    /// Service for executing player commands (write operations)
    /// Auto-registers itself with OpenClawAPIServer on Awake
    /// </summary>
    public class PlayerCommandService : MonoBehaviour, IAPIService
    {
        private FPSPlayerController m_CachedPlayerController;
        private float m_CacheTime;
        private const float CACHE_DURATION = 0.5f;

        private void Start()
        {
            // Auto-register this service with the API server
            OpenClawAPIServer.Instance.RegisterService(this);
        }

        /// <summary>
        /// Register all endpoints for this service
        /// </summary>
        public void RegisterEndpoints(IEndpointRegistry registry)
        {
            registry.RegisterPost("/api/player/move", HandlePlayerMove);
            registry.RegisterPost("/api/player/look", HandlePlayerLook);
            registry.RegisterPost("/api/player/jump", HandlePlayerJump);
            registry.RegisterPost("/api/player/crouch", HandlePlayerCrouch);
            registry.RegisterPost("/api/player/sprint", HandlePlayerSprint);
        }

        #region Endpoint Handlers

        private string HandlePlayerMove(HttpListenerRequest request)
        {
            string body = OpenClawAPIServer.Instance.ReadRequestBody(request);
            var moveCommand = ResponseBuilder.DeserializeRequest<MoveCommandRequest>(body);

            if (!ResponseBuilder.ValidateRequest(moveCommand, out string errorResponse))
            {
                return errorResponse;
            }

            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => ExecuteMove(moveCommand));
        }

        private string HandlePlayerLook(HttpListenerRequest request)
        {
            string body = OpenClawAPIServer.Instance.ReadRequestBody(request);
            var lookCommand = ResponseBuilder.DeserializeRequest<LookCommandRequest>(body);

            if (!ResponseBuilder.ValidateRequest(lookCommand, out string errorResponse))
            {
                return errorResponse;
            }

            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => ExecuteLook(lookCommand));
        }

        private string HandlePlayerJump(HttpListenerRequest request)
        {
            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => ExecuteJump());
        }

        private string HandlePlayerCrouch(HttpListenerRequest request)
        {
            string body = OpenClawAPIServer.Instance.ReadRequestBody(request);
            var crouchCommand = ResponseBuilder.DeserializeRequest<CrouchCommandRequest>(body);

            if (!ResponseBuilder.ValidateRequest(crouchCommand, out string errorResponse))
            {
                return errorResponse;
            }

            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => ExecuteCrouch(crouchCommand));
        }

        private string HandlePlayerSprint(HttpListenerRequest request)
        {
            string body = OpenClawAPIServer.Instance.ReadRequestBody(request);
            var sprintCommand = ResponseBuilder.DeserializeRequest<SprintCommandRequest>(body);

            if (!ResponseBuilder.ValidateRequest(sprintCommand, out string errorResponse))
            {
                return errorResponse;
            }

            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => ExecuteSprint(sprintCommand));
        }

        #endregion

        #region Service Methods

        /// <summary>
        /// Get the player controller (with caching)
        /// </summary>
        private FPSPlayerController GetPlayerController()
        {
            // Refresh cache if expired or null
            if (m_CachedPlayerController == null || Time.time - m_CacheTime > CACHE_DURATION)
            {
                m_CachedPlayerController = FindObjectOfType<FPSPlayerController>();
                m_CacheTime = Time.time;
            }

            return m_CachedPlayerController;
        }

        /// <summary>
        /// Execute move command
        /// </summary>
        private string ExecuteMove(MoveCommandRequest request)
        {
            // var player = GetPlayerController();
            // if (player == null)
            // {
            //     return ResponseBuilder.CreateStandardError(StandardError.PlayerNotFound);
            // }
            //
            // if (player.m_KinematicController == null)
            // {
            //     return ResponseBuilder.CreateStandardError(StandardError.CommandFailed, "Kinematic controller not found");
            // }

            OpenClawInputManager inputManager = OpenClawInputManager.GetInstance();
            try
            {
                // Set movement input on the kinematic controller
                Vector2 localInput = new Vector3(request.x, request.y);

                // Clamp input to reasonable values
                localInput.x = Mathf.Clamp(localInput.x, -1f, 1f);
                localInput.y = Mathf.Clamp(localInput.y, -1f, 1f);
                //
                // // Convert local input to world space using player's rotation
                // Vector3 worldInput = player.transform.TransformDirection(localInput);

                // Apply movement through the kinematic controller's SetInputs method
                //player.m_KinematicController.SetInputs(worldInput, localInput);
                inputManager.Move( localInput.x,  localInput.y);

                var response = new CommandResponse
                {
                    executed = true,
                    message = $"Move command executed: ({localInput.x:F2}, {localInput.y:F2})"
                };

                return ResponseBuilder.CreateSuccessResponse(response);
            }
            catch (System.Exception ex)
            {
                return ResponseBuilder.CreateStandardError(StandardError.CommandFailed, ex.Message);
            }
        }

        /// <summary>
        /// Execute look command
        /// </summary>
        private string ExecuteLook(LookCommandRequest request)
        {
            var player = GetPlayerController();
            if (player == null)
            {
                return ResponseBuilder.CreateStandardError(StandardError.PlayerNotFound);
            }

            try
            {

                //todo
                //// Set player rotation using euler angles
                // Vector3 rotation = new Vector3(request.pitch, request.yaw, 0f);
                // player.SetPlayerRotationEuler(rotation);

                var response = new CommandResponse
                {
                    executed = true,
                    message = $"Look command executed: yaw={request.yaw:F2}, pitch={request.pitch:F2}"
                };

                return ResponseBuilder.CreateSuccessResponse(response);
            }
            catch (System.Exception ex)
            {
                return ResponseBuilder.CreateStandardError(StandardError.CommandFailed, ex.Message);
            }
        }

        /// <summary>
        /// Execute jump command
        /// </summary>
        private string ExecuteJump()
        {
            var player = GetPlayerController();
            // if (player == null)
            // {
            //     return ResponseBuilder.CreateStandardError(StandardError.PlayerNotFound);
            // }
            //
            // if (player.m_KinematicController == null)
            // {
            //     return ResponseBuilder.CreateStandardError(StandardError.CommandFailed, "Kinematic controller not found");
            // }

            try
            {
                // Check if player can jump (must be grounded)
                if (!player.IsOnGround())
                {
                    var response = new CommandResponse
                    {
                        executed = false,
                        message = "Cannot jump: player is not grounded"
                    };
                    return ResponseBuilder.CreateSuccessResponse(response);
                }

                OpenClawInputManager inputManager = OpenClawInputManager.GetInstance();
                inputManager.Jump( );
                
                // Request jump
                //player.m_KinematicController.RequestJump();

                var successResponse = new CommandResponse
                {
                    executed = true,
                    message = "Jump command executed"
                };

                return ResponseBuilder.CreateSuccessResponse(successResponse);
            }
            catch (System.Exception ex)
            {
                return ResponseBuilder.CreateStandardError(StandardError.CommandFailed, ex.Message);
            }
        }

        /// <summary>
        /// Execute crouch command
        /// </summary>
        private string ExecuteCrouch(CrouchCommandRequest request)
        {
            // var player = GetPlayerController();
            // if (player == null)
            // {
            //     return ResponseBuilder.CreateStandardError(StandardError.PlayerNotFound);
            // }
            //
            // if (player.m_KinematicController == null)
            // {
            //     return ResponseBuilder.CreateStandardError(StandardError.CommandFailed, "Kinematic controller not found");
            // }

            try
            {
                // Set crouch state using pose switch methods
                // if (request.crouch)
                // {
                //     player.m_KinematicController.SwitchPoseCrouch();
                // }
                // else
                // {
                //     player.m_KinematicController.SwitchPoseStand();
                // }

                OpenClawInputManager inputManager = OpenClawInputManager.GetInstance();
                inputManager.Crouch(request.crouch);
                var response = new CommandResponse
                {
                    executed = true,
                    message = request.crouch ? "Crouch enabled" : "Crouch disabled"
                };

                return ResponseBuilder.CreateSuccessResponse(response);
            }
            catch (System.Exception ex)
            {
                return ResponseBuilder.CreateStandardError(StandardError.CommandFailed, ex.Message);
            }
        }

        /// <summary>
        /// Execute sprint command
        /// </summary>
        private string ExecuteSprint(SprintCommandRequest request)
        {
            // var player = GetPlayerController();
            // if (player == null)
            // {
            //     return ResponseBuilder.CreateStandardError(StandardError.PlayerNotFound);
            // }
            //
            // if (player.m_KinematicController == null)
            // {
            //     return ResponseBuilder.CreateStandardError(StandardError.CommandFailed, "Kinematic controller not found");
            // }
            //
            // try
            // {
            //     // Set sprint state using move state switch methods
            //     if (request.sprint)
            //     {
            //         player.m_KinematicController.SwitchToSprint();
            //     }
            //     else
            //     {
            //         player.m_KinematicController.SwitchToRun();
            //     }
            //
            //     var response = new CommandResponse
            //     {
            //         executed = true,
            //         message = request.sprint ? "Sprint enabled" : "Sprint disabled"
            //     };
            //
            //     return ResponseBuilder.CreateSuccessResponse(response);
            // }
            // catch (System.Exception ex)
            // {
            //     return ResponseBuilder.CreateStandardError(StandardError.CommandFailed, ex.Message);
            // }
            return ResponseBuilder.CreateStandardError(StandardError.CommandFailed, "not implemented");
        }

        #endregion
    }
}
