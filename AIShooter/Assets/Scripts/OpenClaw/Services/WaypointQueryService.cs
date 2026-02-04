using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System;

namespace CR.OpenClaw
{
    #region Waypoint Models (路点系统)

    [Serializable]
    public class WaypointData
    {
        public int id;
        public Vector3Data position;
        public int[] connectedIds;
        public float distanceFromPlayer;
        public float angleFromPlayer;
    }

    [Serializable, AgentRes]
    public class WaypointsResponse
    {
        public WaypointData[] waypoints;
        public int totalCount;
    }

    [Serializable, AgentRes]
    public class WaypointPathResponse
    {
        public int[] path;
        public float totalDistance;
        public bool pathFound;
    }

    [Serializable, AgentRes]
    public class NearestWaypointResponse
    {
        public WaypointData waypoint;
        public float distance;
    }

    #endregion
    
    /// <summary>
    /// Service for querying waypoint information
    /// Auto-registers itself with OpenClawAPIServer on Awake
    /// </summary>
    public class WaypointQueryService : MonoBehaviour, IAPIService
    {
        private WaypointContainer m_CachedContainer;
        private Dictionary<int, WaypointNode> m_WaypointMap;
        private float m_CacheTime;
        private const float CACHE_DURATION = 1f;
        private bool m_IsInitialized = false;

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
            registry.RegisterGet("/api/waypoints/all", HandleGetAllWaypoints);
            registry.RegisterGet("/api/waypoints/nearby", HandleGetNearbyWaypoints);
            registry.RegisterGet("/api/waypoints/nearest", HandleGetNearestWaypoint);
            registry.RegisterGet("/api/waypoints/path", HandleGetWaypointPath);
            registry.RegisterGet("/api/waypoints/in-view", HandleGetWaypointsInView);
        }

        #region Endpoint Handlers

