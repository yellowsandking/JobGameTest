using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class ActorBase
{
    protected Vector3 m_Pos;
    protected Quaternion m_Rotation = Quaternion.identity;
    protected ActorView m_View;
    protected SkillComponent m_SkillComponent;
    protected ActorAnimState m_ActorAnimState;
    int m_AttackPresentationIntent;

    public PropSet m_PropSet;
    public ActorType m_ActorType;

    /// <summary>???????? AI ???????</summary>
    public Vector3 Position
    {
        get => m_Pos;
        set => m_Pos = value;
    }

    /// <summary>????</summary>
    public Quaternion Rotation
    {
        get => m_Rotation;
        set => m_Rotation = value;
    }

    public ActorAnimState actorAnimState
    {
        get => m_ActorAnimState;
        set => m_ActorAnimState = value;
    }

    public Vector3 Forward => m_Rotation * Vector3.forward;

    /// <summary>????? View?????????</summary>
    public ActorView View => m_View;

    public ActorPresentation Presentation => m_View?.Presentation;

    /// <summary>?? Animator ??????? null??</summary>
    public Animator Animator => Presentation?.Animator;

    public SkillComponent SkillComponent => m_SkillComponent;

    /// <summary>?????????????? Attack Trigger ???</summary>
    public void TriggerAttackPresentation()
    {
        m_AttackPresentationIntent++;
        m_ActorAnimState = ActorAnimState.Attack;
    }

    /// <summary>
    /// ????????? View ???View ????????????????
    /// </summary>
    public void Init(Vector3 pos, ActorView view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        m_View = view;
        m_ActorAnimState = ActorAnimState.Idle;
        m_Pos = pos;
        m_Rotation = Presentation.ReadWorldRotation();

        m_SkillComponent = new SkillComponent();
        m_SkillComponent.Init(this);
        m_PropSet = new PropSet();

        GameObject eventHost = Presentation.Animator != null
            ? Presentation.Animator.gameObject
            : Presentation.VisualRoot;
        GameHelper.AddComponent<BattleEventComponent>(eventHost).SetOwner(this);

        OnInit();
        SyncPresentation();
    }

    /// <summary>?????? <see cref="actorAnimState"/> ?????? Animator?</summary>
    public void SyncPresentation()
    {
        Presentation?.SyncVisual(m_Pos, m_Rotation, m_ActorAnimState, m_AttackPresentationIntent);
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
    }

    public virtual void OnDamage(ActorBase from, float damage)
    {
    }
}
