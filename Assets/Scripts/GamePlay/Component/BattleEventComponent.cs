using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEventComponent : MonoBehaviour
{
    ActorBase m_Actor;
    public void SetOwner(ActorBase actor)
    {
        m_Actor = actor;
    }

    public void AttackAction()
    {
        Debug.LogError("AttackAction");
        m_Actor.SkillComponent.UseSkill();
    }
}
