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
        m_PropSet[PropType.MOVE_SPEED] = 8;
        m_PropSet[PropType.ROTATE_SPEED] = 200;
    }
    public override void Update()
    {
        m_AI.LogicUpdate();
    }
}
