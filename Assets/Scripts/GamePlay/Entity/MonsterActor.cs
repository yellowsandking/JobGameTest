using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

public class MonsterActor : ActorBase
{
    IAI m_AI;
    public bool m_IsDead = false;
    public bool m_CanRecycle = false;

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
        Model.PropSet[PropType.ATT] = 10;
        m_IsDead = false;
        m_CanRecycle = false;
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

    public override void OnDamage(ActorBase from, float damage)
    {
        Model.PropSet[PropType.HP_CUR] -= damage;
        if (Model.PropSet[PropType.HP_CUR] <= 0)
        {
            Model.PropSet[PropType.HP_CUR] = 0;
            OnMonsterDead();
        }
    }

    void OnMonsterDead()
    {
        if (m_IsDead)
        {
            return;
        }
        m_IsDead = true;
        Model.AnimState = ActorAnimState.Dead;
        SyncPresentation();
        WaitTimeToRecycle().Forget();
    }

    async UniTaskVoid WaitTimeToRecycle()
    {
        await (View as EnemyView).WaitForDeadAnim();
        m_CanRecycle = true;
    }
}
