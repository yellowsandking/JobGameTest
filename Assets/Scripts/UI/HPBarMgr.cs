using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 根据 <see cref="BattleMgr"/> 中的角色列表实例化血条模板，并按类型切换 HPPlayer / HPEnemy 子节点显示。
/// 血条实例通过 <see cref="ObjectPool{T}"/> 复用；仅在 <see cref="BattleMgr.actorList"/> 成员或顺序变化时重建条数与类型显示。
/// </summary>
public class HPBarMgr : MonoBehaviour
{
    const string HPPlayerChildName = "HPPlayer";
    const string HPEnemyChildName = "HPEnemy";

    sealed class HPBarPoolItem
    {
        public Transform Transform;
        public Image HpPlayerImage;
        public Image HpEnemyImage;
    }

    [SerializeField]
    Transform m_HPBar;

    [Tooltip("用于 WorldToScreenPoint 的相机，为空则用 Camera.main")]
    [SerializeField]
    Camera m_WorldCamera;

    [Tooltip("血条所在 Canvas；为空则从血条实例上自动查找")]
    [SerializeField]
    Canvas m_Canvas;

    [Tooltip("相对角色逻辑坐标的世界空间偏移（一般在头顶）")]
    [SerializeField]
    Vector3 m_WorldOffset = new Vector3(0f, 3f, 0f);

    readonly List<HPBarPoolItem> m_HPBarInstances = new List<HPBarPoolItem>();
    readonly List<ActorBase> m_CachedActorRefs = new List<ActorBase>();
    readonly List<HPBarPoolItem> m_AllPooledItemsForDestroy = new List<HPBarPoolItem>();

    ObjectPool<HPBarPoolItem> m_Pool;

    void Awake()
    {
        if (m_HPBar != null)
        {
            m_HPBar.gameObject.SetActive(false);
            m_Pool = new ObjectPool<HPBarPoolItem>(
                actionOnGet: OnBarGet,
                actionOnRelease: OnBarRelease,
                actionInit: OnBarInit,
                preCreateCount: 0);
        }
    }

    void Update()
    {
        if (HasActorListChanged())
        {
            RefreshHpBarsFromBattle();
        }

        UpdateHpBarScreenPositionsAndFills();
    }

    bool HasActorListChanged()
    {
        BattleMgr battle = BattleMgr.Instance;
        if (battle == null)
        {
            return m_CachedActorRefs.Count > 0;
        }

        IReadOnlyList<ActorBase> actors = battle.actorList;
        if (actors.Count != m_CachedActorRefs.Count)
        {
            return true;
        }

        for (int i = 0; i < actors.Count; i++)
        {
            if (actors[i] != m_CachedActorRefs[i])
            {
                return true;
            }
        }

        return false;
    }

    void RebuildActorCache()
    {
        m_CachedActorRefs.Clear();
        BattleMgr battle = BattleMgr.Instance;
        if (battle == null)
        {
            return;
        }

        foreach (ActorBase a in battle.actorList)
        {
            m_CachedActorRefs.Add(a);
        }
    }

    void OnBarInit(HPBarPoolItem item)
    {
        GameObject instance = Instantiate(m_HPBar.gameObject, m_HPBar.parent);
        instance.SetActive(false);
        item.Transform = instance.transform;
        item.HpPlayerImage = FindChildRecursive(item.Transform, HPPlayerChildName)?.GetComponent<Image>();
        item.HpEnemyImage = FindChildRecursive(item.Transform, HPEnemyChildName)?.GetComponent<Image>();
        m_AllPooledItemsForDestroy.Add(item);
    }

    static void OnBarGet(HPBarPoolItem item)
    {
        if (item.Transform != null)
        {
            item.Transform.gameObject.SetActive(true);
        }
    }

