using UnityEngine;

/// <summary>
/// View wrapper for the player's pooled wing object. Instances are reused by <see cref="ObjectPool{T}"/>.
/// </summary>
public class WingView
{
    WingModel m_Model;
    PlayerActor m_Owner;
    Transform m_Transform;

    public void Bind(PlayerActor owner, WingModel model, Transform wingTransform)
    {
        m_Owner = owner;
        m_Model = model;
        m_Transform = wingTransform;
        Sync();
    }

    public void Sync()
    {
        if (m_Model == null || m_Transform == null)
        {
            return;
        }

        m_Transform.gameObject.SetActive(m_Model.IsVisible);
        m_Transform.localPosition = m_Model.LocalPosition;
        m_Transform.localRotation = m_Model.LocalRotation;
        m_Transform.localScale = m_Model.LocalScale;
    }

    public void Unbind()
    {
        m_Owner = null;
        m_Model = null;
        m_Transform = null;
    }
}
