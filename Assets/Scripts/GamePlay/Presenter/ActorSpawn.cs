using System;
using UnityEngine;

/// <summary>
/// Actor 创建入口：从池中取出 Prefab 实例，用 <see cref="GameObject.GetComponent{T}"/> /
/// <see cref="GameObject.GetComponentInChildren{T}(bool)"/> 解析对应 View，再交给 Presenter 绑定。
/// </summary>
public static class ActorSpawn
{
    /// <summary>生成玩家（Prefab 须含 <see cref="PlayerView"/>）。</summary>
    public static PlayerActor SpawnPlayer(Vector3 position, string addressableKey)
    {
        if (string.IsNullOrEmpty(addressableKey))
        {
            throw new ArgumentException("addressableKey is null or empty.", nameof(addressableKey));
        }

        AddressablePoolObject poolObj = ResourceLoadMgr.Instance.GetPoolObjectBySourceKey(addressableKey);
        return SpawnPlayer(position, poolObj);
    }

    /// <summary>生成玩家（已有池实例）。</summary>
    public static PlayerActor SpawnPlayer(Vector3 position, AddressablePoolObject poolObject)
    {
        PlayerView view = RequireView<PlayerView>(poolObject);
        view.BindPoolObject(poolObject);
        PlayerActor actor = ActorObjectPools.RentPlayer();
        actor.Init(position, view);
        return actor;
    }

    /// <summary>生成敌人（Prefab 须含 <see cref="EnemyView"/>）。</summary>
    public static MonsterActor SpawnEnemy(Vector3 position, string addressableKey)
    {
        if (string.IsNullOrEmpty(addressableKey))
        {
            throw new ArgumentException("addressableKey is null or empty.", nameof(addressableKey));
        }

        AddressablePoolObject poolObj = ResourceLoadMgr.Instance.GetPoolObjectBySourceKey(addressableKey);
        return SpawnEnemy(position, poolObj);
    }

    /// <summary>生成敌人（已有池实例）。</summary>
    public static MonsterActor SpawnEnemy(Vector3 position, AddressablePoolObject poolObject)
    {
        EnemyView view = RequireView<EnemyView>(poolObject);
        view.BindPoolObject(poolObject);
        MonsterActor actor = ActorObjectPools.RentMonster();
        actor.Init(position, view);
        return actor;
    }

    static T RequireView<T>(AddressablePoolObject poolObject) where T : ActorBaseView
    {
        if (poolObject == null)
        {
            throw new ArgumentNullException(nameof(poolObject));
        }

        GameObject root = poolObject.Object;
        T view = root.GetComponent<T>();
        if (view == null)
        {
            view = root.GetComponentInChildren<T>(true);
        }

        if (view == null)
        {
            throw new InvalidOperationException(
                $"Prefab '{root.name}' must have a {typeof(T).Name} on the root or in children.");
        }

        return view;
    }

    /// <summary>回收玩家 Presenter、Model 与 View（Addressable）。</summary>
    public static void Release(PlayerActor actor)
    {
        actor?.Dispose();
    }

    /// <summary>回收敌人 Presenter、Model 与 View（Addressable）。</summary>
    public static void Release(MonsterActor actor)
    {
        actor?.Dispose();
    }

    // 以actorbase为参数的release方法
    public static void Release(ActorBase actor)
    {
        actor?.Dispose();
        switch (actor)
        {
            case PlayerActor player:
                Release(player);
                break;
            case MonsterActor monster:
                Release(monster);
                break;
        }
    }
}
