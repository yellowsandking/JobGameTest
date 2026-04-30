using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BattleMgr : GameLogicMgr<BattleMgr>
{
    public override UniTask OnInit()
    {
        Debug.Log("BattleMgr init");

        return UniTask.CompletedTask;
    }

    public override void OnUpdate()
    {
    }
}
