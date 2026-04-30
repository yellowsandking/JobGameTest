using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterActor : ActorBase
{
    Animator m_Animator = null;
    IAI m_AI = null;
    public override void OnInit()
    {
        m_Animator = m_SelfTF.GetComponentInChildren<Animator>();
        m_AI = new AI();
        m_AI.Init(this);
    }
    public override void Update()
    {
        m_AI.LogicUpdate();
    }
}
