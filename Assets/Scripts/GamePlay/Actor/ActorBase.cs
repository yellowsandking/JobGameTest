using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActorBase
{
    // Œª÷√
    protected Vector3 m_Pos;
    protected float m_Speed;
    protected Transform m_SelfTF;

    public void Init(Vector3 pos, float speed, Transform tf)
    {
        m_Pos = pos;
        m_Speed = speed;
        m_SelfTF = tf;
    }
}
