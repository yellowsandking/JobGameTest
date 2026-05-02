using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Debug.LogError("Use skill");
        GetEnemyListInSector();
        if (m_EnemyList.Count == 0)
        {
            return;
        }
        for (int i = 0; i < m_EnemyList.Count; ++i)
        {
            Debug.LogError("Hit enemy " + m_EnemyList[i].m_SelfTF.name);
        }
    }

    /// <summary>
    /// 扇形范围内的敌人（默认60度，向量夹角要除以2所以angle是30）
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public void GetEnemyListInSector(float angle = 30)
    {
        m_EnemyList.Clear();
        // 得到前方扇形angle度区域内的敌人
        for (int i = 0; i < BattleMgr.Instance.actorList.Count; ++i)
        {
            ActorBase actor = BattleMgr.Instance.actorList[i];
            if (m_Actor.m_ActorType == actor.m_ActorType)
            {
                continue;
            }
            float distance = Vector3.Distance(actor.m_SelfTF.position, m_Actor.m_SelfTF.position);
            if (distance > 3)
            {
                continue;
            }
            float dirAngle = Mathf.Abs(Vector3.Angle(m_Actor.m_SelfTF.forward, actor.m_SelfTF.position - m_Actor.m_SelfTF.position));
            if (dirAngle < angle)
            {
                m_EnemyList.Add(actor);
            }
        }
    }

    public void Update()
    {
        // 画扇形范围
        int radius = 3;
        int angle = 60;

        Vector3 forward = m_Actor.m_SelfTF.forward;
        Vector3 leftDir = Quaternion.Euler(0, -angle / 2, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, angle / 2, 0) * forward;

        // 画两条边
        Debug.DrawLine(m_Actor.m_SelfTF.position, m_Actor.m_SelfTF.position + leftDir * radius, Color.green);
        Debug.DrawLine(m_Actor.m_SelfTF.position, m_Actor.m_SelfTF.position + rightDir * radius, Color.green);

        // 画弧线（用多段线逼近）
        int segments = 20;
        Vector3 prevPoint = m_Actor.m_SelfTF.position + leftDir * radius;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float currentAngle = -angle / 2 + angle * t;

            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 nextPoint = m_Actor.m_SelfTF.position + dir * radius;

            Debug.DrawLine(prevPoint, nextPoint, Color.green);
            prevPoint = nextPoint;
        }
    }
}
