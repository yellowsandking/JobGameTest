using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Playables;
using Cinemachine;
using UnityEngine.Timeline;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Cysharp.Threading.Tasks;
using System.Linq;

public class ParamReader
{
    enum Symbol
    {
        None,
        Positive,
        Nagetive
    }

    string m_Str;
    int m_Pos;
    int m_Count = -1;

    char[] m_SplitChars;

    char[] DEFAULT_SPLIT = new char[]
    {
        ',',
        ';',
        '|',
        ':',
    };

    char[] CUSTOM_SPLIT = new char[]
    {
        ',',
    };

    public int remainCount
    {
        get
        {
            if (string.IsNullOrEmpty(m_Str))
            {
                return 0;
            }
            else
            {
                int count = 1;

                for (int i = m_Pos; i < m_Str.Length; ++i)
                {
                    char c = m_Str[i];

                    if (IsSplitChar(c))
                    {
                        ++count;
                    }
                }

                return count;
            }
        }
    }

    // 123,34234,0.23498,abc;123,34234,0.23498,abc;
    public void SetStr(string str, char splitChar = '\0')
    {
        m_Str = str;
        m_Pos = 0;
        m_Count = -1;

        if (splitChar != '\0')
        {
            m_SplitChars = CUSTOM_SPLIT;
            CUSTOM_SPLIT[0] = splitChar;
        }
        else
        {
            m_SplitChars = DEFAULT_SPLIT;
        }
    }

    public void SetStr(string str, char[] splitChars)
    {
        m_Str = str;
        m_Pos = 0;
        m_Count = -1;

        if (splitChars == null)
        {
            m_SplitChars = DEFAULT_SPLIT;
        }
        else
        {
            m_SplitChars = splitChars;
        }
    }

    public int ReadInt()
    {
        if (m_Str == null || m_Pos >= m_Str.Length)
        {
            return 0;
        }

        int nextPos = GetNextSpliterPos(m_Pos);
        int val = GetInt(m_Str, m_Pos, nextPos);

        m_Pos = nextPos + 1;

        return val;
    }

    public float ReadFloat()
    {
        if (m_Str == null || m_Pos >= m_Str.Length)
        {
            return 0;
        }

        int nextPos = GetNextSpliterPos(m_Pos);
        float val = GetFloat(m_Str, m_Pos, nextPos);

        m_Pos = nextPos + 1;

        return val;
    }

    public long ReadInt64()
    {
        if (m_Str == null || m_Pos >= m_Str.Length)
        {
            return 0;
        }

        int nextPos = GetNextSpliterPos(m_Pos);
        long val = GetInt64(m_Str, m_Pos, nextPos);

        m_Pos = nextPos + 1;

        return val;
    }

    public string ReadString()
    {
        if (m_Str == null || m_Pos >= m_Str.Length)
        {
            return string.Empty;
        }

        int nextPos = GetNextSpliterPos(m_Pos);
        string val = GetStr(m_Str, m_Pos, nextPos);

        m_Pos = nextPos + 1;

        return val;
    }

    public bool HasData()
    {
        if (string.IsNullOrEmpty(m_Str))
        {
            return false;
        }

        if (m_Pos >= m_Str.Length)
        {
            return false;
        }

        return true;
    }

    public int Count()
    {
        if (m_Count >= 0)
        {
            return m_Count;
        }

        if (string.IsNullOrEmpty(m_Str))
        {
            m_Count = 0;
            return 0;
        }

        int count = 1;

        for (int i = 0; i < m_Str.Length; ++i)
        {
            char c = m_Str[i];

            if (IsSplitChar(c))
            {
                ++count;
            }
        }

        m_Count = count;
        return count;
    }

