using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ObjectPool<T> where T : new()
{
    private readonly Stack<T> m_Stack = new Stack<T>();
    private readonly Run<T> m_ActionOnInit;
    private readonly Run<T> m_ActionOnGet;
    private readonly Run<T> m_ActionOnRelease;

    public int countAll { get; private set; }
    public int countActive { get { return countAll - countInactive; } }
    public int countInactive { get { return m_Stack.Count; } }

    public ObjectPool(Run<T> actionOnGet, Run<T> actionOnRelease, Run<T> actionInit = null, int preCreateCount = 0)
    {
        m_ActionOnGet = actionOnGet;
        m_ActionOnRelease = actionOnRelease;
        m_ActionOnInit = actionInit;

        WarnUp(preCreateCount);
    }

    public void WarnUp(int preCreateCount)
    {
        if (preCreateCount <= 0) return;

        T t;
        for (int i = 0, len = preCreateCount; i < preCreateCount; i++)
        {
            t = new T();
            Release(t);
            if (m_ActionOnInit != null)
            {
                m_ActionOnInit(t);
            }
        }
    }

    public T Get()
    {
        T element;
        if (m_Stack.Count == 0)
        {
            element = new T();
            if (m_ActionOnInit != null)
            {
                m_ActionOnInit(element);
            }
            countAll++;
        }
        else
        {
            element = m_Stack.Pop();
        }
        if (m_ActionOnGet != null)
            m_ActionOnGet(element);
        return element;
    }

    public void Release(T element)
    {
        if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
        {
            Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            return;
        }
        if (m_ActionOnRelease != null)
        {
            m_ActionOnRelease(element);
        }
        m_Stack.Push(element);
    }

    public void Clear()
    {
        m_Stack.Clear();
    }
}
public static class ObjectPoolList<T>
{
    // Object pool to avoid allocations.
    private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, Clear);
    static void Clear(List<T> l) { l.Clear(); }

    public static List<T> Get()
    {
        return s_ListPool.Get();
    }

    public static void Release(List<T> toRelease)
    {
        s_ListPool.Release(toRelease);
    }
}
