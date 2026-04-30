using System;
using System.Collections.Generic;
using UnityEngine;
using Addler.Runtime.Core.Preloading;
using Addler.Runtime.Core.LifetimeBinding;
using Addler.Runtime.Core.Pooling;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Collections;

public class Addr
{
    /// <summary>
    /// Instantiates game object on the scene asynchronously and adds a trigger to the instance that
    /// releases when the instance is destroyed.
    /// </summary>
    /// <returns>Instantiated GameObject on the scene.</returns>
    public static async UniTask<GameObject> InstantiateAsyncWithAutoRelease(string key, Transform parent = null,
        bool inWorldSpace = false, Action<GameObject> onCompletion = null)
    {
        var operationResult = Addressables.InstantiateAsync(key);
        await operationResult.Task;

        var instantiatedGO = operationResult.Result;
        instantiatedGO.transform.SetParent(parent, inWorldSpace);
        instantiatedGO.AddComponent<AddressableAutoRelease>();

        onCompletion?.Invoke(instantiatedGO);

        return instantiatedGO;
    }

    /// <summary>
    /// Load asset async and adds a trigger to the GameObject that
    /// releases when the GameObject is destroyed.
    /// </summary>
    public static AsyncOperationHandle<T> LoadAssetAsync<T>(string key, GameObject bindGO)
    {
        var handle = Addressables.LoadAssetAsync<T>(key);
        if (bindGO != null)
            handle.BindTo(bindGO);

        return handle;
    }

    public static UniTask<T> LoadAssetAsyncTask<T>(string key, GameObject bindGO)
    {
        var operation = Addressables.LoadAssetAsync<T>(key);

        if (bindGO != null)
            operation.BindTo(bindGO);

        var handle = operation.ToUniTask();

        return handle;
    }

    /// <summary>
    /// create AddressablesPreloader and adds a trigger to the GameObject that
    /// releases when the GameObject is destroyed.
    /// </summary>
    public static AddressablePreloader CreateAddressablePreLoader(GameObject bindGO)
    {
        return new AddressablePreloader().BindTo(bindGO);
    }

    /// <summary>
    /// create AddressablesPool and adds a trigger to the GameObject that
    /// releases when the GameObject is destroyed.
    /// </summary>
    public static AddressablePool CreateAddressablePool(string key, GameObject bindGO)
    {
        return new AddressablePool(key).BindTo(bindGO);
    }

    /// <summary>
    /// Bind the lifetime of the instance.
	/// If the bindGO is destroyed, the instance will be returned to the pool.
    /// </summary>   
    public static GameObject GetPoolObject(AddressablePool pool, Transform parent = null, bool inWorldSpace = false)
    {
        var operation = pool.Use();
        var poolObj = operation.Instance;
        operation.BindTo(poolObj);

        return poolObj;
    }

    /// <summary>
    /// 加载所有的js文件
    /// </summary>   
    public static AsyncOperationHandle<IList<TextAsset>> PreloadJS(string jsLabel)
    {
        return Addressables.LoadAssetsAsync<TextAsset>(jsLabel, null);
    }

    /// <summary>
    /// 加载Sprite
    /// </summary>   
    public static AsyncOperationHandle<Sprite> LoadTextureAssetAsync(string key, GameObject bindGO = null)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(key);
        if (bindGO != null)
            handle.BindTo(bindGO);

        return handle;
    }

    public static void ReleaseAsyncOperationHandle<TObject>(AsyncOperationHandle<TObject> op)
    {
        Addressables.Release(op);
    }

    /// <summary>
    /// 加载Scene
    /// </summary>   
    public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(string key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
    {
        var handle = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
        return handle;
    }

    /// <summary>
    /// 批量加载
    /// </summary>   
    public static AsyncOperationHandle<IList<T>> LoadAssetsAsync<T>(IEnumerable keys, GameObject bindGO)
    {
        return bindGO == null ? Addressables.LoadAssetsAsync<T>(keys, null, Addressables.MergeMode.Union) :
        Addressables.LoadAssetsAsync<T>(keys, null, Addressables.MergeMode.Union).BindTo(bindGO);
    }
}

public class AddrPoolObject
{
    private GameObject _instance;
    public GameObject gameObject => _instance;
    private GameObject _bind;
    public GameObject bindGameObject => _bind;

    public AddrPoolObject(GameObject inst, Transform parent = null, bool inWorldSpace = false)
    {
        _instance = inst;
        _bind = new GameObject();
        _instance.transform.SetParent(parent, inWorldSpace);
        _bind.transform.SetParent(parent, inWorldSpace);
    }

    // destroy bind gameobject, the instance will be returned to the pool.
    public void Destroy()
    {
        GameObject.Destroy(_bind);
        _instance = null;
        _bind = null;
    }
}

public class AddressableAutoRelease : MonoBehaviour
{
    void OnDestroy()
    {
        Addressables.ReleaseInstance(gameObject);
    }
}