    public bool SkipTo(int index)
    {
        if (m_Str == null || index < 0)
        {
            return false;
        }

        m_Pos = 0;

        for (int i = 0; i < index; ++i)
        {
            int nextPos = GetNextSpliterPos(m_Pos);

            if (nextPos != m_Pos && nextPos < m_Str.Length)
            {
                m_Pos = GetNextSpliterPos(m_Pos) + 1;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    int GetNextSpliterPos(int startPos)
    {
        if (m_Str == null || startPos < 0)
        {
            return 0;
        }

        for (int i = startPos; i < m_Str.Length; ++i)
        {
            char c = m_Str[i];

            if (IsSplitChar(c))
            {
                return i;
            }
        }

        return m_Str.Length;
    }

    int GetInt(string str, int startPos, int endPos)
    {
        int result = 0;

        if (str == null || startPos < 0)
        {
            return result;
        }

        startPos = MoveToNoSpacePos(str, startPos);
        if (startPos < 0)
        {
            return result;
        }

        var symbol = GetSymbol(str, startPos);
        if (symbol != Symbol.None)
        {
            ++startPos;
        }

        for (int i = startPos; i < endPos && i < str.Length; ++i)
        {
            if (str[i] == ' ')
            {
                break;
            }
            else
            {
                result = result * 10 + (str[i] - '0');
            }
        }

        if (symbol == Symbol.Nagetive)
        {
            result = -result;
        }
        return result;
    }

    float GetFloat(string str, int startPos, int endPos)
    {
        float result = 0;
        bool hasDot = false;
        float factor = 1;

        if (str == null || startPos < 0)
        {
            return result;
        }

        startPos = MoveToNoSpacePos(str, startPos);
        if (startPos < 0)
        {
            return result;
        }

        var symbol = GetSymbol(str, startPos);
        if (symbol != Symbol.None)
        {
            ++startPos;
        }

        for (int i = startPos; i < endPos && i < str.Length; ++i)
        {
            if (str[i] == ' ')
            {
                break;
            }
            else if (str[i] == '.')
            {
                hasDot = true;
            }
            else if (!hasDot)
            {
                result = result * 10 + (str[i] - '0');
            }
            else
            {
                factor = factor * 0.1f;
                result += (str[i] - '0') * factor;
            }
        }

        if (symbol == Symbol.Nagetive)
        {
            result = -result;
        }
        return result;
    }

    long GetInt64(string str, int startPos, int endPos)
    {
        long result = 0;

        if (str == null || startPos < 0)
        {
            return result;
        }

        startPos = MoveToNoSpacePos(str, startPos);
        if (startPos < 0)
        {
            return result;
        }

        var symbol = GetSymbol(str, startPos);
        if (symbol != Symbol.None)
        {
            ++startPos;
        }

        for (int i = startPos; i < endPos && i < str.Length; ++i)
        {
            if (str[i] == ' ')
            {
                break;
            }
            else
            {
                result = result * 10 + (str[i] - '0');
            }
        }

        if (symbol == Symbol.Nagetive)
        {
            result = -result;
        }
        return result;
    }

    string GetStr(string str, int startPos, int endPos)
    {
        string result = string.Empty;

        if (str == null || startPos < 0)
        {
            return result;
        }

        if (startPos >= str.Length)
        {
            return result;
        }

        if (endPos >= str.Length)
        {
            return str.Substring(startPos);
        }

        return str.Substring(startPos, endPos - startPos);
    }

    int MoveToNoSpacePos(string str, int pos)
    {
        for (int i = pos; i < str.Length; i++)
        {
            char c = str[i];
            if (c != ' ')
            {
                return i;
            }
        }

        return -1;
    }

    Symbol GetSymbol(string str, int pos)
    {
        if (str != null && pos < str.Length)
        {
            return GetSymbol(str[pos]);
        }

        return Symbol.None;
    }

    Symbol GetSymbol(char c)
    {
        if (c == '+')
        {
            return Symbol.Positive;
        }
        else if (c == '-')
        {
            return Symbol.Nagetive;
        }

        return Symbol.None;
    }

    bool IsSplitChar(char c)
    {
        if (m_SplitChars == null)
        {
            return false;
        }

        for (int i = 0; i < m_SplitChars.Length; ++i)
        {
            if (c == m_SplitChars[i])
            {
                return true;
            }
        }

        return false;
    }
}


public static class GameHelper
{
    static ParamReader m_ParamReader = new ParamReader();
    static ParamReader m_BaseParamReader = new ParamReader();
    static List<string> m_StrList = new List<string>();
    static List<Vector3> m_TempPointList = new List<Vector3>();
    static System.Text.StringBuilder m_StringBuilder = new System.Text.StringBuilder();

    public static DateTime TimestampToDateTime(float timestamp)
    {
        timestamp = timestamp < 0 ? 0 : timestamp;
        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
        dt = dt.AddSeconds(timestamp);
        return dt;
    }

    public static string GetDayTime(float nTime)
    {
        int nHour;
        int nMinute;
        int nSecond;
        nHour = (int)nTime / 3600;
        nMinute = ((int)nTime % 3600) / 60;
        nSecond = (int)nTime % 60;
        return string.Format("{0:D2}:{1:D2}:{2:D2}", nHour, nMinute, nSecond);
    }

    public static int GetNowTimeStamp()
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt32(ts.TotalSeconds);
    }

    public static string SafeFormat(this string format, params object[] args)
    {
        string ret = format;

        try
        {
            ret = string.Format(format, args);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message + ex.StackTrace);
        }

        return ret;
    }

    public static void ToListInt(this string value, List<int> resultList, string splitter = ";")
    {
        if (resultList == null)
        {
            return;
        }

        resultList.Clear();

        if (string.IsNullOrEmpty(value) || splitter == null)
        {
            return;
        }

        m_BaseParamReader.SetStr(value, splitter[0]);
        int count = m_BaseParamReader.Count();

        for (int i = 0; i < count; i++)
        {
            resultList.Add(m_BaseParamReader.ReadInt());
        }
    }

    public static void ToListFloat(this string value, List<float> result, string splitter = ";")
    {
        if (result == null)
        {
            return;
        }

        result.Clear();

        if (string.IsNullOrEmpty(value) || splitter == null)
        {
            return;
        }

        m_BaseParamReader.SetStr(value, splitter[0]);
        int count = m_BaseParamReader.Count();

        for (int i = 0; i < count; i++)
        {
            result.Add(m_BaseParamReader.ReadFloat());
        }
    }

    public static void ToListString(this string value, List<string> result, string splitter = ";", bool ignoreEmpty = false)
    {
        if (result == null)
        {
            return;
        }

        result.Clear();

        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(splitter))
        {
            return;
        }

        m_BaseParamReader.SetStr(value, splitter[0]);
        int count = m_BaseParamReader.Count();

        for (int i = 0; i < count; i++)
        {
            string str = m_BaseParamReader.ReadString();

            if (ignoreEmpty && string.IsNullOrEmpty(str))
            {
                continue;
            }

            result.Add(str);
        }
    }

