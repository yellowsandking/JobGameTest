using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class AI : IAI
{
    public IAIBehavior[] m_Behaviors = new IAIBehavior[(int)AIBehavoirType.eMax];
    AIBehavoirType m_CurrentBehaviorType = AIBehavoirType.eIdle;
    ActorBase m_Actor = null;

    public ActorBase actor
    {
        get { return m_Actor; }
    }

    public AIBehavoirType currentBehaviorType
    {
        get { return m_CurrentBehaviorType; }
    }

    public void Init(ActorBase actor)
    {
        if (actor is PlayerActor)
        {
            return;
        }
        m_Actor = actor;

        m_Behaviors[0] = new AI_Attack(this);
        m_Behaviors[1] = new AI_Find_Player(this);
        m_Behaviors[2] = new AI_Idle(this);
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
            if (resullt)
            {
                ChangeState((AIBehavoirType)behavior.aiType);
                //Debug.LogError("AI type: " + behavior.aiType.ToString());
                break;
            }
        }
    }

    public bool MoveToPlayer(float deltaTime)
    {
        PlayerActor player = BattleMgr.Instance.mainPlayer;
        // 移动
        float distance = Vector3.Distance(player.Position, m_Actor.Position);

        if (distance > 6)
        {
            // AOI范围外
            return false;
        }

        if (distance > 2)
        {
            // 转向
            Vector3 dir = player.Position - m_Actor.Position;
            Quaternion targetLook = Quaternion.LookRotation(dir);
            Quaternion qua = Quaternion.RotateTowards(m_Actor.Rotation, targetLook, m_Actor.m_PropSet[PropType.ROTATE_SPEED] * Time.deltaTime);
            m_Actor.Rotation = qua;

            //移动
            m_Actor.Position += m_Actor.Forward * m_Actor.m_PropSet[PropType.MOVE_SPEED] * deltaTime;
            return true;
        }



        return false;

    }

    /// <summary>
    /// 攻击玩家
    /// </summary>
    /// <returns></returns>
    public bool AttackPlayer(float deltaTime)
    {
        PlayerActor player = BattleMgr.Instance.mainPlayer;

        // 判断是否在指定距离内
        float distance = Vector3.Distance(player.Position, m_Actor.Position);
        if (distance > 2)
        {
            return false;
        }

        return true;
    }

    public bool Idle()
    {
        return true;
    }

    private void ChangeState(AIBehavoirType eState, float fKeepTime = 0f)
    {
        if (m_CurrentBehaviorType == eState)
        {
            return;
        }
        switch (eState)
        {
            case AIBehavoirType.eIdle:
                m_Actor.actorAnimState = ActorAnimState.Idle;
                break;
            case AIBehavoirType.eFindPlayer:
                m_Actor.actorAnimState = ActorAnimState.Run;
                break;
            case AIBehavoirType.eAttack:
                m_Actor.TriggerAttackPresentation();
                break;
        }
        m_CurrentBehaviorType = eState;
    }

    public void Clear()
    { }
    //m_fStateKeepTime = Time.realtimeSinceStartup + fKeepTime;
}