        private string HandleGetAllWaypoints(HttpListenerRequest request)
        {
            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => GetAllWaypoints());
        }

        private string HandleGetNearbyWaypoints(HttpListenerRequest request)
        {
            var query = request.QueryString;
            float maxDistance = float.TryParse(query["maxDistance"], out float d) ? d : 50f;
            int maxCount = int.TryParse(query["maxCount"], out int c) ? c : 10;

            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => GetNearbyWaypoints(maxDistance, maxCount));
        }

        private string HandleGetNearestWaypoint(HttpListenerRequest request)
        {
            var query = request.QueryString;
            float x = float.TryParse(query["x"], out float px) ? px : 0f;
            float y = float.TryParse(query["y"], out float py) ? py : 0f;
            float z = float.TryParse(query["z"], out float pz) ? pz : 0f;

            Vector3 position = new Vector3(x, y, z);
            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => GetNearestWaypoint(position));
        }

        private string HandleGetWaypointPath(HttpListenerRequest request)
        {
            var query = request.QueryString;
            int fromId = int.TryParse(query["from"], out int f) ? f : -1;
            int toId = int.TryParse(query["to"], out int t) ? t : -1;

            if (fromId < 0 || toId < 0)
            {
                return ResponseBuilder.CreateStandardError(StandardError.MissingParameter, "Both 'from' and 'to' parameters are required");
            }

            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => GetPath(fromId, toId));
        }

        private string HandleGetWaypointsInView(HttpListenerRequest request)
        {
            var query = request.QueryString;
            float fov = float.TryParse(query["fov"], out float f) ? f : 60f;

            return OpenClawAPIServer.Instance.ExecuteOnMainThread(() => GetWaypointsInView(fov));
        }

        #endregion

        #region Service Methods

        /// <summary>
        /// Get waypoint container and initialize map
        /// </summary>
        private bool InitializeWaypointSystem()
        {
            if (m_IsInitialized && Time.time - m_CacheTime < CACHE_DURATION && m_CachedContainer != null)
            {
                return true;
            }

            m_CachedContainer = FindObjectOfType<WaypointContainer>();
            if (m_CachedContainer == null)
            {
                return false;
            }

            // Build waypoint map for fast lookup
            m_WaypointMap = new Dictionary<int, WaypointNode>();
            foreach (var waypoint in m_CachedContainer.m_WaypointNodeList)
            {
                if (waypoint != null)
                {
                    m_WaypointMap[waypoint.m_ID] = waypoint;
                }
            }

            m_CacheTime = Time.time;
            m_IsInitialized = true;
            return true;
        }

        /// <summary>
        /// Get player controller for position queries
        /// </summary>
        private FPSPlayerController GetPlayerController()
        {
            return FindObjectOfType<FPSPlayerController>();
        }

        /// <summary>
        /// Get all waypoints in the scene
        /// </summary>
        private string GetAllWaypoints()
        {
            if (!InitializeWaypointSystem())
            {
                return ResponseBuilder.CreateStandardError(StandardError.WaypointSystemNotFound);
            }

            var waypointDataList = new List<WaypointData>();

            foreach (var waypoint in m_CachedContainer.m_WaypointNodeList)
            {
                if (waypoint != null)
                {
                    var connectedIds = new List<int>();
                    foreach (var connected in waypoint.connections)
                    {
                        if (connected != null)
                        {
                            connectedIds.Add(connected.m_ID);
                        }
                    }

                    waypointDataList.Add(new WaypointData
                    {
                        id = waypoint.m_ID,
                        position = new Vector3Data(waypoint.transform.position),
                        connectedIds = connectedIds.ToArray(),
                        distanceFromPlayer = 0f,
                        angleFromPlayer = 0f
                    });
                }
            }

            var response = new WaypointsResponse
            {
                waypoints = waypointDataList.ToArray(),
                totalCount = waypointDataList.Count
            };

            return ResponseBuilder.CreateSuccessResponse(response);
        }

        /// <summary>
        /// Get nearby waypoints relative to player
        /// </summary>
        private string GetNearbyWaypoints(float maxDistance, int maxCount)
        {
            if (!InitializeWaypointSystem())
            {
                return ResponseBuilder.CreateStandardError(StandardError.WaypointSystemNotFound);
            }

            var player = GetPlayerController();
            if (player == null)
            {
                return ResponseBuilder.CreateStandardError(StandardError.PlayerNotFound);
            }

            Vector3 playerPos = player.GetPlayerPosition();
            Vector3 playerForward = player.GetPlayerFaceDirection();

            var nearbyList = new List<WaypointData>();

            foreach (var waypoint in m_CachedContainer.m_WaypointNodeList)
            {
                if (waypoint != null)
                {
                    Vector3 waypointPos = waypoint.transform.position;
                    float distance = Vector3.Distance(playerPos, waypointPos);

                    if (distance <= maxDistance)
                    {
                        Vector3 direction = (waypointPos - playerPos).normalized;
                        float angle = Vector3.Angle(playerForward, direction);

                        var connectedIds = new List<int>();
                        foreach (var connected in waypoint.connections)
                        {
                            if (connected != null)
                            {
                                connectedIds.Add(connected.m_ID);
                            }
                        }

                        nearbyList.Add(new WaypointData
                        {
                            id = waypoint.m_ID,
                            position = new Vector3Data(waypointPos),
                            connectedIds = connectedIds.ToArray(),
                            distanceFromPlayer = distance,
                            angleFromPlayer = angle
                        });
                    }
                }
            }

            // Sort by distance
            nearbyList.Sort((a, b) => a.distanceFromPlayer.CompareTo(b.distanceFromPlayer));

            // Limit count
            if (nearbyList.Count > maxCount)
            {
                nearbyList = nearbyList.GetRange(0, maxCount);
            }

            var response = new WaypointsResponse
            {
                waypoints = nearbyList.ToArray(),
                totalCount = nearbyList.Count
            };

            return ResponseBuilder.CreateSuccessResponse(response);
        }

        /// <summary>
        /// Get nearest waypoint to a position
        /// </summary>
        private string GetNearestWaypoint(Vector3 position)
        {
            if (!InitializeWaypointSystem())
            {
                return ResponseBuilder.CreateStandardError(StandardError.WaypointSystemNotFound);
            }

            WaypointNode nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var waypoint in m_CachedContainer.m_WaypointNodeList)
            {
                if (waypoint != null)
                {
                    float distance = Vector3.Distance(position, waypoint.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearest = waypoint;
                        nearestDistance = distance;
                    }
                }
            }

            if (nearest == null)
            {
                return ResponseBuilder.CreateStandardError(StandardError.WaypointSystemNotFound, "No waypoints found in scene");
            }

            var connectedIds = new List<int>();
            foreach (var connected in nearest.connections)
            {
                if (connected != null)
                {
                    connectedIds.Add(connected.m_ID);
                }
            }

            var waypointData = new WaypointData
            {
                id = nearest.m_ID,
                position = new Vector3Data(nearest.transform.position),
                connectedIds = connectedIds.ToArray(),
                distanceFromPlayer = nearestDistance,
                angleFromPlayer = 0f
            };

            var response = new NearestWaypointResponse
            {
                waypoint = waypointData,
                distance = nearestDistance
            };

            return ResponseBuilder.CreateSuccessResponse(response);
        }

        /// <summary>
        /// Get path between two waypoints using BFS
        /// </summary>
        private string GetPath(int fromId, int toId)
        {
            if (!InitializeWaypointSystem())
            {
                return ResponseBuilder.CreateStandardError(StandardError.WaypointSystemNotFound);
            }

            if (!m_WaypointMap.ContainsKey(fromId) || !m_WaypointMap.ContainsKey(toId))
            {
                return ResponseBuilder.CreateStandardError(StandardError.MissingParameter, $"Invalid waypoint IDs: from={fromId}, to={toId}");
            }

            // BFS pathfinding
            var queue = new Queue<int>();
            var visited = new HashSet<int>();
            var parent = new Dictionary<int, int>();

            queue.Enqueue(fromId);
            visited.Add(fromId);

            bool pathFound = false;

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();

                if (current == toId)
                {
                    pathFound = true;
                    break;
                }

                WaypointNode currentNode = m_WaypointMap[current];
                foreach (var connected in currentNode.connections)
                {
                    if (connected != null && !visited.Contains(connected.m_ID))
                    {
                        visited.Add(connected.m_ID);
                        parent[connected.m_ID] = current;
                        queue.Enqueue(connected.m_ID);
                    }
                }
            }

            if (!pathFound)
            {
                var response = new WaypointPathResponse
                {
                    path = new int[0],
                    totalDistance = 0f,
                    pathFound = false
                };
                return ResponseBuilder.CreateSuccessResponse(response);
            }

            // Reconstruct path
            var path = new List<int>();
            int node = toId;

            while (node != fromId)
            {
                path.Add(node);
                node = parent[node];
            }
            path.Add(fromId);
            path.Reverse();

            // Calculate total distance
            float totalDistance = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 pos1 = m_WaypointMap[path[i]].transform.position;
                Vector3 pos2 = m_WaypointMap[path[i + 1]].transform.position;
                totalDistance += Vector3.Distance(pos1, pos2);
            }

            var pathResponse = new WaypointPathResponse
            {
                path = path.ToArray(),
                totalDistance = totalDistance,
                pathFound = true
            };

            return ResponseBuilder.CreateSuccessResponse(pathResponse);
        }

        /// <summary>
        /// Get waypoints in player's field of view
        /// </summary>
        private string GetWaypointsInView(float fov)
        {
            if (!InitializeWaypointSystem())
            {
                return ResponseBuilder.CreateStandardError(StandardError.WaypointSystemNotFound);
            }

            var player = GetPlayerController();
            if (player == null)
            {
                return ResponseBuilder.CreateStandardError(StandardError.PlayerNotFound);
            }

            Vector3 playerPos = player.transform.position;
            Vector3 playerForward = player.transform.forward;

            var inViewList = new List<WaypointData>();

            foreach (var waypoint in m_CachedContainer.m_WaypointNodeList)
            {
                if (waypoint != null)
                {
                    Vector3 waypointPos = waypoint.transform.position;
                    Vector3 direction = (waypointPos - playerPos).normalized;
                    float angle = Vector3.Angle(playerForward, direction);

                    if (angle <= fov)
                    {
                        float distance = Vector3.Distance(playerPos, waypointPos);

                        var connectedIds = new List<int>();
                        foreach (var connected in waypoint.connections)
                        {
                            if (connected != null)
                            {
                                connectedIds.Add(connected.m_ID);
                            }
                        }

                        inViewList.Add(new WaypointData
                        {
                            id = waypoint.m_ID,
                            position = new Vector3Data(waypointPos),
                            connectedIds = connectedIds.ToArray(),
                            distanceFromPlayer = distance,
                            angleFromPlayer = angle
                        });
                    }
                }
            }

            // Sort by distance
            inViewList.Sort((a, b) => a.distanceFromPlayer.CompareTo(b.distanceFromPlayer));

            var response = new WaypointsResponse
            {
                waypoints = inViewList.ToArray(),
                totalCount = inViewList.Count
            };

            return ResponseBuilder.CreateSuccessResponse(response);
        }

        #endregion
    }
}