    public static int ToInt(this string value)
    {
        int ret = 0;
        int.TryParse(value, out ret);

        return ret;
    }

    public static uint ToUInt(this string value)
    {
        uint ret = 0;
        uint.TryParse(value, out ret);

        return ret;
    }


    public static bool ToBool(this string value)
    {
        bool ret = false;
        if (ToInt(value) > 0)
        {
            return true;
        }
        else
        {
            bool.TryParse(value, out ret);
        }
        return ret;
    }

    public static long ToInt64(this string value)
    {
        Int64 ret = 0;
        Int64.TryParse(value, out ret);

        return ret;
    }

    public static float ToFloat(this string value)
    {
        float ret = 0;
        float.TryParse(value, out ret);

        return ret;
    }


    public static Vector2 ToVector2(this string pos, char split = ',')
    {
        m_BaseParamReader.SetStr(pos, split);
        int count = m_BaseParamReader.Count();

        float x = count > 0 ? m_BaseParamReader.ReadFloat() : 0f;
        float y = count > 1 ? m_BaseParamReader.ReadFloat() : 0f;

        Vector2 ret = new Vector2(x, y);

        return ret;
    }

    public static void ToListVector2(this string str, List<Vector2> result, string splitter = ";", string nextSplitter = ",")
    {
        if (result == null)
        {
            return;
        }

        result.Clear();

        if (string.IsNullOrEmpty(splitter))
        {
            return;
        }

        m_StrList.Clear();
        m_ParamReader.SetStr(str, splitter[0]);
        int count = m_ParamReader.Count();
        for (int i = 0; i < count; i++)
        {
            m_StrList.Add(m_ParamReader.ReadString());
        }

        for (int i = 0; i < m_StrList.Count; i++)
        {
            result.Add(ToVector2(m_StrList[i], nextSplitter[0]));
        }
    }


    public static Vector3 ToVector3(this string pos, char split = ',')
    {
        if (string.IsNullOrEmpty(pos))
        {
            return Vector3.zero;
        }

        m_BaseParamReader.SetStr(pos, split);
        int count = m_BaseParamReader.Count();

        float x = count > 0 ? m_BaseParamReader.ReadFloat() : 0f;
        float y = count > 1 ? m_BaseParamReader.ReadFloat() : 0f;
        float z = count > 2 ? m_BaseParamReader.ReadFloat() : 0f;

        Vector3 ret = new Vector3(x, y, z);

        return ret;
    }

    public static void ToListVector3(this string str, List<Vector3> result, string splitter = ";", string nextSplitter = ",")
    {
        if (result == null)
        {
            return;
        }

        result.Clear();

        if (string.IsNullOrEmpty(splitter))
        {
            return;
        }

        m_StrList.Clear();
        m_ParamReader.SetStr(str, splitter[0]);
        int count = m_ParamReader.Count();
        for (int i = 0; i < count; i++)
        {
            m_StrList.Add(m_ParamReader.ReadString());
        }

        for (int i = 0; i < m_StrList.Count; i++)
        {
            result.Add(ToVector3(m_StrList[i], nextSplitter[0]));
        }
    }

    public static Vector4 ToVector4(this string pos, char split = ',')
    {
        if (string.IsNullOrEmpty(pos))
        {
            return Vector4.zero;
        }

        m_BaseParamReader.SetStr(pos, split);
        int count = m_BaseParamReader.Count();

        float x = count > 0 ? m_BaseParamReader.ReadFloat() : 0f;
        float y = count > 1 ? m_BaseParamReader.ReadFloat() : 0f;
        float z = count > 2 ? m_BaseParamReader.ReadFloat() : 0f;
        float w = count > 3 ? m_BaseParamReader.ReadFloat() : 0f;

        Vector4 ret = new Vector4(x, y, z, w);

        return ret;
    }

