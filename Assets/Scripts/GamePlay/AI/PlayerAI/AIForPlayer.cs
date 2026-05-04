using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class AIForPlayer : IAI
{
    public IAIBehavior[] m_Behaviors = new IAIBehavior[(int)AIPlayerBehavoirType.eMax];
    AIPlayerBehavoirType m_CurrentBehaviorType = AIPlayerBehavoirType.eAttack;
    float m_fAISwitchTime = 0.0f;
    float m_RotateSpeed = 200;
    float m_MoveSpeed = 5;
    ActorBase m_Actor = null;

    public AIPlayerBehavoirType currentBehaviorType
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
        }
    }

    public bool MoveToPlayer(float deltaTime)
    {
        PlayerActor player = BattleMgr.Instance.mainPlayer;
        if (!AI.IsPlayerAlive(player))
        {
            return false;
        }

        Vector3 dir = player.Position - m_Actor.Position;
        Quaternion targetLook = Quaternion.LookRotation(dir);
        Quaternion qua = Quaternion.RotateTowards(m_Actor.Rotation, targetLook, m_RotateSpeed * Time.deltaTime);
        m_Actor.Rotation = qua;

        // 移动
        float distance = Vector3.Distance(player.Position, m_Actor.Position);
        if (distance > 2)
        {
            m_Actor.Position += m_Actor.Forward * m_MoveSpeed * deltaTime;
        }

        return true;

    }

    /// <summary>
    /// 攻击玩家
    /// </summary>
    /// <returns></returns>
    public bool AttackPlayer(float deltaTime)
    {
        PlayerActor player = BattleMgr.Instance.mainPlayer;
        if (!AI.IsPlayerAlive(player))
        {
            return false;
        }

        // 判断是否在指定距离内
        float distance = Vector3.Distance(player.Position, m_Actor.Position);
        if (distance > 3)
        {
            return false;
        }

        return true;
    }

    public bool Idle()
    {
        return true;
    }

    private void ChangeState(AIPlayerBehavoirType eState, float fKeepTime = 0f)
    {
        m_CurrentBehaviorType = eState;
        //m_fStateKeepTime = Time.realtimeSinceStartup + fKeepTime;
    }

    public void Clear()
    { }
}
