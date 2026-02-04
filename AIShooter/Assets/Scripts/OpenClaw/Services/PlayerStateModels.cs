using System;

namespace CR.OpenClaw
{
    #region Player State Models (玩家状态查询)

    [Serializable]
    public class PlayerPositionResponse
    {
        public Vector3Data position;
        public Vector3Data rotation;
        public Vector3Data lookDir;
        public Vector3Data faceDir;
    }

    [Serializable]
    public class PlayerStateResponse
    {
        public bool isGrounded;
        public bool isCrouching;
        public bool isSprinting;
        public bool isSliding;
        public bool isAlive;
        public float moveSpeed;
        public float health;
    }

    #endregion
}
