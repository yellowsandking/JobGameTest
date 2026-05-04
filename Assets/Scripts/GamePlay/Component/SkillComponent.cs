using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ???¨¹????????????????????????????¦Ç?¦¶???????
/// </summary>
public class SkillComponent
{
    ActorBase m_Actor;
    List<ActorBase> m_EnemyList = new List<ActorBase>();

    public void Init(ActorBase actor)
    {
        m_Actor = actor;
    }

    public void UseSkill()
    {
        //Debug.LogError("Use skill");
        GetEnemyListInSector();
        if (m_EnemyList.Count == 0)
        {
            return;
        }
        for (int i = 0; i < m_EnemyList.Count; ++i)
        {
            //Debug.LogError("Hit enemy ");
            m_EnemyList[i].OnDamage(m_Actor, m_Actor.m_PropSet[PropType.ATT]);
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public void GetEnemyListInSector(float angle = 30)
    {
        m_EnemyList.Clear();
        for (int i = 0; i < BattleMgr.Instance.actorList.Count; ++i)
        {
            ActorBase actor = BattleMgr.Instance.actorList[i];
            if (m_Actor.m_ActorType == actor.m_ActorType)
            {
                continue;
            }
            float distance = Vector3.Distance(actor.Position, m_Actor.Position);
            if (distance > 3)
            {
                continue;
            }
            float dirAngle = Mathf.Abs(Vector3.Angle(m_Actor.Forward, actor.Position - m_Actor.Position));
            if (dirAngle < angle)
            {
                m_EnemyList.Add(actor);
            }
        }
    }

    public void Update()
    {
        int radius = 3;
        int angle = 60;

        Vector3 forward = m_Actor.Forward;
        Vector3 leftDir = Quaternion.Euler(0, -angle / 2, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, angle / 2, 0) * forward;

        Debug.DrawLine(m_Actor.Position, m_Actor.Position + leftDir * radius, Color.green);
        Debug.DrawLine(m_Actor.Position, m_Actor.Position + rightDir * radius, Color.green);

        int segments = 10;
        Vector3 prevPoint = m_Actor.Position + leftDir * radius;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float currentAngle = -angle / 2 + angle * t;

            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 nextPoint = m_Actor.Position + dir * radius;

            Debug.DrawLine(prevPoint, nextPoint, Color.green);
            prevPoint = nextPoint;
        }
    }
}
