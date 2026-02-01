using UnityEngine;

namespace CR
{
    /// <summary>
    /// 简化的Bobbing曲线计算类
    /// 负责计算摄像机晃动的角度，角度在0~π之间循环
    /// 静止时自动归零
    /// </summary>
    public class BobbingCurve
    {
        private float m_BuiltinCurveAngle = 0f;
        private bool m_WasMoving = false;

        [Header("Speed Settings")]
        public float m_WalkSpeed = 5f;
        public float m_RunSpeed = 7f;
        public float m_SprintSpeed = 10f;
        public float m_CrouchSpeed = 4f;

        [Header("Reset Settings")]
        public float m_AngleResetSpeed = 3f;
        public float m_IdleThreshold = 0.01f;

        public float BuiltinCurveAngle => m_BuiltinCurveAngle;

        /// <summary>
        /// 更新曲线角度
        /// </summary>
        public void UpdateCurve(float dt, PlayerStatus status)
        {
            if (status == null)
                return;

            // 判断玩家是否在移动
            bool isMoving = status.m_CharacterRunSpeedNormalized > m_IdleThreshold;

            if (isMoving)
            {
                // 玩家正在移动 - 根据移动状态推进角度
                float speed = CalculateSpeed(status);
                m_BuiltinCurveAngle += speed * dt;

                // 保持角度在0~π范围内循环
                if (m_BuiltinCurveAngle >= Mathf.PI)
                {
                    m_BuiltinCurveAngle -= Mathf.PI;
                }

                m_WasMoving = true;
            }
            else
            {
                // 玩家停止移动 - 慢慢将角度归零
                if (m_WasMoving && m_BuiltinCurveAngle > 0f)
                {
                    // 平滑归零，为下次移动循环做准备
                    m_BuiltinCurveAngle = Mathf.MoveTowards(m_BuiltinCurveAngle, 0f, m_AngleResetSpeed * dt);

                    // 如果角度已经归零，标记为不再移动
                    if (Mathf.Approximately(m_BuiltinCurveAngle, 0f))
                    {
                        m_BuiltinCurveAngle = 0f;
                        m_WasMoving = false;
                    }
                }
            }

            // 将计算结果赋值给PlayerStatus
            status.m_BuiltinCurveAngle = m_BuiltinCurveAngle;
        }

        /// <summary>
        /// 根据玩家状态计算速度倍数
        /// </summary>
        private float CalculateSpeed(PlayerStatus status)
        {
            // 疾跑优先级最高
            if (status.m_IsSprinting)
            {
                return m_SprintSpeed;
            }

            // 下蹲
            if (status.m_Crouch)
            {
                return status.m_CharacterRunSpeedNormalized * m_CrouchSpeed;
            }

            // 使用归一化速度在行走和跑步之间插值
            float normalizedSpeed = status.m_CharacterRunSpeedNormalized;

            if (normalizedSpeed < 0.5f)
            {
                // 行走
                return normalizedSpeed * 2f * m_WalkSpeed;
            }
            else
            {
                // 跑步
                return Mathf.Lerp(m_WalkSpeed, m_RunSpeed, (normalizedSpeed - 0.5f) * 2f);
            }
        }

        /// <summary>
        /// 立即重置角度（用于传送、重生等）
        /// </summary>
        public void ResetAngle()
        {
            m_BuiltinCurveAngle = 0f;
            m_WasMoving = false;
        }
    }
}
