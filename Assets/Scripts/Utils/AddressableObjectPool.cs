using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AddressableObjectPool
{
    private readonly Stack<AddressablePoolObject> m_Stack = new Stack<AddressablePoolObject>();
    private readonly Run<AddressablePoolObject> m_ActionOnGet;
    private readonly Run<AddressablePoolObject> m_ActionOnRelease;
    bool m_IsDispose = false;
    string m_AddrKey;
    GameObject m_BindObj;
    GameObject m_LoadGameObject;
    GameObject m_PoolObj;

    public GameObject PoolGameObject
    {
        get { return m_PoolObj; }
    }

    public int countAll { get; private set; }
    public int countActive { get { return countAll - countInactive; } }
    public int countInactive { get { return m_Stack.Count; } }

    public AddressableObjectPool(string addrKey, GameObject bindObj, Run<AddressablePoolObject> actionOnGet, Run<AddressablePoolObject> actionOnRelease)
    {
        m_IsDispose = false;
        m_PoolObj = new GameObject(addrKey + "_pool");
        m_PoolObj.transform.position = Vector3.zero;
        m_ActionOnGet = actionOnGet;
        m_ActionOnRelease = actionOnRelease;
        m_AddrKey = addrKey;
        m_BindObj = bindObj;
    }

    public async UniTask WarnUp(int preCreateCount)
    {
        if (preCreateCount <= 0)
        {
            return;
        }

        m_LoadGameObject = await Addr.LoadAssetAsyncTask<GameObject>(m_AddrKey, m_BindObj);
        if (m_LoadGameObject == null)
        {
            Debug.LogError("LoadAssetAsyncTask error, addr key: " + m_AddrKey);
            return;
        }
        //m_LoadGameObject.SetActive(false);
        for (int i = 0, len = preCreateCount; i < preCreateCount; i++)
        {
            GameObject obj = GameObject.Instantiate(m_LoadGameObject);
            obj.transform.SetParent(m_PoolObj.transform);
            AddressablePoolObject poolObj = new AddressablePoolObject(this, obj);
            Release(poolObj);
        }
    }

    public AddressablePoolObject Get()
    {
        if (m_LoadGameObject == null)
        {
            Debug.LogError("LoadAssetAsyncTask error, addr key: " + m_AddrKey);
            return null;
        }
        AddressablePoolObject element;
        if (m_Stack.Count == 0)
        {
            GameObject obj = GameObject.Instantiate(m_LoadGameObject);
            obj.transform.SetParent(m_PoolObj.transform);
            element = new AddressablePoolObject(this, obj);
            countAll++;
        }
        else
        {
            element = m_Stack.Pop();
        }
        if (m_ActionOnGet != null)
            m_ActionOnGet(element);
        element.Reset();
        element.Object.SetActive(true);
        return element;
    }

    public void Release(AddressablePoolObject element)
    {
        if (m_IsDispose)
        {
            return;
        }

        if (element == null || element.Object == null || m_PoolObj == null)
        {
            return;
        }

        if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
        {
            Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            return;
        }
        if (m_ActionOnRelease != null)
        {
            m_ActionOnRelease(element);
        }
        element.Object.SetActive(false);
        element.Object.transform.SetParent(m_PoolObj.transform);
        m_Stack.Push(element);
    }

    public void Dispose()
    {
        //foreach (var item in m_Stack)
        //{
        //    if (item.Object != null)
        //    {
        //        GameObject.Destroy(item.Object);
        //    }
        //}
        //Debug.LogError("pool dispose: " + m_AddrKey);
        m_IsDispose = true;
        m_LoadGameObject = null;
        if (m_PoolObj != null)
        {
            GameObject.Destroy(m_PoolObj);
            m_PoolObj = null;
        }
        m_BindObj = null;
        m_Stack.Clear();
    }
}

public class AddressablePoolObject
{
    AddressableObjectPool m_pool;
    GameObject m_GameObj;
    bool m_IsDisposed;
    bool m_IsDisposing;

    public AddressablePoolObject(AddressableObjectPool pool, GameObject obj)
    {
        m_pool = pool;
        m_GameObj = obj;
        m_IsDisposed = false;
        m_IsDisposing = false;
    }

    public void Reset()
    {
        m_IsDisposed = false;
        m_IsDisposing = false;
    }

    public GameObject Object
    {
        get { return m_GameObj; }
    }

    public void Dispose()
    {
        if (m_IsDisposed || m_IsDisposing)
        {
            return;
        }
        if (m_GameObj == null)
        {
            m_IsDisposed = true;
            return;
        }

        m_IsDisposed = true;
        m_pool.Release(this);
    }
}
