using System;
using Cysharp.Threading.Tasks;

public interface IGameMgr
{
    UniTask OnInit();
    void OnUpdate();
    void LateUpdate();
}

public abstract class GameLogicMgr<T> : SimpleSingleton<T>, IGameMgr where T : new()
{
    public virtual UniTask OnInit()
    {
        return UniTask.CompletedTask;
    }
    public virtual void OnUpdate() { }
    public virtual void LateUpdate() { }
}
