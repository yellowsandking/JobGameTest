using UnityEngine;

/// <summary>
/// 角色在场景中的可视表现：托管实例化的模型根节点，将逻辑层位姿同步到 Transform。
/// </summary>
public class ActorPresentation
{
    readonly AddressablePoolObject m_PoolObject;
    readonly Transform m_Root;

    public ActorPresentation(AddressablePoolObject poolObject)
    {
        m_PoolObject = poolObject;
        m_Root = poolObject.Object.transform;
    }

    public Quaternion ReadWorldRotation()
    {
        return m_Root.rotation;
    }

    public void ApplyWorldPose(Vector3 worldPosition, Quaternion worldRotation)
    {
        m_Root.SetPositionAndRotation(worldPosition, worldRotation);
    }

    public Animator GetAnimatorInChildren()
    {
        return m_Root.GetComponentInChildren<Animator>();
    }

    public void Dispose()
    {
        m_PoolObject.Dispose();
    }
}
