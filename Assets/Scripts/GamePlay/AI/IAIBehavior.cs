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
    eAttack = 0,
    eMax,
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
    int aiType
    {
        get;
    }
}
