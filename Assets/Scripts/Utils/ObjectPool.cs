using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : new()
{
    private readonly Stack<T> m_Stack = new Stack<T>();
    private readonly List<T> m_AllAllocated = new List<T>();
    private readonly HashSet<T> m_InPool = new HashSet<T>();

    private readonly Action<T> m_ActionOnInit;
    private readonly Action<T> m_ActionOnGet;
    private readonly Action<T> m_ActionOnRelease;
    private readonly Action<T> m_ActionOnClear;

    private bool m_IsCleared = false;

    public int countAll => m_AllAllocated.Count;
    public int countInactive => m_Stack.Count;
    public int countActive => countAll - countInactive;

    public IReadOnlyList<T> allAllocated => m_AllAllocated;

    public ObjectPool(
        Action<T> actionOnGet,
        Action<T> actionOnRelease,
        Action<T> actionInit = null,
        Action<T> actionOnClear = null,
        int preCreateCount = 0)
    {
        m_ActionOnGet = actionOnGet;
        m_ActionOnRelease = actionOnRelease;
        m_ActionOnInit = actionInit;
        m_ActionOnClear = actionOnClear;

        WarnUp(preCreateCount);
    }

    public void WarnUp(int preCreateCount)
    {
        if (m_IsCleared)
        {
            Debug.LogError("ObjectPool: 已 Clear，不能再 WarmUp！");
            return;
        }

        if (preCreateCount <= 0) return;

        for (int i = 0; i < preCreateCount; i++)
        {
            T t = new T();
            m_AllAllocated.Add(t);

            m_ActionOnInit?.Invoke(t); // 先初始化

            m_Stack.Push(t);
            m_InPool.Add(t);
        }
    }

    public T Get()
    {
        if (m_IsCleared)
        {
            Debug.LogError("ObjectPool: 已 Clear，不能再 Get！");
            return default;
        }

        T element;

        if (m_Stack.Count == 0)
        {
            element = new T();
            m_AllAllocated.Add(element);
            m_ActionOnInit?.Invoke(element);
        }
        else
        {
            element = m_Stack.Pop();
            m_InPool.Remove(element);
        }

        m_ActionOnGet?.Invoke(element);
        return element;
    }

    public void Release(T element)
    {
        if (m_IsCleared)
        {
            Debug.LogError("ObjectPool: 已 Clear，禁止 Release！");
            return;
        }

        if (!m_InPool.Add(element))
        {
            Debug.LogError("ObjectPool: 重复回收对象！");
            return;
        }

        m_ActionOnRelease?.Invoke(element);
        m_Stack.Push(element);
    }

    public void Clear()
    {
        if (m_IsCleared)
        {
            return; // 防重复 Clear
        }

        m_IsCleared = true;

        if (m_ActionOnClear != null)
        {
            for (int i = 0; i < m_AllAllocated.Count; i++)
            {
                m_ActionOnClear(m_AllAllocated[i]);
            }
        }

        m_Stack.Clear();
        m_AllAllocated.Clear();
        m_InPool.Clear();
    }
}
