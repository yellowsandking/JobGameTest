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

            float dirAngle = Mathf.Abs(Vector3.Angle(m_Actor.m_SelfTF.forward, actor.m_SelfTF.position - m_Actor.m_SelfTF.position));
            if (dirAngle < angle)
            {
                m_EnemyList.Add(actor);
            }
        }
    }
}
