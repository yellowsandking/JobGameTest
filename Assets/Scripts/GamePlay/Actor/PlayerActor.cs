using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActor : ActorBase
{
    void Update()
    {
        Vector2 moveDir = InputMgr.Instance.moveDir;
        Vector3 velocity = new Vector3(moveDir.x, 0, moveDir.y);
        m_Pos += velocity * m_Speed * Time.deltaTime;
    }
}
