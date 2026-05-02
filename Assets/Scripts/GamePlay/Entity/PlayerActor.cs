using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActor : ActorBase
{
    int m_Run = 1;
    public override void OnInit()
    {
        m_ActorType = ActorType.Player;
        m_PropSet[PropType.HP_MAX] = 100;
        m_PropSet[PropType.HP_CUR] = 100;
        m_PropSet[PropType.MOVE_SPEED] = 7;
        m_PropSet[PropType.ROTATE_SPEED] = 500;
    }

    public override void Update()
    {
        m_SkillComponent.Update();
        Vector2 moveDir = InputMgr.Instance.moveDir;
        if (moveDir.sqrMagnitude <= 0)
        {
            if (m_Run > 0)
            {
                m_Run = 0;
                m_Animator.PlayAnimation(ActorAnimState.Idle);
            }
            return;
        }
        Vector3 velocity = new Vector3(moveDir.x, 0, moveDir.y);
        m_Pos += velocity * m_PropSet[PropType.MOVE_SPEED] * Time.deltaTime;
        m_Rotation = Quaternion.LookRotation(velocity);
        SyncPresentation();

        m_Run = 1;
        m_Animator.PlayAnimation(ActorAnimState.Run);
    }
}
