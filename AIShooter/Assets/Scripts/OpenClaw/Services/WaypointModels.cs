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

    [Serializable]
    public class WaypointsResponse
    {
        public WaypointData[] waypoints;
        public int totalCount;
    }

    [Serializable]
    public class WaypointPathResponse
    {
        public int[] path;
        public float totalDistance;
        public bool pathFound;
    }

    [Serializable]
    public class NearestWaypointResponse
    {
        public WaypointData waypoint;
        public float distance;
    }

    #endregion
}
