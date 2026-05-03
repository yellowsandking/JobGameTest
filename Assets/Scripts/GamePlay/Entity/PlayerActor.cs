using UnityEngine;

public class PlayerActor : ActorBase
{
    int m_Run = 1;

    protected override void ResetPresenterState()
    {
        m_Run = 1;
    }

    protected override void ReturnSelfToPool()
    {
        ActorObjectPools.ReleasePlayer(this);
    }

    public override void OnInit()
    {
        m_ActorType = ActorType.Player;
        Model.PropSet[PropType.HP_MAX] = 100;
        Model.PropSet[PropType.HP_CUR] = 100;
        Model.PropSet[PropType.MOVE_SPEED] = 7;
        Model.PropSet[PropType.ROTATE_SPEED] = 500;
        Model.PropSet[PropType.ATT] = 1;
    }

    public override void Update()
    {
        SkillComponent.Update();
        Vector2 moveDir = InputMgr.Instance.moveDir;
        if (moveDir.sqrMagnitude <= 0)
        {
            if (m_Run > 0)
            {
                m_Run = 0;
                Model.AnimState = ActorAnimState.Idle;
                SyncPresentation();
            }

            return;
        }

        Vector3 velocity = new Vector3(moveDir.x, 0, moveDir.y);
        Model.Position += velocity * Model.PropSet[PropType.MOVE_SPEED] * Time.deltaTime;
        Model.Rotation = Quaternion.LookRotation(velocity);

        m_Run = 1;
        Model.AnimState = ActorAnimState.Run;

        SyncPresentation();
    }
}
