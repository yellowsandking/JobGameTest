using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// MVP — Presenter：持有池化的 <see cref="ActorModel"/> 与 Prefab 上的 <see cref="ActorBaseView"/>。
/// </summary>
public abstract class ActorBase
{
    bool m_Disposed;
    ActorModel m_Model;
    ActorBaseView m_View;
    public bool m_IsDead = false;
    public bool m_CanRecycle = false;

    /// <summary>逻辑模型（位置、属性、展示意图等）。</summary>
    public ActorModel Model => m_Model;

    public PropSet m_PropSet => m_Model.PropSet;

    public ActorType m_ActorType
    {
        get => m_Model.ActorType;
        set => m_Model.ActorType = value;
    }

    public Vector3 Position
    {
        get => m_Model.Position;
        set => m_Model.Position = value;
    }

    public Quaternion Rotation
    {
        get => m_Model.Rotation;
        set => m_Model.Rotation = value;
    }

    public ActorAnimState actorAnimState
    {
        get => m_Model.AnimState;
        set => m_Model.AnimState = value;
    }

    public Vector3 Forward => m_Model.Forward;

    public ActorBaseView View => m_View;

    public Animator Animator => m_View?.Animator;

    public SkillComponent SkillComponent { get; private set; }

    public void TriggerAttackPresentation()
    {
        m_Model.TriggerAttackPresentation();
    }

    /// <summary>子类在每次从池取出再 Init 时重置自身字段（如玩家跑步标记）。</summary>
    protected virtual void ResetPresenterState()
    {
    }

    /// <summary>将 Presenter 实例归还对应对象池。</summary>
    protected abstract void ReturnSelfToPool();

    /// <summary>绑定池化 Model 并完成初始化；<paramref name="view"/> 须已通过 <see cref="ActorBaseView.BindPoolObject"/> 绑定池对象。</summary>
    public void Init(Vector3 pos, ActorBaseView view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }
        m_IsDead = false;
        m_CanRecycle = false;
        m_Disposed = false;
        ResetPresenterState();

        m_Model = ActorObjectPools.RentModel();
        m_View = view;
        view.BindPresenter(this);

        m_Model.AnimState = ActorAnimState.Idle;
        m_Model.Position = pos;
        m_Model.Rotation = view.ReadWorldRotation();

        SkillComponent = new SkillComponent();
        SkillComponent.Init(this);

        GameObject eventHost = view.Animator != null
            ? view.Animator.gameObject
            : view.VisualRoot;
        BattleEventComponent battleEvt = eventHost.GetComponent<BattleEventComponent>();
        if (battleEvt == null)
        {
            battleEvt = GameHelper.AddComponent<BattleEventComponent>(eventHost);
        }

        battleEvt.SetOwner(this);

        OnInit();
        SyncPresentation();
    }

    public void SyncPresentation()
    {
        if (m_View == null || m_Model == null)
        {
            return;
        }

        m_View.SyncVisual(
            m_Model.Position,
            m_Model.Rotation,
            m_Model.AnimState,
            m_Model.AttackPresentationIntent);
    }

    public virtual void Update()
    {
    }

    public virtual void OnInit()
    {
    }

    public void Dispose()
    {
        if (m_Disposed)
        {
            return;
        }

        m_Disposed = true;

        m_View?.Dispose();
        m_View = null;

        if (m_Model != null)
        {
            ActorObjectPools.ReleaseModel(m_Model);
            m_Model = null;
        }

        SkillComponent = null;

        ReturnSelfToPool();
    }

    public virtual void OnDamage(ActorBase from, float damage)
    {
        Model.PropSet[PropType.HP_CUR] -= damage;
        if (Model.PropSet[PropType.HP_CUR] <= 0)
        {
            Model.PropSet[PropType.HP_CUR] = 0;
            OnDead();
        }
    }

    void OnDead()
    {
        if (m_IsDead)
        {
            return;
        }
        m_IsDead = true;
        Model.AnimState = ActorAnimState.Dead;
        SyncPresentation();
        WaitTimeToRecycle().Forget();
    }

    async UniTaskVoid WaitTimeToRecycle()
    {
        await View.WaitForDeadAnim();
        m_CanRecycle = true;
    }
}
