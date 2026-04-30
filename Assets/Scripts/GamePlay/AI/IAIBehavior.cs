using UnityEngine;
using System.Collections;

public enum AIBehavoirType
{
    eIdle = 0,
    eFindPlayer = 1,
    eAttack = 2,
    eMax = 3,
}

public interface IAI
{
    void Init(ActorBase actor);
    void LogicUpdate();
    void Clear();
}

public interface IAIBehavior
{
    bool Update(float deltTime);
    AIBehavoirType aiType
    {
        get;
    }
}
