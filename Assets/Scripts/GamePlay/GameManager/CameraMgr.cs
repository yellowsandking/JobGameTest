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
        ResetCameraOffset(CameraProperty.Instance.m_CameraDefaultOffsetY);
        m_DefaultFov = m_VirtualCamera.m_Lens.FieldOfView;
        //m_CameraTarget.eulerAngles = new Vector3(0, m_eulerAnglesY, 0);
    }

    public override void OnUpdate()
    {
        if (m_LockCamera)
        {
            return;
        }
        CameraMove();
        CameraUpDown();
        CameraRotate();
    }

    Vector3 m_DragStartPos = Vector3.zero;
    void CameraMove()
    {
        Vector3 inputDir = Vector3.zero;
        if (Input.GetMouseButtonDown(2))
        {
            m_LastMousePos = Input.mousePosition;
            m_LastMousePos.y *= 1.3f;
            m_DragStartPos = m_CameraTarget.position;
            return;
        }
        else if (Input.GetMouseButton(2))
        {
            DragScreen();
            return;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            m_LastMousePos = Vector3.zero;
            return;
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
                inputDir.z = 1f;

            if (Input.GetKey(KeyCode.S))
                inputDir.z = -1f;

            if (Input.GetKey(KeyCode.D))
                inputDir.x = 1f;

            if (Input.GetKey(KeyCode.A))
                inputDir.x = -1f;
        }

        if (m_useEdgeScrolling)
        {
            if (Input.mousePosition.x > Screen.width || Input.mousePosition.x < 0 || Input.mousePosition.y > Screen.height || Input.mousePosition.y < 0)
                return;

            int edgeScrollSize = 30;
            if (Input.mousePosition.x < edgeScrollSize)
            {
                inputDir.x = -1f;
            }
            if (Input.mousePosition.y < edgeScrollSize)
            {
                inputDir.z = -1f;
            }
            if (Input.mousePosition.x > Screen.width - edgeScrollSize)
            {
                inputDir.x = 1f;
            }
            if (Input.mousePosition.y > Screen.height - edgeScrollSize)
            {
                inputDir.z = 1f;
            }
        }

        if (inputDir == Vector3.zero)
            return;

        var move = (m_CameraTarget.forward * inputDir.z + m_CameraTarget.right * inputDir.x) * m_MoveSpeed * Time.deltaTime;
        m_MovePos.x = Mathf.Clamp(m_CameraTarget.position.x + move.x, m_MoveBound.x, m_MoveBound.y);
        m_MovePos.y = m_CameraTarget.position.y;
        m_MovePos.z = Mathf.Clamp(m_CameraTarget.position.z + move.z, m_MoveBound.z, m_MoveBound.w);
        // Debug.LogWarning($"m_CinemachineVirtualCamera 1 position {m_CinemachineVirtualCamera.transform.position}");
        m_CameraTarget.position = m_MovePos;
        m_VirtualCamera.InternalUpdateCameraState(Vector3.up, Time.deltaTime);
        // Debug.LogWarning($"m_CinemachineVirtualCamera 2 position {m_CinemachineVirtualCamera.transform.position}");
    }

    void DragScreen()
    {
        var screenPos = m_Main_Camera.WorldToScreenPoint(m_DragStartPos);
        m_LastMousePos.z = screenPos.z;
        var currPoint = Input.mousePosition;
        currPoint.y *= 1.3f;
        currPoint.z = screenPos.z;
        var lastWorldPos = m_Main_Camera.ScreenToWorldPoint(m_LastMousePos);
        var currWorldPos = m_Main_Camera.ScreenToWorldPoint(currPoint);
        var move = lastWorldPos - currWorldPos;

        m_MovePos.x = Mathf.Clamp(m_DragStartPos.x + move.x, m_MoveBound.x, m_MoveBound.y);
        m_MovePos.y = m_DragStartPos.y;
        m_MovePos.z = Mathf.Clamp(m_DragStartPos.z + move.z, m_MoveBound.z, m_MoveBound.w);

        m_CameraTarget.position = m_MovePos;
    }


    void CameraUpDown()
    {
        //获取虚拟按键(鼠标中轴滚轮)
        float mouseCenter = Input.GetAxis("Mouse ScrollWheel");

        //鼠标滑动中键滚轮,实现摄像机的镜头放大和缩放
        //mouseCenter < 0 = 负数 往后滑动,缩放镜头
        if (mouseCenter < 0)
        {
            //滑动限制
            if (m_VCBody.m_FollowOffset.y < m_MaxFollowOffsetY)
            {
                var dir = m_Main_Camera.transform.forward * -1;
                if (m_UpDownPreCameraPos != Vector3.zero)
                {
                    dir = (m_VirtualCamera.transform.position - m_UpDownPreCameraPos).normalized;
                    dir = (dir.x > 0 && m_Main_Camera.transform.forward.x > 0) || (dir.y > 0 && m_Main_Camera.transform.forward.y > 0) || (dir.z > 0 && m_Main_Camera.transform.forward.z > 0) ? dir * -1 : dir;
                }
                var dt = 1 + (m_FovSpeed * Time.deltaTime);
                followOffsetResult.y = Mathf.Min(m_VCBody.m_FollowOffset.y * dt, m_MaxFollowOffsetY);
                followOffsetResult.z = followOffsetResult.y * m_FollowOffsetRate;
                followOffsetResult.x = m_VCBody.m_FollowOffset.x;
                m_VCBody.m_FollowOffset = followOffsetResult;
                m_UpDownPreCameraPos = m_VirtualCamera.transform.position;
                m_VirtualCamera.InternalUpdateCameraState(Vector3.up, Time.deltaTime);
            }
            //mouseCenter >0 = 正数 往前滑动,放大镜头
        }
        else if (mouseCenter > 0)
        {
            if (m_Main_Camera.transform.position == m_UpDownPreCameraPos) return;
            //滑动限制
            if (m_VCBody.m_FollowOffset.y > m_MinFollowOffsetY)
            {
                var dir = m_Main_Camera.transform.forward;
                if (m_UpDownPreCameraPos != Vector3.zero)
                {
                    dir = (m_VirtualCamera.transform.position - m_UpDownPreCameraPos).normalized;
                    dir = (dir.x > 0 && m_Main_Camera.transform.forward.x < 0) || (dir.y > 0 && m_Main_Camera.transform.forward.y < 0) || (dir.z > 0 && m_Main_Camera.transform.forward.z < 0) ? dir * -1 : dir;
                }
                var dt = 1 + (m_FovSpeed * Time.deltaTime);
                followOffsetResult.y = Mathf.Max(m_VCBody.m_FollowOffset.y / dt, m_MinFollowOffsetY);
                followOffsetResult.z = followOffsetResult.y * m_FollowOffsetRate;
                followOffsetResult.x = m_VCBody.m_FollowOffset.x;
                m_VCBody.m_FollowOffset = followOffsetResult;
                m_UpDownPreCameraPos = m_VirtualCamera.transform.position;
                m_VirtualCamera.InternalUpdateCameraState(Vector3.up, Time.deltaTime);
            }
        }
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
        float mouseY = Input.GetAxis("Mouse Y") * m_RotateSpeed * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, m_UpDownAngle.x, m_UpDownAngle.y);
        GameHelper.SetLocalEulerAngles(m_CameraTarget, xRotation, m_CameraTarget.localEulerAngles.y + mouseX, m_CameraTarget.localEulerAngles.z);
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
