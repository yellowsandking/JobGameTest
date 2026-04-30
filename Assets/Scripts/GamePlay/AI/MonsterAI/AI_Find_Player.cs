using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//寻找玩家
public class AI_Find_Player : IAIBehavior
{
    private AI m_AI;
    //private float m_AddTime = 0;
    //private const float INTERVAL_TIME = 0.045f;
    float m_LastCheckTime = 0f;
    float m_CurrentTime = 0;
    float m_IntervalTime = 1.0f;
    bool bFirstEnter = true;

    public AI_Find_Player(AI ai)
    {
        this.m_AI = ai;
        //m_LastCheckTime = Time.realtimeSinceStartup;
        bFirstEnter = true;
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
            bFirstEnter = false;
            m_LastCheckTime = Time.realtimeSinceStartup;
        }

        return result;
    }
}
