using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 开火
public class AI_Attack : IAIBehavior
{
    private AI m_AI;

    float ATTACK_INTERVAL = 1.4f;
    float m_LastCheckTime = 0f;
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
        if (m_CurrentTime < m_LastCheckTime + ATTACK_INTERVAL)
        {
            return true;
        }
        bool result = m_AI.AttackPlayer(deltTime);
        if (result)
        {
            m_LastCheckTime = Time.realtimeSinceStartup;
        }
        return result;
    }
}
