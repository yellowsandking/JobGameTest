using System;
using UnityEngine;

/// <summary>
/// MVP — Presenter：持有 <see cref="ActorModel"/> 与 Prefab 上的 <see cref="ActorBaseView"/>，
/// 将 Model 同步到 View，并挂载技能等组件。
/// </summary>
public class ActorBase
{
    ActorModel m_Model;
    ActorBaseView m_View;

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

    /// <summary>Prefab 上的 View（PlayerView / EnemyView 等）。</summary>
    public ActorBaseView View => m_View;

    public Animator Animator => m_View?.Animator;

    public SkillComponent SkillComponent { get; private set; }

    public void TriggerAttackPresentation()
    {
        m_Model.TriggerAttackPresentation();
    }

    /// <summary>绑定 Model 并完成组件初始化；<paramref name="view"/> 须已通过 <see cref="ActorBaseView.BindPoolObject"/> 绑定池对象。</summary>
    public void Init(Vector3 pos, ActorBaseView view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        m_Model = new ActorModel();
        m_View = view;

        m_Model.AnimState = ActorAnimState.Idle;
        m_Model.Position = pos;
        m_Model.Rotation = view.ReadWorldRotation();

        SkillComponent = new SkillComponent();
        SkillComponent.Init(this);

        GameObject eventHost = view.Animator != null
            ? view.Animator.gameObject
            : view.VisualRoot;
        GameHelper.AddComponent<BattleEventComponent>(eventHost).SetOwner(this);

        OnInit();
        SyncPresentation();
    }

    /// <summary>把当前 Model 状态推送到 View。</summary>
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
        m_View?.Dispose();
        m_View = null;
        m_Model = null;
        SkillComponent = null;
    }

    public virtual void OnDamage(ActorBase from, float damage)
    {
    }
}