    public static Color ToColor(this string value, char split = ',')
    {
        if (string.IsNullOrEmpty(value))
        {
            return Color.white;
        }

        m_BaseParamReader.SetStr(value, split);
        int count = m_BaseParamReader.Count();

        float r = count > 0 ? m_BaseParamReader.ReadInt() / 255.0f : 0f;
        float g = count > 1 ? m_BaseParamReader.ReadInt() / 255.0f : 0f;
        float b = count > 2 ? m_BaseParamReader.ReadInt() / 255.0f : 0f;
        float a = count > 3 ? m_BaseParamReader.ReadInt() / 255.0f : 1f;

        Color ret = new Color(r, g, b, a);

        return ret;
    }

    public static string ListInt2String(this List<int> list, char split = ';')
    {
        if (null == list || list.Count <= 0)
        {
            return "";
        }

        StringBuilder data = new StringBuilder();

        for (int i = 0; i < list.Count; ++i)
        {
            data.Append(list[i]);

            if (i != list.Count - 1)
            {
                data.Append(split);
            }
        }

        return data.ToString();
    }

    public static string ListString2String(this List<string> list, char split = ';')
    {
        if (null == list || list.Count <= 0)
        {
            return "";
        }

        StringBuilder data = new StringBuilder();

        for (int i = 0; i < list.Count; ++i)
        {
            data.Append(list[i]);

            if (i != list.Count - 1)
            {
                data.Append(split);
            }
        }

        return data.ToString();
    }

    public static int[] ToIntArray(this string data, string split = "-")
    {
        m_BaseParamReader.SetStr(data, split[0]);
        int count = m_BaseParamReader.Count();
        int[] ret = new int[count];

        for (int i = 0; i < count; i++)
        {
            ret[i] = m_BaseParamReader.ReadInt();
        }

        return ret;
    }

    public static long[] ToInt64Array(this string data, string split = "-")
    {
        m_BaseParamReader.SetStr(data, split[0]);
        int count = m_BaseParamReader.Count();
        long[] ret = new long[count];

        for (int i = 0; i < count; i++)
        {
            ret[i] = m_BaseParamReader.ReadInt64();
        }

        return ret;
    }

    /// <summary>
    /// 向上取整
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CeilToInt(this float value)
    {
        try
        {
            return Mathf.CeilToInt(value);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return 0;
        }
    }

    /// <summary>
    /// 向下取整
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int FloorToInt(this float value)
    {
        try
        {
            return Mathf.FloorToInt(value);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return 0;
        }
    }

    public static T AddComponent<T>(GameObject go) where T : Component
    {
        T t = go.GetComponent<T>();
        if (t != null)
        {
            return t;
        }
        return go.AddComponent<T>();
    }

    public static string DictionaryToJson(Dictionary<string, string> dic)
    {
        string s = "{";
        if (dic != null)
        {
            bool needRemove = false;
            foreach (var kv in dic)
            {
                s = s + "\"" + kv.Key + "\":\"" + kv.Value + "\",";
                needRemove = true;
            }
            if (needRemove)
            {
                s = s.Remove(s.Length - 1);
            }
        }
        s = s + "}";
        return s;
    }

