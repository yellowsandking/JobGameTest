using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;

public enum PropType
{
    None = 0,
    HP_MAX = 1,      // 最大生命值
    HP_CUR = 2,      // 当前生命值
    ATT = 3,         // 攻击力
    MOVE_SPEED = 4,   // 移动速度
    ROTATE_SPEED = 5, // 旋转速度
    Max,
}

public class PropSet
{
    private const int m_Length = (int)PropType.Max;
    private float[] m_Data = new float[m_Length];

    public int length
    {
        get { return m_Length; }
    }

    public PropSet()
    {
        Reset();
    }

    public PropSet(PropSet other)
    {
        for (int i = 0; i < m_Length; ++i)
        {
            m_Data[i] = other.m_Data[i];
        }
    }

    // 空属性集
    public static PropSet Empty
    {
        get { return new PropSet(); }
    }

    public void Reset()
    {
        for (int i = 0; i < m_Length; ++i)
        {
            m_Data[i] = 0f;
        }
    }

    public float this[PropType index]
    {
        set { this[(int)index] = value; }
        get { return this[(int)index]; }
    }

    public float this[int i]
    {
        set
        {
            if (i < 0 || i >= m_Length)
            {
                return;
            }

            m_Data[i] = value;
        }

        get
        {
            if (i < 0 || i >= m_Length)
            {
                return 0f;
            }

            return m_Data[i];
        }
    }

    public static PropSet operator +(PropSet propSet1, PropSet propSet2)
    {
        PropSet result = new PropSet();

        for (int i = 0; i < m_Length; ++i)
        {
            result.m_Data[i] = propSet1.m_Data[i] + propSet2.m_Data[i];
        }

        return result;
    }

    public static PropSet operator -(PropSet propSet1, PropSet propSet2)
    {
        PropSet result = new PropSet();

        for (int i = 0; i < m_Length; ++i)
        {
            result.m_Data[i] = propSet1.m_Data[i] - propSet2.m_Data[i];
        }

        return result;
    }

    public static PropSet operator <(PropSet propSet1, PropSet propSet2)
    {
        propSet1.AddPropSet(propSet2);
        return propSet1;
    }

    public static PropSet operator >(PropSet propSet1, PropSet propSet2)
    {
        propSet1.AddPropSet(propSet2);
        return propSet1;
    }

    public static PropSet operator *(PropSet propSet1, float factor)
    {
        for (int i = 0; i < m_Length; ++i)
        {
            propSet1.m_Data[i] = propSet1.m_Data[i] * factor;
        }

        return propSet1;
    }

    public static PropSet operator /(PropSet propSet1, float factor)
    {
        if (factor <= 0f)
        {
            return propSet1;
        }
        for (int i = 0; i < m_Length; ++i)
        {
            propSet1.m_Data[i] = propSet1.m_Data[i] / factor;
        }

        return propSet1;
    }

    public void AddProp(PropType index, float value)
    {
        this[index] += value;
    }

    public void AddPropSet(PropSet propSet)
    {
        if (null == propSet)
        {
            return;
        }

        for (int i = 0; i < m_Length; ++i)
        {
            m_Data[i] += propSet.m_Data[i];
        }
    }

    public void AddPropSetWithCompare(PropSet propSet)
    {
        if (null == propSet)
        {
            return;
        }

        for (int i = 0; i < m_Length; ++i)
        {
            m_Data[i] = Mathf.Max(m_Data[i], propSet.m_Data[i]);
        }
    }

    public string GetPropInfo()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < m_Length; ++i)
        {
            sb.Append(((PropType)i).ToString());
            sb.Append(":");
            sb.Append(m_Data[i].ToString());
            sb.Append(";");
        }
        return sb.ToString();
    }
}
