using UnityEngine;

public enum ActorAnimState
{
    Idle,
    Run,
    Attack,
    Dead
}

/// <summary>
/// 角色场景表现：根 Transform、Animator 参数与逻辑层位姿/动画状态同步。
/// </summary>
public class ActorPresentation
{
    static readonly int SpeedID = Animator.StringToHash("Speed");
    static readonly int AttackID = Animator.StringToHash("Attack");
    static readonly int DeadID = Animator.StringToHash("Dead");

    readonly AddressablePoolObject m_PoolObject;
    readonly Transform m_Root;
    readonly Animator m_Animator;

    ActorAnimState? m_LastSyncedAnimState;
    int m_LastAppliedAttackIntentId = -1;

    public Animator Animator => m_Animator;

    public GameObject VisualRoot => m_Root.gameObject;

    public ActorPresentation(AddressablePoolObject poolObject)
    {
        m_PoolObject = poolObject;
        m_Root = poolObject.Object.transform;
        m_Animator = m_Root.GetComponentInChildren<Animator>();
    }

    public Quaternion ReadWorldRotation()
    {
        return m_Root.rotation;
    }

    public void ApplyWorldPose(Vector3 worldPosition, Quaternion worldRotation)
    {
        m_Root.SetPositionAndRotation(worldPosition, worldRotation);
    }

    /// <summary>
    /// 同步位姿；按逻辑状态驱动 Animator（Attack 依赖 attackPresentationIntent 区分多次触发）。
    /// </summary>
    public void SyncVisual(Vector3 worldPosition, Quaternion worldRotation, ActorAnimState animState, int attackPresentationIntent)
    {
        ApplyWorldPose(worldPosition, worldRotation);
        if (m_Animator == null)
        {
            return;
        }

        if (animState == ActorAnimState.Attack)
        {
            if (attackPresentationIntent == m_LastAppliedAttackIntentId)
            {
                return;
            }

            PlayAnimation(animState);
            m_LastAppliedAttackIntentId = attackPresentationIntent;
            m_LastSyncedAnimState = null;
            return;
        }

        if (animState == ActorAnimState.Dead)
        {
            if (m_LastSyncedAnimState.HasValue && m_LastSyncedAnimState.Value == ActorAnimState.Dead)
            {
                return;
            }

            PlayAnimation(animState);
            m_LastSyncedAnimState = ActorAnimState.Dead;
            return;
        }

        if (m_LastSyncedAnimState.HasValue && m_LastSyncedAnimState.Value == animState)
        {
            return;
        }

        PlayAnimation(animState);
        m_LastSyncedAnimState = animState;
    }

    void PlayAnimation(ActorAnimState state)
    {
        //Debug.Log($"[ActorPresentation] PlayAnimation trigger: {state}, root={m_Root.name}", m_Root.gameObject);

        switch (state)
        {
            case ActorAnimState.Idle:
                m_Animator.SetInteger(SpeedID, 0);
                break;
            case ActorAnimState.Run:
                m_Animator.SetInteger(SpeedID, 1);
                break;
            case ActorAnimState.Attack:
                m_Animator.SetTrigger(AttackID);
                break;
            case ActorAnimState.Dead:
                m_Animator.SetTrigger(DeadID);
                break;
        }
    }

    public void ChangeAnimatorSpeed(float speed)
    {
        if (m_Animator != null)
        {
            m_Animator.speed = speed;
        }
    }

    public void Dispose()
    {
        m_PoolObject.Dispose();
    }
}
