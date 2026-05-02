using System.Collections.Generic;
using UnityEngine;

public class AI : IAI
{
    /// <summary>与其它怪物圆心小于此距离时产生排斥（XZ 平面）。</summary>
    const float CrowdSeparationRadius = 1.45f;

    /// <summary>与攻击目标（玩家）圆心至少保持此距离；更近则沿径向推开。</summary>
    const float MinSeparationFromAttackTarget = 1f;

    /// <summary>排斥位移相对移动速度的倍率，越大分得越开。</summary>
    const float CrowdSeparationSpeedFactor = 1.15f;

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

    /// <summary>
    /// Boids 式分离：与附近存活怪物相互推开，避免 Idle / 追击 / 攻击时模型重叠。
    /// 在 <see cref="MonsterActor.Update"/> 中于 <see cref="LogicUpdate"/> 之后调用。
    /// </summary>
    public void ApplyCrowdSeparation(float deltaTime)
    {
        if (m_Actor == null || m_Actor.m_ActorType != ActorType.Monster)
        {
            return;
        }

        if (m_Actor.actorAnimState == ActorAnimState.Dead)
        {
            return;
        }

        BattleMgr battle = BattleMgr.Instance;
        if (battle == null)
        {
            return;
        }

        Vector3 self = m_Actor.Position;
        Vector3 separation = Vector3.zero;
        IReadOnlyList<ActorBase> actors = battle.actorList;

        for (int i = 0; i < actors.Count; i++)
        {
            ActorBase other = actors[i];
            if (other == null || other == m_Actor)
            {
                continue;
            }

            if (other.m_ActorType != ActorType.Monster || other.actorAnimState == ActorAnimState.Dead)
            {
                continue;
            }

            AccumulateRadialSeparation(ref separation, self, other.Position, CrowdSeparationRadius);
        }

        PlayerActor player = battle.mainPlayer;
        if (player != null && player.actorAnimState != ActorAnimState.Dead)
        {
            AccumulateRadialSeparation(ref separation, self, player.Position, MinSeparationFromAttackTarget);
        }

        if (separation.sqrMagnitude < 1e-8f)
        {
            return;
        }

        separation.y = 0f;
        float moveSpeed = m_Actor.m_PropSet[PropType.MOVE_SPEED];
        Vector3 dir = separation.normalized;
        float pushMag = separation.magnitude;
        float step = Mathf.Min(pushMag * moveSpeed * CrowdSeparationSpeedFactor * deltaTime, moveSpeed * deltaTime * 2.5f);
        m_Actor.Position += dir * step;
    }

    /// <summary>XZ 平面：与另一点距离小于 influenceRadius 时，沿远离方向叠加分离向量。</summary>
    static void AccumulateRadialSeparation(ref Vector3 separation, Vector3 selfWorld, Vector3 otherWorld, float influenceRadius)
    {
        Vector3 delta = selfWorld - otherWorld;
        delta.y = 0f;
        float dist = delta.magnitude;
        if (dist < 1e-5f || dist >= influenceRadius)
        {
            return;
        }

        separation += delta.normalized * (influenceRadius - dist);
    }

    /// <summary>水平面内转向玩家，追击与攻击共用。</summary>
    public void FacePlayer(float deltaTime)
    {
        PlayerActor player = BattleMgr.Instance.mainPlayer;
        if (player == null || player.actorAnimState == ActorAnimState.Dead)
        {
            return;
        }

        Vector3 dir = player.Position - m_Actor.Position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-8f)
        {
            return;
        }

        Quaternion targetLook = Quaternion.LookRotation(dir);
        m_Actor.Rotation = Quaternion.RotateTowards(
            m_Actor.Rotation,
            targetLook,
            m_Actor.m_PropSet[PropType.ROTATE_SPEED] * deltaTime);
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
            FacePlayer(deltaTime);

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

        FacePlayer(deltaTime);
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

