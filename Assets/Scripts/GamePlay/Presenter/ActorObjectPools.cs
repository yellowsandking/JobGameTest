/// <summary>
/// Actor 层统一对象池入口：内部使用 <see cref="ObjectPool{T}"/>。
/// View 仍由 Addressable 管理；回收 Presenter 请 <see cref="ActorBase.Dispose"/> 或 <see cref="ActorSpawn.Release"/>。
/// </summary>
public static class ActorObjectPools
{
    static readonly ObjectPool<ActorModel> s_ModelPool = new ObjectPool<ActorModel>(
        actionOnGet: null,
        actionOnRelease: OnReleaseModel);

    static readonly ObjectPool<PlayerActor> s_PlayerPool = new ObjectPool<PlayerActor>(
        actionOnGet: null,
        actionOnRelease: null);

    static readonly ObjectPool<MonsterActor> s_MonsterPool = new ObjectPool<MonsterActor>(
        actionOnGet: null,
        actionOnRelease: null);

    static void OnReleaseModel(ActorModel model)
    {
        model.ResetForPool();
    }

    public static ActorModel RentModel()
    {
        return s_ModelPool.Get();
    }

    public static void ReleaseModel(ActorModel model)
    {
        if (model != null)
        {
            s_ModelPool.Release(model);
        }
    }

    public static PlayerActor RentPlayer()
    {
        return s_PlayerPool.Get();
    }

    public static void ReleasePlayer(PlayerActor actor)
    {
        if (actor != null)
        {
            s_PlayerPool.Release(actor);
        }
    }

    public static MonsterActor RentMonster()
    {
        return s_MonsterPool.Get();
    }

    public static void ReleaseMonster(MonsterActor actor)
    {
        if (actor != null)
        {
            s_MonsterPool.Release(actor);
        }
    }

    public static void Clear()
    {
        s_ModelPool.Clear();
        s_PlayerPool.Clear();
        s_MonsterPool.Clear();
    }
}
