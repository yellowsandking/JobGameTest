using NaughtyAttributes.Test;
using UnityEditor.SceneManagement;
using UnityEngine;

public class PlayerActor : ActorBase
{
    IAI m_AI;
    int m_Run = 1;
    int m_WingID = 1;

    public int wingID
    {
        get => m_WingID;
        set => m_WingID = value;
    }

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
        m_AI = new AIForPlayer();
        m_AI.Init(this);
        Model.PropSet[PropType.HP_MAX] = 100;
        Model.PropSet[PropType.HP_CUR] = 100;
        Model.PropSet[PropType.MOVE_SPEED] = 3;
        Model.PropSet[PropType.ROTATE_SPEED] = 500;
        Model.PropSet[PropType.ATT] = 50;
        Model.AnimState = ActorAnimState.Idle;
        if (m_WingID > 0)
        {
            WingPresenter wingComponent = Has<WingPresenter>()
                ? Get<WingPresenter>()
                : new WingPresenter();

            Add(wingComponent);
            wingComponent.Init(this, m_WingID);
        }
        SyncPresentation();
    }

    public override void Update()
    {
        SkillComponent.Update();
        if (TryGet(out WingPresenter wingComponent))
        {
            wingComponent.Update();
        }

        m_AI.LogicUpdate();
        Vector2 moveDir = InputMgr.Instance.moveDir;
        if (moveDir.sqrMagnitude <= 0)
        {
            m_Run = 0;
            SetPlayerAnimState();
            return;
        }

        Vector3 velocity = new Vector3(moveDir.x, 0, moveDir.y);
        Model.Position += velocity * Model.PropSet[PropType.MOVE_SPEED] * Time.deltaTime;
        Model.Rotation = Quaternion.LookRotation(velocity);

        m_Run = 1;
        SetPlayerAnimState();
    }

    void SetPlayerAnimState()
    {
        if (m_AI.currentBehaviorType == (int)AIPlayerBehavoirType.eAttack)
        {
            SyncPresentation();
        }
        if (m_Run == 0)
        {
            Model.AnimState = ActorAnimState.Idle;
        }
        else
        {
            Model.AnimState = ActorAnimState.Run;
        }
        SyncPresentation();
    }

}
