using System;
using UnityEngine;

/// <summary>
/// Player wing component. Owns the wing model and binds a <see cref="WingView"/> to the player's view root.
/// </summary>
public class WingPresenter : IDisposable
{
    const string WingAddressableKey = "Wing";

    static readonly ObjectPool<WingView> s_ViewPool = new ObjectPool<WingView>(
        actionOnGet: null,
        actionOnRelease: OnReleaseView);

    PlayerActor m_Owner;
    WingModel m_Model;
    WingView m_View;
    AddressablePoolObject m_WingPoolObject;

    public WingModel Model => m_Model;
    public WingView View => m_View;

    static void OnReleaseView(WingView view)
    {
        view.Unbind();
    }

    public void Init(PlayerActor owner, int wingID)
    {
        Dispose();
        m_Owner = owner;
        if (m_Model == null)
        {
            m_Model = new WingModel();
        }

        m_Model.Reset();
        m_Model.WingID = wingID;

        Transform parent = owner?.View?.transform;
        if (parent == null)
        {
            Debug.LogError("WingComponent: player view root is null.");
            return;
        }

        m_WingPoolObject = ResourceLoadMgr.Instance.GetPoolObjectBySourceKey(WingAddressableKey);
        if (m_WingPoolObject == null || m_WingPoolObject.Object == null)
        {
            Debug.LogError("WingComponent: failed to get wing object from ResourceLoadMgr.");
            return;
        }

        GameObject wingObject = m_WingPoolObject.Object;
        wingObject.transform.SetParent(parent, false);
        m_View = s_ViewPool.Get();
        m_View.Bind(owner, m_Model, wingObject.transform);
    }

    public void Update()
    {
        m_View?.Sync();
    }

    public void Dispose()
    {
        if (m_View != null)
        {
            s_ViewPool.Release(m_View);
            m_View = null;
        }

        if (m_WingPoolObject != null)
        {
            m_WingPoolObject.Dispose();
            m_WingPoolObject = null;
        }
        m_Owner?.TryRemoveComponent<WingPresenter>();
        m_Owner = null;
    }
}
