using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BattleMgr : GameLogicMgr<BattleMgr>
{
    List<ActorBase> m_ActorList = new List<ActorBase>();
    PlayerActor m_PlayerActor = null;

    public List<ActorBase> actorList
    {
        get { return m_ActorList; }
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
        return UniTask.CompletedTask;
    }

    void CreateMainPlayer()
    {
        m_PlayerActor = new PlayerActor();
        m_PlayerActor.Init(Vector3.zero, ResourceLoadMgr.Instance.GetPoolObjectBySourceKey("Player"));
        m_ActorList.Add(m_PlayerActor);
    }

    void CreateEnemy()
    {
        for (int i = 0; i < 5; ++i)
        {
            MonsterActor enemy = new MonsterActor();
            enemy.Init(new Vector3(5, 0, 5 * i), ResourceLoadMgr.Instance.GetPoolObjectBySourceKey("Enemy"));
            m_ActorList.Add(enemy);
        }
    }

    public override void OnUpdate()
    {
        for (int i = 0; i < m_ActorList.Count; ++i)
        {
            m_ActorList[i].Update();
        }
    }
}
