using UnityEngine;

/// <summary>MVP — Model：角色纯逻辑状态，不含场景对象引用。</summary>
public enum ActorAnimState
{
    Idle,
    Run,
    Attack,
    Dead
}

public class ActorModel
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; } = Quaternion.identity;
    public ActorAnimState AnimState { get; set; } = ActorAnimState.Idle;
    public int AttackPresentationIntent { get; private set; }
    public PropSet PropSet { get; } = new PropSet();
    public ActorType ActorType { get; set; }

    public Vector3 Forward => Rotation * Vector3.forward;

    public void TriggerAttackPresentation()
    {
        AttackPresentationIntent++;
        AnimState = ActorAnimState.Attack;
    }
}
