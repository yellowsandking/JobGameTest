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
    Vector3 m_PlayerRelivePos = Vector3.zero;

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
        CreateMainPlayer(Vector3.zero);
        CreateEnemy();
        m_IsInit = true;
        return UniTask.CompletedTask;
    }

    void CreateMainPlayer(Vector3 pos)
    {
        m_PlayerActor = ActorSpawn.SpawnPlayer(pos, "Player");
        m_ActorList.Add(m_PlayerActor);
        BumpActorListRevision();
    }

    void CreateEnemy()
    {
        for (int i = 0; i < 10; ++i)
        {
            // 让怪物出生在半径为10米的圆周上，分布尽可能均匀
            float radius = 10f;
            Vector2 spawnPos = UnityEngine.Random.insideUnitCircle * radius;
            MonsterActor enemy = ActorSpawn.SpawnEnemy(new Vector3(spawnPos.x, 0, spawnPos.y) + m_PlayerActor.Position, "Enemy");
            m_ActorList.Add(enemy);
            BumpActorListRevision();
        }
    }

    public override void OnUpdate()
    {
        for (int i = m_ActorList.Count - 1; i >= 0; --i)
        {
            ActorBase actor = m_ActorList[i];
            if (actor.m_IsDead)
            {
                if (actor.m_CanRecycle)
                {
                    if (actor is PlayerActor p)
                    {
                        m_PlayerRelivePos = p.Position;
                    }
                    m_ActorList.RemoveAt(i);
                    BumpActorListRevision();
                    ActorSpawn.Release(actor);
                }
            }
            else
            {
                m_ActorList[i].Update();
            }
        }
        CheckActorRelive();
    }


    void CheckActorRelive()
    {
        // 检查主角是否死亡，如果死亡则复活
        if (m_PlayerActor == null || !m_ActorList.Contains(m_PlayerActor))
        {
            CreateMainPlayer(m_PlayerRelivePos);
        }

        // 检查怪物数量是否少于6只，如果是，就调用CreateEnemy
        int monsterCount = 0;
        foreach (var actor in m_ActorList)
        {
            if (actor is MonsterActor)
            {
                monsterCount++;
            }
        }
        if (monsterCount < 6)
        {
            CreateEnemy();
        }
    }

    void ClearBattle()
    {
        foreach (var actor in m_ActorList)
        {
            switch (actor)
            {
                case PlayerActor player:
                    ActorSpawn.Release(player);
                    break;
                case MonsterActor monster:
                    ActorSpawn.Release(monster);
                    break;
            }
        }
        m_ActorList.Clear();
        BumpActorListRevision();
    }
}
