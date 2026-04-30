using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class CameraProperty : Singleton<CameraProperty>
{
    [Label("摄像机镜头移动速度")]
    [OnValueChanged("OnCameraValueChanged")]
    public int m_CameraMoveSpeed = 100;

    [Label("摄像机镜头缩放速度")]
    [OnValueChanged("OnCameraValueChanged")]
    public int m_CameraOffsetSpeed = 1;

    [Label("摄像机镜头缩放最小偏移值")]
    [OnValueChanged("OnCameraValueChanged")]
    public int m_MinCameraOffsetY = 100;

    [Label("摄像机镜头缩放最大偏移值")]
    [OnValueChanged("OnCameraValueChanged")]
    public int m_MaxCameraOffsetY = 600;

    [Label("摄像机镜头左移动边界")]
    [OnValueChanged("OnCameraMoveValueChanged")]
    public int m_MoveLeftLimit = -100;

    [Label("摄像机镜头右移动边界")]
    [OnValueChanged("OnCameraMoveValueChanged")]
    public int m_MoveRightLimit = 100;

    [Label("摄像机镜头下移动边界")]
    [OnValueChanged("OnCameraMoveValueChanged")]
    public int m_MoveDownLimit = -100;

    [Label("摄像机镜头上移动边界")]
    [OnValueChanged("OnCameraMoveValueChanged")]
    public int m_MoveUpLimit = 100;

    [Label("摄像机镜头默认缩放距离")]
    public int m_CameraDefaultOffsetY = 300;

    [Label("摄像机镜头俯仰角范围")]
    [OnValueChanged("OnCameraValueChanged")]
    public Vector2 m_UpDownAngle = new Vector2(-30, 30);

    [Label("摄像机镜头旋转速度")]
    [OnValueChanged("OnCameraValueChanged")]
    public float m_CameraRotateSpeed = 90;

    void OnCameraValueChanged()
    {
        CameraMgr.Instance.m_MinFollowOffsetY = m_MinCameraOffsetY;
        CameraMgr.Instance.m_MaxFollowOffsetY = m_MaxCameraOffsetY;
        CameraMgr.Instance.m_FovSpeed = m_CameraOffsetSpeed;
        CameraMgr.Instance.m_MoveSpeed = m_CameraMoveSpeed;
        CameraMgr.Instance.m_RotateSpeed = m_CameraRotateSpeed;
        CameraMgr.Instance.m_UpDownAngle = m_UpDownAngle;
    }

    void OnCameraMoveValueChanged()
    {
        CameraMgr.Instance.m_MoveBound = new Vector4(m_MoveLeftLimit, m_MoveRightLimit, m_MoveDownLimit, m_MoveUpLimit);
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        //m_MainCamera = Camera.main.transform;
        OnCameraValueChanged();
        OnCameraMoveValueChanged();
        //Debug.LogError("使用配置: " + transform.name);
    }
}

