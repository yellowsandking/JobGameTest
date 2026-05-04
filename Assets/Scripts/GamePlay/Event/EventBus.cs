using System;
using System.Collections.Generic;

public static class EventBus
{
    // 事件表：EventType -> 委托列表
    private static readonly Dictionary<Type, List<Delegate>> _eventTable = new();

    // 反查表：用于按 owner 解绑
    private static readonly Dictionary<object, List<(Type, Delegate)>> _ownerMap = new();

    #region Subscribe

    public static void Subscribe<T>(Action<T> handler, object owner = null)
    {
        var type = typeof(T);

        if (!_eventTable.TryGetValue(type, out var list))
        {
            list = new List<Delegate>(4);
            _eventTable[type] = list;
        }

        if (!list.Contains(handler))
            list.Add(handler);

        // 记录 owner -> handler（用于统一解绑）
        if (owner != null)
        {
            if (!_ownerMap.TryGetValue(owner, out var ownerList))
            {
                ownerList = new List<(Type, Delegate)>(4);
                _ownerMap[owner] = ownerList;
            }

            ownerList.Add((type, handler));
        }
    }

    #endregion

    #region Subscribe Once

    public static void SubscribeOnce<T>(Action<T> handler, object owner = null)
    {
        Action<T> wrapper = null;

        wrapper = (e) =>
        {
            Unsubscribe(wrapper);
            handler(e);
        };

        Subscribe(wrapper, owner);
    }

    #endregion

    #region Unsubscribe

    public static void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);

        if (_eventTable.TryGetValue(type, out var list))
        {
            list.Remove(handler);

            if (list.Count == 0)
                _eventTable.Remove(type);
        }
    }

    public static void UnsubscribeAll(object owner)
    {
        if (owner == null) return;

        if (_ownerMap.TryGetValue(owner, out var list))
        {
            foreach (var (type, handler) in list)
            {
                if (_eventTable.TryGetValue(type, out var handlers))
                {
                    handlers.Remove(handler);

                    if (handlers.Count == 0)
                        _eventTable.Remove(type);
                }
            }

            _ownerMap.Remove(owner);
        }
    }

    #endregion

    #region Publish

    public static void Publish<T>(T e)
    {
        var type = typeof(T);

        if (_eventTable.TryGetValue(type, out var list))
        {
            // 防止遍历过程中被修改
            var temp = list.ToArray();

            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] is Action<T> action)
                {
                    try
                    {
                        action.Invoke(e);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"EventBus Exception: {ex}");
                    }
                }
            }
        }
    }

    #endregion

    #region Debug

    public static void Clear()
    {
        _eventTable.Clear();
        _ownerMap.Clear();
    }

    #endregion
}