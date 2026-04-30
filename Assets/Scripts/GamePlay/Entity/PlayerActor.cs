using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActor : ActorBase
{
    Animator m_Animator = null;
    int m_Run = 1;
    int m_MoveSpeed = 20;
    public override void OnInit()
    {
        m_Animator = m_SelfTF.GetComponentInChildren<Animator>();
    }

    public override void Update()
    {
        Vector2 moveDir = InputMgr.Instance.moveDir;
        if (moveDir.sqrMagnitude <= 0)
        {
            if (m_Run > 0)
            {
                m_Run = 0;
                m_Animator.SetInteger("Speed", m_Run);
            }
            return;
        }
        Vector3 velocity = new Vector3(moveDir.x, 0, moveDir.y);
        m_Pos += velocity * m_MoveSpeed * Time.deltaTime;
        m_SelfTF.position = m_Pos;

        m_Run = 1;
        m_Animator.SetInteger("Speed", m_Run);
    }
}
