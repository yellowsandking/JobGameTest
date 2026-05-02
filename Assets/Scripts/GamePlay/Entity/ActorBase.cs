using System;

using System.Collections;

using System.Collections.Generic;

using System.Diagnostics;

using Unity.VisualScripting;

using UnityEngine;



public class ActorBase : IDamage

{

    protected Vector3 m_Pos;

    protected Quaternion m_Rotation = Quaternion.identity;

    protected ActorPresentation m_Presentation;

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



    public ActorPresentation Presentation => m_Presentation;



    /// <summary>?? Animator ??????? null??</summary>

    public Animator Animator => m_Presentation?.Animator;



    public SkillComponent SkillComponent => m_SkillComponent;



    /// <summary>?????????????? Attack Trigger ???</summary>

    public void TriggerAttackPresentation()

    {

        m_AttackPresentationIntent++;

        m_ActorAnimState = ActorAnimState.Attack;

    }



    public void Init(Vector3 pos, AddressablePoolObject obj)

    {

        m_ActorAnimState = ActorAnimState.Idle;

        m_Pos = pos;

        m_Presentation = new ActorPresentation(obj);

        m_Rotation = m_Presentation.ReadWorldRotation();

        m_SkillComponent = new SkillComponent();

        m_SkillComponent.Init(this);

        m_PropSet = new PropSet();



        GameObject eventHost = m_Presentation.Animator != null

            ? m_Presentation.Animator.gameObject

            : m_Presentation.VisualRoot;

        GameHelper.AddComponent<BattleEventComponent>(eventHost).SetOwner(this);



        OnInit();

        SyncPresentation();

    }



    /// <summary>?????? <see cref="actorAnimState"/> ?????? Animator?</summary>

    public void SyncPresentation()

    {

        m_Presentation?.SyncVisual(m_Pos, m_Rotation, m_ActorAnimState, m_AttackPresentationIntent);

    }



    public virtual void Update()

    {

    }



    public virtual void OnInit()

    {

    }



    public void Dispose()

    {

        m_Presentation?.Dispose();

        m_Presentation = null;

    }



    public virtual void OnDamage(ActorBase from, float damage)

    {

    }

}


