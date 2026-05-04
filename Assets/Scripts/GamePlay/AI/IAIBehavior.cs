using UnityEngine;
using System.Collections;

public enum AIBehavoirType
{
    eIdle = 0,
    eFindPlayer = 1,
    eAttack = 2,
    eMax,
}

public enum AIPlayerBehavoirType
{
    eNone = 0,
    eAttack = 1,
    eMax,
}

public interface IAI
{
    void Init(ActorBase actor);
    void LogicUpdate();
    void Clear();
    int currentBehaviorType
    {
        get;
    }
}
public interface IAIBehavior
{
    bool Update(float deltTime);
    int aiType
    {
        get;
    }
}
