using UnityEngine;

/// <summary>
/// MVP — View 容器：装配具体 <see cref="ActorPresentation"/>，对外可同时作为 <see cref="IActorView"/> 使用。
/// 由 <see cref="ActorSpawn"/> 创建，再由 Presenter（<see cref="ActorBase"/>）绑定 Model。
/// </summary>
public class ActorView
{
    readonly ActorPresentation m_Presentation;

    /// <summary>具体表现实现（Animator / Transform）。</summary>
    public ActorPresentation Presentation => m_Presentation;

    /// <summary>View 接口，供 Presenter 按契约刷新。</summary>
    public IActorView ViewContract => m_Presentation;

    public ActorView(AddressablePoolObject poolObject)
    {
        m_Presentation = new ActorPresentation(poolObject);
    }

    public void Dispose()
    {
        m_Presentation.Dispose();
    }
}
