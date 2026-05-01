using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActorAnimState
{
    Idle,
    Run,
    Attack,
    Dead
}

public class ActorAnimator
{
    static readonly int SpeedID = Animator.StringToHash("Speed");
    static readonly int AttackID = Animator.StringToHash("Attack");
    static readonly int DeadID = Animator.StringToHash("Dead");

    public ActorBase m_Actor;
    public Animator m_Animator;

    public void Init(ActorBase actor)
    {
        m_Actor = actor;
        m_Animator = m_Actor.m_SelfTF.GetComponentInChildren<Animator>();
    }

    public void PlayAnimation(ActorAnimState state)
    {
        if (m_Animator == null)
        {
            return;
        }

        switch (state)
        {
            case ActorAnimState.Idle:
                {
                    m_Animator.SetInteger(SpeedID, 0);
                }
                break;
            case ActorAnimState.Run:
                {
                    m_Animator.SetInteger(SpeedID, 1);
                }
                break;
            case ActorAnimState.Attack:
                {
                    m_Animator.SetTrigger(AttackID);
                }
                break;
            case ActorAnimState.Dead:
                {
                    m_Animator.SetTrigger(DeadID);
                }
                break;
        }
    }

    public void ChangeSpeed(float speed)
    {
        if (this.m_Animator != null)
        {
            this.m_Animator.speed = speed;
        }
    }

    //public void Update()
    //{
    //}
}
