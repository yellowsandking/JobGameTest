using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class ActorBase
{
    // Î»ÖÃ
    protected Vector3 m_Pos;
    protected AddressablePoolObject m_SelfObj;
    protected ActorAnimator m_Animator;
    public Transform m_SelfTF;

    public ActorAnimator Animator => m_Animator;

    public void Init(Vector3 pos, AddressablePoolObject obj)
    {
        m_Pos = pos;
        m_SelfObj = obj;
        m_SelfTF = m_SelfObj.Object.transform;
        m_SelfTF.position = m_Pos;
        m_Animator = new ActorAnimator();
        m_Animator.Init(this);
        GameHelper.AddComponent<BattkeEventReceiver>(m_Animator.Animator.gameObject).SetOwner(this);
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
