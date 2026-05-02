using System;
using UnityEngine;

/// <summary>
/// Actor 创建统一入口：按 Addressable Key 取池对象、构建 <see cref="ActorView"/>、实例化逻辑体并 <see cref="ActorBase.Init"/>。
/// </summary>
public static class ActorSpawn
{
    /// <summary>使用资源 Key 创建并初始化 Actor。</summary>
    public static T Spawn<T>(Vector3 position, string addressableKey) where T : ActorBase, new()
    {
        if (string.IsNullOrEmpty(addressableKey))
        {
            throw new ArgumentException("addressableKey is null or empty.", nameof(addressableKey));
        }

        AddressablePoolObject poolObj = ResourceLoadMgr.Instance.GetPoolObjectBySourceKey(addressableKey);
        return Spawn<T>(position, poolObj);
    }

    /// <summary>使用已有池实例创建并初始化 Actor（调用方不再单独 Dispose 该 pool 对象，由 Actor.Dispose → View 接管）。</summary>
    public static T Spawn<T>(Vector3 position, AddressablePoolObject poolObject) where T : ActorBase, new()
    {
        if (poolObject == null)
        {
            throw new ArgumentNullException(nameof(poolObject));
        }

        ActorView view = new ActorView(poolObject);
        T actor = new T();
        actor.Init(position, view);
        return actor;
    }
}
