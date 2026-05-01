using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//寻找玩家
public class AI_Find_Player : IAIBehavior
{
    private AI m_AI;
    float m_LastCheckTime = 0f;
    float m_CurrentTime = 0;

    public AI_Find_Player(AI ai)
    {
        this.m_AI = ai;
    }

    public int aiType
    {
        get { return (int)AIBehavoirType.eFindPlayer; }
    }

    public bool Update(float deltTime)
    {
        m_CurrentTime = Time.realtimeSinceStartup;

        bool result = m_AI.MoveToPlayer(deltTime);
        if (result == true)
        {
            m_LastCheckTime = Time.realtimeSinceStartup;
        }

        return result;
    }
}
