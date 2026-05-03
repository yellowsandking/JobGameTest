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
    }

    public override void Update()
    {
        m_AI.LogicUpdate();
        if (m_AI is AI monsterAi)
        {
            monsterAi.ApplyCrowdSeparation(Time.deltaTime);
        }

        SyncPresentation();
    }

    public override void OnDamage(ActorBase from, float damage)
    {
        Model.PropSet[PropType.HP_CUR] -= damage;
        if (Model.PropSet[PropType.HP_CUR] <= 0)
        {
            Model.PropSet[PropType.HP_CUR] = 0;
            Model.AnimState = ActorAnimState.Dead;
            SyncPresentation();
        }
    }
}
