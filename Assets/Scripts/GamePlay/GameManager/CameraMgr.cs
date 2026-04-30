using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;
using Cinemachine;
using Cysharp.Threading.Tasks;

public class CameraMgr : GameLogicMgr<CameraMgr>
{
    Camera m_Main_Camera;
    Transform m_CameraTarget = null;
    CinemachineVirtualCamera m_VirtualCamera = null;
    Vector3 m_MovePos = Vector3.zero;
    CinemachineTransposer m_VCBody;
    Vector3 m_CameraMainPos = new Vector3(536, 0, 502);

    public float m_DefaultFov = 0;
    public float m_MoveSpeed = 100;
    bool m_useEdgeScrolling = false;
    float m_eulerAnglesY = -155;
    public float m_MinFollowOffsetY = 120;
    public float m_MaxFollowOffsetY = 600;
    float m_FollowOffsetRate = 0;
    float m_FollowOffsetDefaultY = 0;
    public Vector4 m_MoveBound = new Vector4(-9999, 9999, -9999, 9999);

    float m_FovMax = 40;
    float m_FovMin = 15;
    public float m_FovSpeed = 10.0f;
    Vector3 followOffsetResult = Vector3.zero;
    Vector3 m_UpDownPreCameraPos = Vector3.zero;
    Vector3 m_LastMousePos = Vector3.zero;
    public Vector2 m_UpDownAngle = new Vector3(-30, 30);
    bool m_LockCamera = false;

    public CinemachineVirtualCamera virtualCamera
    {
        get { return m_VirtualCamera; }
    }

    public bool lockCamera
    {
        get { return m_LockCamera; }
        set { m_LockCamera = value; }
    }

    public Vector3 cameraTargetPos
    {
        get { return m_CameraTarget.position; }
    }

    public Vector3 cameraTargetEuler
    {
        get { return m_CameraTarget.localEulerAngles; }
    }

    public float cameraDistance
    {
        get { return m_VCBody.m_FollowOffset.y; }
    }

    public override UniTask OnInit()
    {
        Debug.Log("CameraMgr init");
        m_Main_Camera = Camera.main;
        LoadCameraAsset();
        return UniTask.CompletedTask;
    }

    void LoadCameraAsset()
    {
        m_CameraTarget = new GameObject("CameraTarget").transform;
        GameObject virCamera = GameObject.Instantiate(ResourceLoadMgr.Instance.cameraPrefab);
        m_VirtualCamera = virCamera.GetComponent<CinemachineVirtualCamera>();
        m_VirtualCamera.LookAt = m_CameraTarget;
        m_VirtualCamera.Follow = m_CameraTarget;
        m_VCBody = m_VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        //m_VirtualCamera.transform.localPosition = m_CameraMainPos;
        m_FollowOffsetRate = m_VCBody.m_FollowOffset.z / m_VCBody.m_FollowOffset.y;
        m_FollowOffsetDefaultY = m_VCBody.m_FollowOffset.y;

        //m_CameraTarget.transform.position = m_CameraMainPos;
        m_DefaultFov = m_VirtualCamera.m_Lens.FieldOfView;
        //m_CameraTarget.eulerAngles = new Vector3(0, m_eulerAnglesY, 0);
    }

    public override void OnUpdate()
    {
        if (m_LockCamera)
        {
            return;
        }

        m_CameraTarget.position = BattleMgr.Instance.mainPlayer.m_SelfTF.position;
        //CameraRotate();
    }

    public float m_RotateSpeed = 90;
    float xRotation = 0;
    bool m_IsRotation = false;

    void CameraRotate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            m_IsRotation = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            m_IsRotation = false;
        }
        if (!m_IsRotation)
        {
            return;
        }
        float mouseX = Input.GetAxis("Mouse X") * m_RotateSpeed * Time.deltaTime;
        GameHelper.SetLocalEulerAngles(m_CameraTarget, m_CameraTarget.localEulerAngles.x, m_CameraTarget.localEulerAngles.y + mouseX, m_CameraTarget.localEulerAngles.z);
    }

    public void SetCameraTargetPosition(Vector3 pos)
    {
        if (m_CameraTarget != null)
        {
            m_CameraTarget.position = pos;
        }
    }

    public void ChangeFollowTarget(Transform target)
    {
        m_VirtualCamera.Follow = null;
        m_VirtualCamera.LookAt = target;
    }

    public void ResetTarget()
    {
        m_VirtualCamera.Follow = m_CameraTarget;
        m_VirtualCamera.LookAt = m_CameraTarget;
    }

    public void SetCameraTargetEuler(Vector3 euler)
    {
        if (m_CameraTarget != null)
        {
            m_CameraTarget.localEulerAngles = euler;
            xRotation = euler.x;
        }
    }

    public void SetCameraFov(float fov)
    {
        m_VirtualCamera.m_Lens.FieldOfView = fov;
    }

    public void ResetCameraOffset(float y)
    {
        followOffsetResult.y = y;
        followOffsetResult.z = followOffsetResult.y * m_FollowOffsetRate;
        followOffsetResult.x = m_VCBody.m_FollowOffset.x;
        m_VCBody.m_FollowOffset = followOffsetResult;
    }

}
