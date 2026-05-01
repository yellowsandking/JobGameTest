using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterActor : ActorBase
{
    IAI m_AI = null;
    public override void OnInit()
    {
        m_ActorType = ActorType.Monster;
        m_AI = new AI();
        m_AI.Init(this);
        m_PropSet[PropType.HP_MAX] = 100;
        m_PropSet[PropType.HP_CUR] = 100;
    }
    public override void Update()
    {
        m_AI.LogicUpdate();
    }
}
