using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BattleMgr : GameLogicMgr<BattleMgr>
{
    List<ActorBase> m_ActorList = new List<ActorBase>();
    PlayerActor m_PlayerActor = null;
    int m_ActorListRevision;
    public bool m_IsInit = false;

    /// <summary>
    /// 在 <see cref="m_ActorList"/> 增删或重排后递增；供 UI 等以 O(1) 判断列表是否相对上次变化。
    /// </summary>
    public int actorListRevision => m_ActorListRevision;

    public List<ActorBase> actorList
    {
        get { return m_ActorList; }
    }

    void BumpActorListRevision()
    {
        m_ActorListRevision++;
    }

    public PlayerActor mainPlayer
    {
        get { return m_PlayerActor; }
    }

    public override UniTask OnInit()
    {
        Debug.Log("BattleMgr init");
        CreateMainPlayer();
        CreateEnemy();
        m_IsInit = true;
        return UniTask.CompletedTask;
    }

    void CreateMainPlayer()
    {
        m_PlayerActor = ActorSpawn.SpawnPlayer(Vector3.zero, "Player");
        m_ActorList.Add(m_PlayerActor);
        BumpActorListRevision();
    }

    void CreateEnemy()
    {
        for (int i = 0; i < 5; ++i)
        {
            MonsterActor enemy = ActorSpawn.SpawnEnemy(new Vector3(5, 0, 5 * i), "Enemy");
            m_ActorList.Add(enemy);
            BumpActorListRevision();
        }
    }

    public override void OnUpdate()
    {
        for (int i = m_ActorList.Count - 1; i >= 0; --i)
        {
            if (m_ActorList[i] is MonsterActor monster && monster.m_IsDead)
            {
                if (monster.m_CanRecycle)
                {
                    m_ActorList.RemoveAt(i);
                    BumpActorListRevision();
                    ActorSpawn.Release(monster);
                }
            }
            else
            {
                m_ActorList[i].Update();
            }
        }
    }
}
