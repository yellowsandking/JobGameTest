using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 开火
public class AI_Idle : IAIBehavior
{
    private AI m_AI;

    private float m_AddTime = 0;
    private const float INTERVAL_TIME = 0.04f;

    public AI_Idle(AI ai)
    {
        this.m_AI = ai;
    }

    public int aiType
    {
        get { return (int)AIBehavoirType.eIdle; }
    }

    public bool Update(float deltTime)
    {
        //m_AddTime += deltTime;
        //if (m_AddTime < INTERVAL_TIME)
        //{
        //    return false;
        //}
        //m_AddTime = 0;

        bool result = m_AI.Idle();
        return result;
    }
}
