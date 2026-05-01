using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class ActorBase
{
    // Î»ÖÃ
    protected Vector3 m_Pos;
    protected AddressablePoolObject m_SelfObj;
    protected ActorAnimatorComponent m_Animator;
    protected SkillComponent m_SkillComponent;
    protected PropSet m_PropSet;
    public ActorType m_ActorType;
    public Transform m_SelfTF;

    public ActorAnimatorComponent Animator => m_Animator;
    public SkillComponent SkillComponent => m_SkillComponent;

    public void Init(Vector3 pos, AddressablePoolObject obj)
    {
        m_Pos = pos;
        m_SelfObj = obj;
        m_SelfTF = m_SelfObj.Object.transform;
        m_SelfTF.position = m_Pos;
        m_Animator = new ActorAnimatorComponent();
        m_Animator.Init(this);
        m_SkillComponent = new SkillComponent();
        m_SkillComponent.Init(this);
        m_PropSet = new PropSet();
        GameHelper.AddComponent<BattleEventComponent>(m_Animator.Animator.gameObject).SetOwner(this);
        OnInit();
    }

    public virtual void Update()
    {
    }

    public virtual void OnInit()
    {
    }

    public void Dispose()
    {
        m_SelfObj.Dispose();
    }
}
