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
    protected ActorAnimatorComponent m_Animator;
    protected SkillComponent m_SkillComponent;
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

    public Vector3 Forward => m_Rotation * Vector3.forward;

    public ActorPresentation Presentation => m_Presentation;

    public ActorAnimatorComponent Animator => m_Animator;
    public SkillComponent SkillComponent => m_SkillComponent;

    public void Init(Vector3 pos, AddressablePoolObject obj)
    {
        m_Pos = pos;
        m_Presentation = new ActorPresentation(obj);
        m_Rotation = m_Presentation.ReadWorldRotation();
        SyncPresentation();
        m_Animator = new ActorAnimatorComponent();
        m_Animator.Init(this);
        m_SkillComponent = new SkillComponent();
        m_SkillComponent.Init(this);
        m_PropSet = new PropSet();
        GameHelper.AddComponent<BattleEventComponent>(m_Animator.Animator.gameObject).SetOwner(this);
        OnInit();
    }

    /// <summary>?????????? Transform?</summary>
    protected void SyncPresentation()
    {
        if (m_Presentation != null)
        {
            m_Presentation.ApplyWorldPose(m_Pos, m_Rotation);
        }
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
