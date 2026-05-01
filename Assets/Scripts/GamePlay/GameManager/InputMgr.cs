using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class InputMgr : GameLogicMgr<InputMgr>
{
    public Vector2 moveDir { get; set; }
    public void MoveDir(Vector2 dir)
    {
        moveDir = dir;
    }

    public override UniTask OnInit()
    {
        Debug.Log("InputMgr init");
        return UniTask.CompletedTask;
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            BattleMgr.Instance.mainPlayer.Animator.PlayAnimation(ActorAnimState.Attack);
        }
    }
}

