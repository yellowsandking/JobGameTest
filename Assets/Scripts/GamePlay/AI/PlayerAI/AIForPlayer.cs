using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class AIForPlayer : IAI
{
    public IAIBehavior[] m_Behaviors = new IAIBehavior[(int)AIPlayerBehavoirType.eMax];
    AIPlayerBehavoirType m_CurrentBehaviorType = AIPlayerBehavoirType.eAttack;
    PlayerActor m_Actor = null;

    public int currentBehaviorType
    {
        get { return (int)m_CurrentBehaviorType; }
    }

    public void Init(ActorBase actor)
    {
        if ((actor is PlayerActor) == false)
        {
            return;
        }
        m_Actor = actor as PlayerActor;

        m_Behaviors[0] = new AI_AttackForPlayer(this);
    }

    public void LogicUpdate()
    {
        float deltaTime = Time.deltaTime;
        for (int i = 0; i < m_Behaviors.Length; i++)
        {
            IAIBehavior behavior = m_Behaviors[i];
            if (behavior == null)
            {
                continue;
            }

            bool resullt = behavior.Update(deltaTime);
            if (resullt == true)
            {
                ChangeState((AIPlayerBehavoirType)behavior.aiType);
                //Debug.LogError("AI type: " + behavior.aiType.ToString());
                break;
            }
            else
            {
                ChangeState(AIPlayerBehavoirType.eNone);
            }
        }
    }

    public bool AttackEnemy(float deltaTime)
    {
        for (int i = 0; i < BattleMgr.Instance.actorList.Count; ++i)
        {
            ActorBase actor = BattleMgr.Instance.actorList[i];
            if (m_Actor.m_ActorType == actor.m_ActorType)
            {
                continue;
            }
            float distance = Vector3.Distance(actor.Position, m_Actor.Position);
            if (distance > 2.2f)
            {
                continue;
            }
            return true;
        }

        return false;
    }

    public bool Idle(float deltTime)
    {
        return true;
    }

    private void ChangeState(AIPlayerBehavoirType eState, float fKeepTime = 0f)
    {
        if (m_CurrentBehaviorType == eState)
        {
            return;
        }
        switch (eState)
        {
            case AIPlayerBehavoirType.eAttack:
                m_Actor.TriggerAttackPresentation();
                break;
        }
        m_CurrentBehaviorType = eState;
        //m_fStateKeepTime = Time.realtimeSinceStartup + fKeepTime;
    }

    public void Clear()
    { }
}
