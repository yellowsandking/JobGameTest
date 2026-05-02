using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 开火
public class AI_Attack : IAIBehavior
{
    private AI m_AI;

    float ATTACK_DURATION = 1.2f;
    float ATTACK_COOLDOWN = 1.0f;
    float m_LastAttackTime = 0f;
    float m_CurrentTime = 0;

    public AI_Attack(AI ai)
    {
        this.m_AI = ai;
    }

    public int aiType
    {
        get { return (int)AIBehavoirType.eAttack; }
    }

    public bool Update(float deltTime)
    {
        m_CurrentTime = Time.realtimeSinceStartup;
        if (m_CurrentTime < m_LastAttackTime + ATTACK_DURATION)
        {
            return true;
        }

        // 2. 冷却中（攻击结束后的CD）
        if (m_CurrentTime < m_LastAttackTime + ATTACK_DURATION + ATTACK_COOLDOWN)
        {
            return false;
        }

        bool result = m_AI.AttackPlayer(deltTime);
        if (result)
        {
            m_LastAttackTime = Time.realtimeSinceStartup;
        }
        return result;
    }
}
