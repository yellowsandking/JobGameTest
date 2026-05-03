using UnityEngine;

/// <summary>MVP — View 契约：由 Presenter 驱动刷新，不持有业务规则。</summary>
public interface IActorView
{
    Quaternion ReadWorldRotation();

    void SyncVisual(Vector3 worldPosition, Quaternion worldRotation, ActorAnimState animState, int attackPresentationIntent);

    Animator Animator { get; }

    GameObject VisualRoot { get; }

    void Dispose();
}
