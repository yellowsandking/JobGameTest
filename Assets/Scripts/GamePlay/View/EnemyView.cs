using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>敌人 Prefab 上的 View，挂在与模型同步移动的节点（通常为根节点）。</summary>
public class EnemyView : ActorBaseView
{
    public async UniTask WaitForDeadAnim()
    {
        await UniTask.Delay(4000);
    }
}