    /// <summary>
    /// 获取贝兹曲线路径
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="controlPoint"></param>
    /// <param name="endPoint"></param>
    /// <param name="segmentNum"></param>
    /// <param name="pathList"></param>
    public static void GetBeizerPathList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int segmentNum, List<Vector3> pathList)
    {
        if (pathList == null)
        {
            return;
        }
        pathList.Clear();
        pathList.Add(startPoint);

        for (int i = 0; i < segmentNum; i++)
        {
            float t = i / (float)segmentNum;
            Vector3 point = CalculateCubicBezierPoint(t, startPoint, controlPoint, endPoint);
            pathList.Add(point);
            //GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //obj.transform.position = point;
        }
        pathList.Add(endPoint);
    }

    /// <summary>
    /// 有一个控制点的贝兹曲线
    /// </summary>
    /// <param name="t"></param>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }

    /// <summary>
    /// 实例化一个新对象并设置它的父对象
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static GameObject AddChild(GameObject prefab, Transform parent)
    {
        GameObject go = GameObject.Instantiate(prefab, parent) as GameObject;
        if (go != null && parent != null)
        {
            Transform t = go.transform;
            t.SetParent(parent);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
        return go;
    }

    public static void SetParent(GameObject go, Transform parent)
    {
        if (go != null && parent != null)
        {
            Transform t = go.transform;
            t.SetParent(parent);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 判断是否在UI对象上
    /// </summary>
    /// <returns></returns>
    public static bool IsPointerOverGameObject()
    {
        //#if UNITY_EDITOR
        return EventSystem.current.IsPointerOverGameObject();
        //#else
        //        return Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        //#endif
    }

    public static void SetAllChildrenLayer(GameObject go, string layerName)
    {
        Transform[] trans = go.GetComponentsInChildren<Transform>(true);
        int layer = LayerMask.NameToLayer(layerName);
        go.layer = layer;
        for (int i = 0; i < trans.Length; ++i)
        {
            trans[i].gameObject.layer = layer;
        }
    }

    public static T ToEnum<T>(string str)
    {
        try
        {
            return (T)Enum.Parse(typeof(T), str);
        }
        catch
        {
            return default(T);
        }
    }

    public static T ToEnum<T>(int val)
    {
        try
        {
            return (T)Enum.ToObject(typeof(T), val);
        }
        catch
        {
            return default(T);
        }
    }

    public static int GetQualityValue(int quality)
    {
        float value = Mathf.Log(quality, 2);
        return (int)value;
    }

    public static string GetPercentageText(float value)
    {
        string text = string.Format("{0}%", (int)(value / 100.0f));
        return text;
    }

    public static float GetAniClipLength(Animator animator, string animationName)
    {
        if (animator == null)
        {
            return 0;
        }

        float timeDuration = 0;
        AnimationClip animationClip = GetAniClipByName(animator, animationName);
        if (animationClip != null)
        {
            timeDuration = animationClip.length;
        }

        return timeDuration;
    }

    public static AnimationClip GetAniClipByName(Animator animator, string clipName)
    {
        if (animator == null || String.IsNullOrEmpty(clipName) || animator.runtimeAnimatorController == null)
        {
            return null;
        }

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        if (clips == null || clips.Length <= 0)
        {
            return null;
        }

        AnimationClip result = null;
        AnimationClip clip;
        for (int i = 0, len = clips.Length; i < len; ++i)
        {
            clip = clips[i];
            if (clip != null && clip.name.Contains(clipName))
            {
                result = clip;
            }
            //Debug.LogError(string.Format("clip.name: {0}, time : {1}", clip.name, clip.length));
        }
        return result;

    }

    public static void GetRealMovePath(List<Vector3> orgMovePath, List<Vector3> realMovePath, int distance = 13, int segmentNum = 15)
    {
        realMovePath.Clear();
        if (orgMovePath.Count <= 2)
        {
            realMovePath.AddRange(orgMovePath);
            return;
        }
        int pathCount = orgMovePath.Count;
        for (int i = 0; i < orgMovePath.Count; ++i)
        {
            if (i == 0 || i == (pathCount - 1))
            {
                //第一个点或者最后一个点
                realMovePath.Add(orgMovePath[i]);
            }
            else
            {
                Vector3 curPoint = orgMovePath[i];
                Vector3 lastPoint = orgMovePath[i - 1];
                Vector3 nextPoint = orgMovePath[i + 1];
                curPoint.y = 0;
                lastPoint.y = 0;
                nextPoint.y = 0;
                if ((int)Vector3.Angle(curPoint - lastPoint, nextPoint - curPoint) == 90)
                {
                    // 弯角
                    Vector3 startPos = (orgMovePath[i] + orgMovePath[i - 1]) / 2 + (orgMovePath[i] - orgMovePath[i - 1]).normalized * distance;
                    Vector3 endPos = (orgMovePath[i] + orgMovePath[i + 1]) / 2 + (orgMovePath[i] - orgMovePath[i + 1]).normalized * distance;
                    //Vector3 controlPos = (startPos + endPos) / 2;
                    //Vector3 dir = (orgMovePath[i] - controlPos).normalized * 14;
                    //controlPos += dir;
                    Vector3 controlPos = orgMovePath[i];
                    GetMoveRotatePath(startPos, controlPos, endPos, m_TempPointList, segmentNum);
                    realMovePath.AddRange(m_TempPointList);
                }
                else
                {
                    realMovePath.Add(orgMovePath[i]);
                }
            }
        }
    }

    static void GetMoveRotatePath(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, List<Vector3> pointList, int segmentNum)
    {
        GetBeizerPathList(startPoint, controlPoint, endPoint, segmentNum, pointList);
    }

    public static void SetLocalPosition(Transform tf, float x, float y, float z)
    {
        Vector3 localPosition = tf.localPosition;
        localPosition.x = x;
        localPosition.y = y;
        localPosition.z = z;
        tf.localPosition = localPosition;
    }

    public static void SetLocalEulerAngles(Transform tf, float x, float y, float z)
    {
        Vector3 localEulerAngles = tf.localEulerAngles;
        localEulerAngles.x = x;
        localEulerAngles.y = y;
        localEulerAngles.z = z;
        tf.localEulerAngles = localEulerAngles;
    }

    public static void SetLocalScale(Transform tf, float x, float y, float z)
    {
        Vector3 localScale = tf.localScale;
        localScale.x = x;
        localScale.y = y;
        localScale.z = z;
        tf.localScale = localScale;
    }

    public static bool IsNumeric(string str)
    {
        if (str == null || str.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < str.Length; i++)
        {
            if (i == 0 && str[i] == '-')
            {
                // 跳过负号
                continue;
            }
            if (!Char.IsNumber(str[i]))
            {
                return false;
            }
        }
        return true;
    }

    public static void PlayTimeline(PlayableDirector pd, Animator mech, Animator targetMech)
    {
        foreach (var bind in pd.playableAsset.outputs)
        {
            //Debug.LogError(bind.streamName);
            if (bind.streamName.Contains("Cinemachine Track"))
            {
                pd.SetGenericBinding(bind.sourceObject, Camera.main.gameObject.GetComponent<CinemachineBrain>());
            }
            else if (bind.streamName.Contains("PlayerModel Animation Track"))
            {
                pd.SetGenericBinding(bind.sourceObject, mech);
            }
            else if (bind.streamName.Contains("TargetModel Animation Track"))
            {
                pd.SetGenericBinding(bind.sourceObject, targetMech);
            }
            else if (bind.streamName.Contains("_VFX"))
            {
                ControlTrack controlTrack = bind.sourceObject as ControlTrack;
                if (controlTrack == null)
                {
                    continue;
                }
                foreach (var timelineClip in controlTrack.GetClips())
                {
                    ControlPlayableAsset clip = timelineClip.asset as ControlPlayableAsset;
                    //Debug.LogError(clip.name);
                    //bool isvalid;
                    //UnityEngine.Object obj = pd.GetReferenceValue(clip.sourceGameObject.exposedName, out isvalid);
                    if (clip != null)
                    {
                        PropertyName propertyName = clip.sourceGameObject.exposedName;
                        Transform bindPosTF = GetBindPos(mech.gameObject.transform, bind.streamName);
                        pd.SetReferenceValue(propertyName, bindPosTF.gameObject);
                    }
                }
            }
        }

        pd.Play(pd.playableAsset, DirectorWrapMode.Hold);
    }

    public static void PlayTemplateTimeline(PlayableDirector pd, Animator mech, Animator targetMech)
    {
        foreach (var bind in pd.playableAsset.outputs)
        {
            //Debug.LogError(bind.streamName);
            if (bind.streamName.Contains("_VFX"))
            {
                ControlTrack controlTrack = bind.sourceObject as ControlTrack;
                if (controlTrack == null)
                {
                    continue;
                }
                foreach (var timelineClip in controlTrack.GetClips())
                {
                    ControlPlayableAsset clip = timelineClip.asset as ControlPlayableAsset;
                    //Debug.LogError(clip.name);
                    //bool isvalid;
                    //UnityEngine.Object obj = pd.GetReferenceValue(clip.sourceGameObject.exposedName, out isvalid);
                    if (clip != null)
                    {
                        PropertyName propertyName = clip.sourceGameObject.exposedName;
                        Transform bindPosTF = GetBindPos(mech.gameObject.transform, bind.streamName);
                        pd.SetReferenceValue(propertyName, bindPosTF.gameObject);
                    }
                }
            }
        }

        pd.Play(pd.playableAsset, DirectorWrapMode.Hold);
    }

    public static void HideObjectChildren(Transform objTF)
    {
        int childCount = objTF.childCount;
        for (int i = 0; i < childCount; ++i)
        {
            Transform tf = objTF.GetChild(i);
            if (tf != null)
            {
                tf.gameObject.SetActive(false);
            }
        }
    }

    public static Transform GetBindPos(Transform tf, string bindPosName)
    {
        if (tf == null)
        {
            return null;
        }

        Transform[] childs = tf.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < childs.Length; ++i)
        {
            if (childs[i].name == bindPosName)
            {
                return childs[i];
            }
        }

        if (!string.IsNullOrEmpty(bindPosName))
            Debug.LogError($"{tf.name} transform not contain {bindPosName} child node");

        for (int i = 0; i < childs.Length; ++i)
        {
            if (childs[i].name == "Vp_01")
            {
                return childs[i];
            }
        }

        return tf;
    }

    public static void ResetTransform(Transform tf)
    {
        tf.localPosition = Vector3.zero;
        tf.localRotation = Quaternion.identity;
        tf.localScale = Vector3.one;
    }

    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public static T DeepClone<T>(T t)
    {
        T clone = default(T);
        using (Stream stream = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(stream, t);
                stream.Seek(0, SeekOrigin.Begin);
                clone = (T)formatter.Deserialize(stream);
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to serialize. Reason: " + e.Message);
            }
        }
        return clone;
    }

    public static async void LoadImage(string resKey, Image image)
    {
        var handle = Addr.LoadTextureAssetAsync(resKey);
        await handle.Task;
        image.sprite = handle.Result;
    }

    //public static async void LoadRawImage(string resKey, RawImage image)
    //{
    //    var handle = Addr.LoadAssetAsync<Texture>(resKey, null);
    //    await handle.Task;
    //    image.texture = handle.Result;
    //}

    public static async UniTask LoadImageAsync(string resKey, Image image)
    {
        var handle = Addr.LoadTextureAssetAsync(resKey);
        await handle.Task;
        image.sprite = handle.Result;
    }

    //    public static string GetFilePath(string FolderName, string FileName = "")
    //    {
    //        string filePath;
    //#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    //        // mac
    //        filePath = Path.Combine(Application.streamingAssetsPath, ("data/" + FolderName));

    //        if (FileName != "")
    //            filePath = Path.Combine(filePath, FileName);
    //#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    //        // windows
    //        filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

    //        if (FileName != "")
    //            filePath = Path.Combine(filePath, FileName);
    //#elif UNITY_ANDROID
    //        // android
    //        filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

    //        if(FileName != "")
    //            filePath = Path.Combine(filePath, FileName);
    //#elif UNITY_IOS
    //        // ios
    //        filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

    //        if(FileName != "")
    //            filePath = Path.Combine(filePath, FileName);
    //#endif
    //        return filePath;
    //    }

    public static Vector4 GetTipsLocalPosition(RectTransform pointerRect, RectTransform tipsParent)
    {
        var camera = GetUICamera();
        var screenPos = UnityEngine.RectTransformUtility.WorldToScreenPoint(camera, pointerRect.position);
        UnityEngine.Vector2 localPos;
        UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(tipsParent, screenPos, camera, out localPos);

        if (localPos.x >= 0 && localPos.y >= 0)
        {
            var pos = localPos;
            pos.x -= (pointerRect.rect.width * 0.5f);
            pos.y += (pointerRect.rect.height * 0.5f);
            return new Vector4(1, 1, pos.x, pos.y);
        }

        else if (localPos.x < 0 && localPos.y >= 0)
        {
            var pos = localPos;
            pos.x += (pointerRect.rect.width * 0.5f);
            pos.y += (pointerRect.rect.height * 0.5f);
            return new Vector4(0, 1, pos.x, pos.y);
        }
        else if (localPos.x < 0 && localPos.y < 0)
        {
            var pos = localPos;
            pos.x += (pointerRect.rect.width * 0.5f);
            pos.y -= (pointerRect.rect.height * 0.5f);
            return new Vector4(0, 0, pos.x, pos.y);
        }
        else
        {
            var pos = localPos;
            pos.x -= (pointerRect.rect.width * 0.5f);
            pos.y -= (pointerRect.rect.height * 0.5f);
            return new Vector4(1, 0, pos.x, pos.y);
        }
    }

    public static Vector4 GetTipsLocalPosition2(RectTransform pointerRect, RectTransform tipsParent)
    {
        var camera = GetUICamera();
        var screenPos = UnityEngine.RectTransformUtility.WorldToScreenPoint(camera, pointerRect.position);
        UnityEngine.Vector2 localPos;
        UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(tipsParent, screenPos, camera, out localPos);

        if (localPos.x >= 0 && localPos.y >= 0)
        {
            var pos = localPos;
            return new Vector4(1, 1, pos.x, pos.y);
        }

        else if (localPos.x < 0 && localPos.y >= 0)
        {
            var pos = localPos;
            return new Vector4(0, 1, pos.x, pos.y);
        }
        else if (localPos.x < 0 && localPos.y < 0)
        {
            var pos = localPos;
            return new Vector4(0, 0, pos.x, pos.y);
        }
        else
        {
            var pos = localPos;
            return new Vector4(1, 0, pos.x, pos.y);
        }
    }

    public static Vector4 GetTipsLocalPosition3(Transform pointer, RectTransform tipsParent, Canvas mainCanvas)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, pointer.position);
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(tipsParent, screenPos, mainCanvas.worldCamera, out localPos);

        if (localPos.x >= 0 && localPos.y >= 0)
        {
            var pos = localPos;
            return new Vector4(1, 1, pos.x, pos.y);
        }

        else if (localPos.x < 0 && localPos.y >= 0)
        {
            var pos = localPos;
            return new Vector4(0, 1, pos.x, pos.y);
        }
        else if (localPos.x < 0 && localPos.y < 0)
        {
            var pos = localPos;
            return new Vector4(0, 0, pos.x, pos.y);
        }
        else
        {
            var pos = localPos;
            return new Vector4(1, 0, pos.x, pos.y);
        }
    }

    private static Camera m_UICamera;
    public static Camera GetUICamera()
    {
        if (m_UICamera != null) return m_UICamera;

        Camera[] sceneCameras = GameObject.FindObjectsOfType<Camera>();

        for (int i = 0; i < sceneCameras.Length; i++)
        {
            if (sceneCameras[i].gameObject.activeInHierarchy && sceneCameras[i].enabled)
            {
                m_UICamera = sceneCameras[i];
                if (m_UICamera.tag == "UICamera")
                    break;
            }
        }

        return m_UICamera;
    }

    public static void GetWindowsResolutions(ref List<Resolution> resolutions)
    {
        foreach (var resolution in Screen.resolutions)
        {
            Debug.Log($"resolution {resolution.width}x{resolution.height}");
            if (RatioCheck(resolution) && resolution.width >= 1280 && resolutions.Where((r) => r.width == resolution.width && r.height == resolution.height).ToList().Count == 0)
            {
                Debug.Log($"add resolution {resolution.width}x{resolution.height}");
                resolutions.Add(resolution);
            }
        }

        resolutions.Sort((a, b) =>
        {
            if (a.width == b.width)
                return -a.height.CompareTo(b.height);

            return -a.width.CompareTo(b.width);
        });
    }

    public static bool RatioCheck(Resolution resolution)
    {
        if (resolution.width > 2560) return false;

        return resolution.width / 16 == resolution.height / 9 || resolution.width / 16 == resolution.height / 10;
    }

    public static Transform GetBindPosByName(Transform tf, string bindPosName)
    {
        if (tf == null)
        {
            return null;
        }

        Transform[] childs = tf.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < childs.Length; ++i)
        {
            if (childs[i].name == bindPosName)
            {
                return childs[i];
            }
        }
        if (!string.IsNullOrEmpty(bindPosName))
            Debug.LogError($"{tf.name} transform not contain {bindPosName} child node");
        return tf;
    }

    public static Transform GetTransformByName(Transform tf, string name)
    {
        if (tf == null)
        {
            return null;
        }

        Transform[] childs = tf.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < childs.Length; ++i)
        {
            if (childs[i].name == name)
            {
                return childs[i];
            }
        }

        return null;
    }

    static List<Transform> m_TransformList = new List<Transform>();
    public static List<Transform> GetTransformListByName(Transform tf, string name)
    {
        if (tf == null)
        {
            return null;
        }

        m_TransformList.Clear();
        Transform[] childs = tf.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < childs.Length; ++i)
        {
            if (childs[i].name == name)
            {
                m_TransformList.Add(childs[i]);
            }
        }

        return m_TransformList;
    }

    public static Transform GetTransformByContainName(Transform tf, string name)
    {
        if (tf == null)
        {
            return null;
        }

        Transform[] childs = tf.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < childs.Length; ++i)
        {
            if (childs[i].name.Contains(name))
            {
                return childs[i];
            }
        }

        return null;
    }

    public static void GetMinuteAndSecondByTime(float nTime, out int minute, out int second)
    {
        int nHour;
        int nMinute;
        int nSecond;
        nHour = (int)nTime / 3600;
        nMinute = ((int)nTime % 3600) / 60;
        nMinute += nHour * 60;
        nSecond = (int)nTime % 60;
        minute = nMinute;
        second = nSecond;
    }


    /// <summary>
    /// 删除对象所有子节点
    /// </summary>
    /// <param name="go"></param>
    public static void ClearChild(Transform go)
    {
        if (go == null)
        {
            return;
        }
        for (int i = go.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(go.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 隐藏对象所有子节点
    /// </summary>
    /// <param name="go"></param>
    public static void HideChild(Transform go)
    {
        if (go == null)
        {
            return;
        }
        for (int i = go.childCount - 1; i >= 0; i--)
        {
            go.GetChild(i).gameObject.SetActive(false);
        }
    }

    public static bool CheckPointIsInCamera(Vector3 pos)
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(pos);
        Debug.LogError("view pos: " + viewPos.ToString());
        if (viewPos.x <= 0 || viewPos.y <= 0 || viewPos.x >= 1 || viewPos.y >= 1)
        {
            return false;
        }

        return true;
    }

    public static string StringHeadAndEndEllip(string str, int headLen, int endLen)
    {
        if (string.IsNullOrEmpty(str) || str.Length < headLen + endLen) return str;

        var head = str[..headLen];
        var end = str.Substring(str.Length - endLen, endLen);

        return head + "......" + end;
    }

    /// <summary>
    /// 获取带颜色的字符串(etc: "00FF00")
    /// </summary>
    /// <param name="colorStr"></param>
    /// <returns></returns>
    public static string GetColorTextByColorString(string colorStr, string text)
    {
        return string.Format("<color=#{0}>{1}</color>", colorStr, text);
    }
}

public class EventTriggerListener : UnityEngine.EventSystems.EventTrigger
{
    public delegate void VoidDelegate(GameObject go);
    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    public VoidDelegate onSelect;
    public VoidDelegate onUpdateSelect;

    static public EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
        if (listener == null) listener = go.AddComponent<EventTriggerListener>();
        return listener;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (onClick != null) onClick(gameObject);
        }
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (onDown != null) onDown(gameObject);
        }
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(gameObject);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (onUp != null) onUp(gameObject);
        }
    }
    public override void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(gameObject);
    }
    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
    }
}

