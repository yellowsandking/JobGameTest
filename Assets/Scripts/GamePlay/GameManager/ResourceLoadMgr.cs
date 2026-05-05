using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;

public class ResourceLoadMgr : GameLogicMgr<ResourceLoadMgr>
{
    GameObject m_GameAsset;
    GameObject m_GameObjectPool;
    GameObject m_GameEffectLayer;
    GameObject m_CameraPrefab = null;

    // 需要加载的资源列表
    List<string> m_NeedLoadResourceList = new List<string>();
    Dictionary<string, int> m_NeedLoadResourcePoolNumDic = new Dictionary<string, int>();

    Dictionary<string, AddressableObjectPool> m_GameAssetPoolDic = new Dictionary<string, AddressableObjectPool>();
    List<UniTask> m_UniTaskList = new List<UniTask>();

    public Transform gameEffectLayer
    {
        get { return m_GameEffectLayer.transform; }
    }

    public GameObject cameraPrefab
    {
        get { return m_CameraPrefab; }
    }

    public GameObject gameAsset
    {
        get { return m_GameAsset; }
    }

    public override async UniTask OnInit()
    {
        //Debug.Log("ResourceLoadMgr init");
        m_GameAsset = new GameObject("GameAsset");
        m_GameObjectPool = new GameObject("GameObjectPool");
        m_GameEffectLayer = new GameObject("GameEffectLayer");
        m_CameraPrefab = await Addr.LoadAssetAsyncTask<GameObject>("GameCamera", m_GameAsset);

        await LoadGameResource();
    }

    async UniTask LoadGameResource()
    {
        m_NeedLoadResourceList.Clear();
        m_NeedLoadResourcePoolNumDic.Clear();
        AddToNeedLoadResourceList("Player", 1);
        AddToNeedLoadResourceList("Enemy", 20);
        AddToNeedLoadResourceList("Wing", 2);

        for (int i = 0; i < m_NeedLoadResourceList.Count; ++i)
        {
            string resName = m_NeedLoadResourceList[i];
            if (string.IsNullOrEmpty(resName))
            {
                continue;
            }
            if (m_GameAssetPoolDic.ContainsKey(resName))
            {
                continue;
            }

            AddressableObjectPool addrPool = new AddressableObjectPool(resName, m_GameObjectPool, null, null);
            int warmupCount = 1;
            if (m_NeedLoadResourcePoolNumDic.ContainsKey(resName))
            {
                warmupCount = m_NeedLoadResourcePoolNumDic[resName];
            }
            m_UniTaskList.Add(addrPool.WarnUp(warmupCount));
            m_GameAssetPoolDic.Add(resName, addrPool);
            addrPool.PoolGameObject.transform.SetParent(m_GameObjectPool.transform);
            addrPool.PoolGameObject.transform.localPosition = Vector3.zero;
        }

        await UniTask.WhenAll(m_UniTaskList);
    }

    public AddressablePoolObject GetPoolObjectBySourceKey(string sourceKey, GameObject bingGo = null)
    {
        if (m_GameAssetPoolDic.ContainsKey(sourceKey))
        {
            return m_GameAssetPoolDic[sourceKey].Get();
        }
        else
        {
            Debug.LogError("GetPoolObjectBySourceKey error, sourceKey: " + sourceKey);
        }

        Debug.LogError("Pool object is not enough, sourceKey: " + sourceKey);
        return null;
    }

    void AddToNeedLoadResourceList(string addrKey, int poolCount)
    {
        if (string.IsNullOrEmpty(addrKey) || addrKey == "0")
        {
            return;
        }

        if (m_NeedLoadResourceList.Contains(addrKey))
        {
            m_NeedLoadResourcePoolNumDic[addrKey] = poolCount;
        }
        else
        {
            m_NeedLoadResourceList.Add(addrKey);
            m_NeedLoadResourcePoolNumDic[addrKey] = poolCount;
        }
    }
}
