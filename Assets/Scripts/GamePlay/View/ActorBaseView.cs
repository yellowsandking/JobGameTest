using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂在角色 Prefab 上的 View 基类：实现 <see cref="IActorView"/>，负责 Transform / Animator 与对象池回收。
/// 具体角色使用 <see cref="PlayerView"/> 或 <see cref="EnemyView"/>。
/// </summary>
public abstract class ActorBaseView : MonoBehaviour, IActorView
{
    static readonly int SpeedID = Animator.StringToHash("Speed");
    static readonly int AttackID = Animator.StringToHash("Attack");
    static readonly int DeadID = Animator.StringToHash("Dead");
    static readonly int UpperAttackID = Animator.StringToHash("UpperAttack");

    AddressablePoolObject m_PoolObject;
    Animator m_Animator;
    readonly Dictionary<Type, object> _components = new Dictionary<Type, object>();

    /// <summary>绑定后的逻辑层 Presenter，运行期由 <see cref="ActorBase.Init"/> 赋值，供调试/编辑器查看。</summary>
    public ActorBase Presenter { get; private set; }

    ActorAnimState? m_LastSyncedAnimState;
    int m_LastAppliedAttackIntentId = -1;

    public Animator Animator => m_Animator;

    public GameObject VisualRoot => gameObject;

    public void Add<T>(T comp)
    {
        _components[typeof(T)] = comp;
    }

    public T Get<T>()
    {
        return (T)_components[typeof(T)];
    }

    public bool TryGet<T>(out T comp)
    {
        if (_components.TryGetValue(typeof(T), out object value))
        {
            comp = (T)value;
            return true;
        }

        comp = default;
        return false;
    }

    public bool Has<T>()
    {
        return _components.ContainsKey(typeof(T));
    }

    void ClearComponents()
    {
        foreach (object comp in _components.Values)
        {
            if (comp is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _components.Clear();
    }

    /// <summary>
    /// 从对象池取出实例后由 <see cref="ActorSpawn"/> 调用：绑定池回收句柄，并重置动画同步缓存（防止复用时状态错乱）。
    /// </summary>
    internal void BindPresenter(ActorBase presenter)
    {
        Presenter = presenter;
    }

    internal void ClearPresenter()
    {
        Presenter = null;
    }

    public void BindPoolObject(AddressablePoolObject poolObject)
    {
        if (poolObject == null)
        {
            throw new ArgumentNullException(nameof(poolObject));
        }

        m_PoolObject = poolObject;
        m_LastSyncedAnimState = null;
        m_LastAppliedAttackIntentId = -1;
        m_Animator = GetComponentInChildren<Animator>(true);
    }

    public Quaternion ReadWorldRotation()
    {
        return transform.rotation;
    }

    /// <inheritdoc />
    public void SyncVisual(Vector3 worldPosition, Quaternion worldRotation, ActorAnimState animState, int attackPresentationIntent)
    {
        transform.SetPositionAndRotation(worldPosition, worldRotation);
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
        //if (this is PlayerView)
        //{
        //    Debug.Log($"Play animation {state} for {name}");
        //}

        switch (state)
        {
            case ActorAnimState.Idle:
                m_Animator.SetInteger(SpeedID, 0);
                break;
            case ActorAnimState.Run:
                m_Animator.SetInteger(SpeedID, 1);
                break;
            case ActorAnimState.Attack:
                if (this is PlayerView)
                {
                    m_Animator.SetTrigger(UpperAttackID);
                }
                else
                {
                    m_Animator.SetTrigger(AttackID);
                }
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

    /// <inheritdoc />
    public void Dispose()
    {
        ClearPresenter();
        ClearComponents();
        if (m_PoolObject != null)
        {
            m_PoolObject.Dispose();
            m_PoolObject = null;
        }
    }

    protected virtual void OnDestroy()
    {
        ClearPresenter();
        ClearComponents();
        m_PoolObject = null;
    }

    public async UniTask WaitForDeadAnim()
    {
        await UniTask.Delay(2500);
    }
}
