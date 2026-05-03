using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 根据 <see cref="BattleMgr"/> 中的角色列表实例化血条模板，并按类型切换 HPPlayer / HPEnemy 子节点显示。
/// </summary>
public class HPBarMgr : MonoBehaviour
{
    const string HPPlayerChildName = "HPPlayer";
    const string HPEnemyChildName = "HPEnemy";

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

    readonly List<Transform> m_HPBarInstances = new List<Transform>();

    void Awake()
    {
        if (m_HPBar != null)
        {
            m_HPBar.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        RefreshHpBarsFromBattle();
        UpdateHpBarScreenPositions();
    }

    /// <summary>
    /// 将每条血条对应的角色世界坐标转为屏幕坐标，并设置到 UI RectTransform（需在 Canvas 下）。
    /// </summary>
    void UpdateHpBarScreenPositions()
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
            canvas = m_HPBarInstances[0].GetComponentInParent<Canvas>();
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
            Transform inst = m_HPBarInstances[i];
            if (inst == null)
            {
                continue;
            }

            var barRt = inst.GetComponent<RectTransform>();
            if (barRt == null)
            {
                continue;
            }

            ActorBase actor = actors[i];
            if (actor == null)
            {
                continue;
            }

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
    /// 读取 <see cref="BattleMgr.actorList"/>，使血条实例数量与角色一致；
    /// 每个实例下名为 HPPlayer / HPEnemy 的子物体按 <see cref="ActorType"/> 显示或隐藏（NPC 时两者皆隐藏）。
    /// </summary>
    public void RefreshHpBarsFromBattle()
    {
        if (m_HPBar == null)
        {
            Debug.LogWarning("[HPBarMgr] m_HPBar template is not assigned.");
            return;
        }

        BattleMgr battle = BattleMgr.Instance;
        if (battle == null)
        {
            ClearSpawnedBars();
            return;
        }

        IReadOnlyList<ActorBase> actors = battle.actorList;
        int need = actors.Count;

        while (m_HPBarInstances.Count > need)
        {
            int last = m_HPBarInstances.Count - 1;
            Transform remove = m_HPBarInstances[last];
            m_HPBarInstances.RemoveAt(last);
            if (remove != null)
            {
                Destroy(remove.gameObject);
            }
        }

        Transform parent = m_HPBar.parent;
        while (m_HPBarInstances.Count < need)
        {
            GameObject instance = Instantiate(m_HPBar.gameObject, parent);
            instance.SetActive(true);
            m_HPBarInstances.Add(instance.transform);
        }

        for (int i = 0; i < need; i++)
        {
            ApplyActorTypeToBar(m_HPBarInstances[i], actors[i]);
        }
    }

    void ApplyActorTypeToBar(Transform barRoot, ActorBase actor)
    {
        if (actor == null || barRoot == null)
        {
            return;
        }

        Transform hpPlayer = FindChildRecursive(barRoot, HPPlayerChildName);
        Transform hpEnemy = FindChildRecursive(barRoot, HPEnemyChildName);

        ActorType type = actor.m_ActorType;

        if (hpPlayer != null)
        {
            hpPlayer.gameObject.SetActive(type == ActorType.Player);
            hpPlayer.GetComponent<Image>().fillAmount = actor.m_PropSet[PropType.HP_CUR] / (float)actor.m_PropSet[PropType.HP_MAX];
        }

        if (hpEnemy != null)
        {
            hpEnemy.gameObject.SetActive(type == ActorType.Monster);
            hpEnemy.GetComponent<Image>().fillAmount = actor.m_PropSet[PropType.HP_CUR] / (float)actor.m_PropSet[PropType.HP_MAX];
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
        for (int i = 0; i < m_HPBarInstances.Count; i++)
        {
            if (m_HPBarInstances[i] != null)
            {
                Destroy(m_HPBarInstances[i].gameObject);
            }
        }

        m_HPBarInstances.Clear();
    }

    void OnDestroy()
    {
        ClearSpawnedBars();
    }
}
