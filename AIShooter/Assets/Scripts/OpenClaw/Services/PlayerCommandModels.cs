using System;

namespace CR.OpenClaw
{
    #region Player Command Models (玩家命令)

    [Serializable]
    public class MoveCommandRequest
    {
        public float x;
        public float y;
    }

    [Serializable]
    public class LookCommandRequest
    {
        public float yaw;
        public float pitch;
    }

    [Serializable]
    public class CrouchCommandRequest
    {
        public bool crouch;
    }

    [Serializable]
    public class SprintCommandRequest
    {
        public bool sprint;
    }

    [Serializable]
    public class CommandResponse
    {
        public bool executed;
        public string message;
    }

    #endregion
}
