using UnityEngine;

/// <summary>
/// View 层：持有 Addressable 实例对应的 <see cref="ActorPresentation"/>，负责与逻辑 Actor 绑定前的视觉资源生命周期。
/// 通常由 <see cref="ActorSpawn"/> 创建并与 <see cref="ActorBase.Init(Vector3, ActorView)"/> 绑定。
/// </summary>
public class ActorView
{
    readonly ActorPresentation m_Presentation;

    public ActorPresentation Presentation => m_Presentation;

    public ActorView(AddressablePoolObject poolObject)
    {
        m_Presentation = new ActorPresentation(poolObject);
    }

    public void Dispose()
    {
        m_Presentation.Dispose();
    }
}
