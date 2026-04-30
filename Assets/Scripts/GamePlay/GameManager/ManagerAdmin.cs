using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class ManagerAdmin : Singleton<ManagerAdmin>
{
    List<IGameMgr> m_MgrList = new List<IGameMgr>();
    List<IGameMgr> m_UpdateMgrList = new List<IGameMgr>();
    List<IGameMgr> m_LateUpdateMgrList = new List<IGameMgr>();
    bool m_IsInit = false;

    public bool IsInit => m_IsInit;
    public GameObject adminObj
    {
        get { return this.gameObject; }
    }

    public async void Init()
    {
        if (m_IsInit)
        {
            return;
        }

        gameObject.hideFlags = HideFlags.None;
        gameObject.name = "ManagerAdmin";
        DontDestroyOnLoad(gameObject);
        await RegistManagers();
        m_IsInit = true;
    }

    async UniTask RegistManagers()
    {
        await UniTask.NextFrame();
        // 注册各类逻辑管理器
        await RegisterLogicMgr(ResourceLoadMgr.Instance);
        await RegisterLogicMgr(BattleMgr.Instance);
        await RegisterLogicMgr(CameraMgr.Instance);
        await RegisterLogicMgr(InputMgr.Instance);
    }

    public void Update()
    {
        for (int i = 0; i < m_UpdateMgrList.Count; ++i)
        {
            m_UpdateMgrList[i].OnUpdate();
        }
    }

    public void LateUpdate()
    {
        for (int i = 0; i < m_LateUpdateMgrList.Count; ++i)
        {
            m_LateUpdateMgrList[i].LateUpdate();
        }
    }

    public async UniTask RegisterLogicMgr(IGameMgr mgr)
    {
        if (null == mgr)
        {
            return;
        }

        if (m_MgrList.Contains(mgr))
        {
            return;
        }

        // 加入列表
        m_MgrList.Add(mgr);
        Type type = mgr.GetType();

        // 将覆盖过的OnUpdate加入列表
        MethodInfo updateMethod = type.GetMethod("OnUpdate");
        if (updateMethod != null && updateMethod != updateMethod.GetBaseDefinition())
        {
            m_UpdateMgrList.Add(mgr);
            //Debug.Log(string.Format("{0}.OnUpdate", mgr.ToString()));
        }

        MethodInfo lateUpdateMethod = type.GetMethod("LateUpdate");
        if (lateUpdateMethod != null && lateUpdateMethod != lateUpdateMethod.GetBaseDefinition())
        {
            m_LateUpdateMgrList.Add(mgr);
            //Debug.Log(string.Format("{0}.LateUpdate", mgr.ToString()));
        }

        // 初始化
        await mgr.OnInit();
    }

    void OnApplicationPause(bool pause)
    {

    }

    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
    }

    private static bool m_Focus = true;
    public static bool Focus { get { return m_Focus; } }
    void OnApplicationFocus(bool focus)
    {
        m_Focus = focus;
    }

    //    public static void QuitGame()
    //    {
    //#if UNITY_EDITOR
    //        UnityEditor.EditorApplication.ExitPlaymode();
    //#else
    //        Application.Quit();
    //#endif
    //    }
}
