using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T>
{
    readonly Stack<T> m_Stack = new Stack<T>();
    readonly List<T> m_AllAllocated = new List<T>();
    readonly HashSet<T> m_AllAllocatedSet = new HashSet<T>();
    readonly HashSet<T> m_InPool = new HashSet<T>();

    readonly Func<T> m_CreateFunc;
    readonly Action<T> m_ActionOnInit;
    readonly Action<T> m_ActionOnGet;
    readonly Action<T> m_ActionOnRelease;
    readonly Action<T> m_ActionOnClear;

    bool m_IsCleared;

    public int countAll => m_AllAllocated.Count;
    public int countInactive => m_Stack.Count;
    public int countActive => countAll - countInactive;

    public IReadOnlyList<T> allAllocated => m_AllAllocated;

    public ObjectPool(
        Action<T> actionOnGet,
        Action<T> actionOnRelease,
        Action<T> actionInit = null,
        Action<T> actionOnClear = null,
        int preCreateCount = 0,
        Func<T> createFunc = null)
    {
        m_CreateFunc = createFunc ?? CreateWithDefaultConstructor;
        m_ActionOnGet = actionOnGet;
        m_ActionOnRelease = actionOnRelease;
        m_ActionOnInit = actionInit;
        m_ActionOnClear = actionOnClear;

        WarmUp(preCreateCount);
    }

    static T CreateWithDefaultConstructor()
    {
        try
        {
            return Activator.CreateInstance<T>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"ObjectPool<{typeof(T).Name}>: no default constructor. Pass createFunc when constructing the pool.",
                ex);
        }
    }

    T CreateElement()
    {
        T element = m_CreateFunc();
        if (element == null)
        {
            throw new InvalidOperationException($"ObjectPool<{typeof(T).Name}>: createFunc returned null.");
        }

        m_AllAllocated.Add(element);
        m_AllAllocatedSet.Add(element);
        m_ActionOnInit?.Invoke(element);
        return element;
    }

    public void WarmUp(int preCreateCount)
    {
        if (m_IsCleared)
        {
            Debug.LogError("ObjectPool: already cleared, cannot WarmUp.");
            return;
        }

        if (preCreateCount <= 0)
        {
            return;
        }

        for (int i = 0; i < preCreateCount; i++)
        {
            T element = CreateElement();
            m_Stack.Push(element);
            m_InPool.Add(element);
        }
    }

    // Kept for compatibility with existing call sites that used the old typo.
    public void WarnUp(int preCreateCount)
    {
        WarmUp(preCreateCount);
    }

    public T Get()
    {
        if (m_IsCleared)
        {
            Debug.LogError("ObjectPool: already cleared, cannot Get.");
            return default;
        }

        T element;
        if (m_Stack.Count == 0)
        {
            element = CreateElement();
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
        if (EqualityComparer<T>.Default.Equals(element, default))
        {
            Debug.LogError("ObjectPool: cannot release a null/default object.");
            return;
        }

        if (m_IsCleared)
        {
            Debug.LogError("ObjectPool: already cleared, cannot Release.");
            return;
        }

        if (!m_AllAllocatedSet.Contains(element))
        {
            Debug.LogError("ObjectPool: cannot release an object created by another pool.");
            return;
        }

        if (!m_InPool.Add(element))
        {
            Debug.LogError("ObjectPool: duplicate Release.");
            return;
        }

        m_ActionOnRelease?.Invoke(element);
        m_Stack.Push(element);
    }

    public void Clear()
    {
        if (m_IsCleared)
        {
            return;
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
        m_AllAllocatedSet.Clear();
        m_InPool.Clear();
    }
}