    static void OnBarRelease(HPBarPoolItem item)
    {
        if (item.Transform != null)
        {
            item.Transform.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 将每条血条对应的角色世界坐标转为屏幕坐标，并设置到 UI RectTransform（需在 Canvas 下）；并同步血量填充。
    /// </summary>
    void UpdateHpBarScreenPositionsAndFills()
    {
        if (m_HPBar == null || m_HPBarInstances.Count == 0)
        {
            return;
        }

        BattleMgr battle = BattleMgr.Instance;
        if (battle == null)
        {
            return;
        }

        Camera worldCam = m_WorldCamera != null ? m_WorldCamera : Camera.main;
        if (worldCam == null)
        {
            return;
        }

        Canvas canvas = m_Canvas;
        if (canvas == null)
        {
            canvas = m_HPBarInstances[0].Transform.GetComponentInParent<Canvas>();
        }

        if (canvas == null && m_HPBar != null)
        {
            canvas = m_HPBar.GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        var canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
        {
            return;
        }

        Camera uiEventCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : (canvas.worldCamera != null ? canvas.worldCamera : worldCam);

        IReadOnlyList<ActorBase> actors = battle.actorList;
        int n = Mathf.Min(actors.Count, m_HPBarInstances.Count);

        for (int i = 0; i < n; i++)
        {
            HPBarPoolItem poolItem = m_HPBarInstances[i];
            if (poolItem == null || poolItem.Transform == null)
            {
                continue;
            }

            var barRt = poolItem.Transform.GetComponent<RectTransform>();
            if (barRt == null)
            {
                continue;
            }

            ActorBase actor = actors[i];
            if (actor == null)
            {
                continue;
            }

            UpdateBarHpFill(poolItem, actor);

            Vector3 worldPos = actor.Position + m_WorldOffset;
            Vector3 screenPoint = worldCam.WorldToScreenPoint(worldPos);

            if (screenPoint.z <= 0f)
            {
                barRt.gameObject.SetActive(false);
                continue;
            }

            barRt.gameObject.SetActive(true);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPoint,
                    uiEventCam,
                    out Vector2 localPoint))
            {
                barRt.anchoredPosition = localPoint;
            }
        }
    }

    /// <summary>
    /// 读取 <see cref="BattleMgr.actorList"/>，使血条实例数量与角色一致；池化复用而非 Destroy。
    /// 每个实例下名为 HPPlayer / HPEnemy 的子物体按 <see cref="ActorType"/> 显示或隐藏（NPC 时两者皆隐藏）。
    /// </summary>
    public void RefreshHpBarsFromBattle()
    {
        if (m_HPBar == null)
        {
            Debug.LogWarning("[HPBarMgr] m_HPBar template is not assigned.");
            return;
        }

        if (m_Pool == null)
        {
            m_Pool = new ObjectPool<HPBarPoolItem>(
                actionOnGet: OnBarGet,
                actionOnRelease: OnBarRelease,
                actionInit: OnBarInit,
                preCreateCount: 0);
        }

        BattleMgr battle = BattleMgr.Instance;
        if (battle == null)
        {
            ClearSpawnedBars();
            RebuildActorCache();
            return;
        }

        IReadOnlyList<ActorBase> actors = battle.actorList;
        int need = actors.Count;

        while (m_HPBarInstances.Count > need)
        {
            int last = m_HPBarInstances.Count - 1;
            HPBarPoolItem remove = m_HPBarInstances[last];
            m_HPBarInstances.RemoveAt(last);
            if (remove != null)
            {
                m_Pool.Release(remove);
            }
        }

        while (m_HPBarInstances.Count < need)
        {
            m_HPBarInstances.Add(m_Pool.Get());
        }

        for (int i = 0; i < need; i++)
        {
            ApplyActorTypeToBar(m_HPBarInstances[i], actors[i]);
        }

        RebuildActorCache();
    }

    void ApplyActorTypeToBar(HPBarPoolItem item, ActorBase actor)
    {
        if (actor == null || item == null || item.Transform == null)
        {
            return;
        }

        ActorType type = actor.m_ActorType;

        if (item.HpPlayerImage != null)
        {
            item.HpPlayerImage.gameObject.SetActive(type == ActorType.Player);
        }

        if (item.HpEnemyImage != null)
        {
            item.HpEnemyImage.gameObject.SetActive(type == ActorType.Monster);
        }

        UpdateBarHpFill(item, actor);
    }

    static void UpdateBarHpFill(HPBarPoolItem item, ActorBase actor)
    {
        if (actor == null || item == null)
        {
            return;
        }

        float max = (float)actor.m_PropSet[PropType.HP_MAX];
        float fill = max > 0f ? actor.m_PropSet[PropType.HP_CUR] / max : 0f;

        if (item.HpPlayerImage != null)
        {
            item.HpPlayerImage.fillAmount = fill;
        }

        if (item.HpEnemyImage != null)
        {
            item.HpEnemyImage.fillAmount = fill;
        }
    }

    static Transform FindChildRecursive(Transform root, string childName)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == childName)
            {
                return t;
            }
        }

        return null;
    }

    void ClearSpawnedBars()
    {
        if (m_Pool == null)
        {
            m_HPBarInstances.Clear();
            return;
        }

        for (int i = 0; i < m_HPBarInstances.Count; i++)
        {
            HPBarPoolItem item = m_HPBarInstances[i];
            if (item != null)
            {
                m_Pool.Release(item);
            }
        }

        m_HPBarInstances.Clear();
    }

    void OnDestroy()
    {
        ClearSpawnedBars();
        m_Pool?.Clear();

        for (int i = 0; i < m_AllPooledItemsForDestroy.Count; i++)
        {
            HPBarPoolItem item = m_AllPooledItemsForDestroy[i];
            if (item != null && item.Transform != null)
            {
                Destroy(item.Transform.gameObject);
            }
        }

        m_AllPooledItemsForDestroy.Clear();
    }
}
