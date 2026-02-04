using System.Net;
using UnityEngine;

namespace CR.OpenClaw
{
    /// <summary>
    /// Service for querying player state (read-only operations)
    /// Auto-registers itself with OpenClawAPIServer on Awake
    /// </summary>
    public class PlayerStateService : MonoBehaviour, IAPIService
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
            registry.RegisterGet("/api/player/position", HandleGetPlayerPosition);
            registry.RegisterGet("/api/player/state", HandleGetPlayerState);
        }

        #region Endpoint Handlers

        private string HandleGetPlayerPosition(HttpListenerRequest request)
        {
            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => GetPlayerPosition());
        }

        private string HandleGetPlayerState(HttpListenerRequest request)
        {
            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => GetPlayerState());
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
        /// Check if player exists in scene
        /// </summary>
        public bool PlayerExists()
        {
            return GetPlayerController() != null;
        }

        /// <summary>
        /// Get player position and rotation
        /// </summary>
        private string GetPlayerPosition()
        {
            var player = GetPlayerController();
            if (player == null)
            {
                return ResponseBuilder.CreateStandardError(StandardError.PlayerNotFound);
            }

            var response = new PlayerPositionResponse
            {
                position = new Vector3Data(player.transform.position),
                rotation = new Vector3Data(player.transform.eulerAngles),
                lookDir = new Vector3Data(player.GetPlayerRotationEuler()),
                faceDir = new Vector3Data(player.GetPlayerFaceDirection())
            };

            return ResponseBuilder.CreateSuccessResponse(response);
        }

        /// <summary>
        /// Get player state information
        /// </summary>
        private string GetPlayerState()
        {
            var player = GetPlayerController();
            if (player == null)
            {
                return ResponseBuilder.CreateStandardError(StandardError.PlayerNotFound);
            }

            var status = player.m_PlayerStatus;
            var response = new PlayerStateResponse
            {
                isGrounded = status.m_IsOnGround,
                isCrouching = status.m_Crouch,
                isSprinting = status.m_IsSprinting,
                isSliding = status.m_IsSliding,
                isAlive = status.m_IsAlive,
                moveSpeed = status.m_CharacterRunSpeedNormalized,
                health = 100f // Default, can be extended if health system exists
            };

            return ResponseBuilder.CreateSuccessResponse(response);
        }

        #endregion
    }
}
