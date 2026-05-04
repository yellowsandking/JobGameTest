using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

public class MonsterActor : ActorBase
{
    IAI m_AI;


    protected override void ReturnSelfToPool()
    {
        ActorObjectPools.ReleaseMonster(this);
    }

    public override void OnInit()
    {
        m_ActorType = ActorType.Monster;
        m_AI = new AI();
        m_AI.Init(this);
        Model.PropSet[PropType.HP_MAX] = 100;
        Model.PropSet[PropType.HP_CUR] = 100;
        Model.PropSet[PropType.MOVE_SPEED] = 5;
        Model.PropSet[PropType.ROTATE_SPEED] = 500;
        Model.PropSet[PropType.ATT] = 30;
    }

    public override void Update()
    {
        if (Model.PropSet[PropType.HP_CUR] <= 0)
        {
            return;
        }
        m_AI.LogicUpdate();
        if (m_AI is AI monsterAi)
        {
            monsterAi.ApplyCrowdSeparation(Time.deltaTime);
        }

        SyncPresentation();
    }
}
