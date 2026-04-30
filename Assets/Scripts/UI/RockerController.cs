using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class RockerController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            yaoGanBGPos, eventData.position, eventData.enterEventCamera, out outPos
            );
        float S = Vector2.Distance(Vector2.zero, outPos);
        Debug.Log("S: " + S);
        Debug.Log("R: " + R);
        if (S > R)
        {
            outPos = outPos.normalized * R;
            Debug.Log("outPos: " + outPos);
        }
        yaoGanLight.transform.up = outPos.normalized;
        yaoGanPos.localPosition = outPos;
        InputMgr.Instance.MoveDir(outPos.normalized);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform, eventData.pressPosition, eventData.enterEventCamera, out outPos
            );
        yaoGanBGPos.localPosition = outPos;
        outPos = Vector2.zero;
    }
    bool dragOver;
    public void OnEnable()
    {
        dragOver = true;
    }
    bool isSetactive;
    public void OnDisable()
    {
        outPos = Vector2.zero;
        yaoGanPos.localPosition = outPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        outPos = Vector2.zero;
        yaoGanPos.localPosition = outPos;
        if (Screen.width > Screen.height)
        {
            yaoGanBGPos.anchoredPosition = new Vector2(290, 215);
        }
        else
        {
            yaoGanBGPos.anchoredPosition = new Vector2(547, 244);
        }
        yaoGanLight.localRotation = Quaternion.identity;
        InputMgr.Instance.MoveDir(outPos.normalized);
    }
    private RectTransform yaoGanPos;
    private RectTransform yaoGanBGPos;
    private RectTransform yaoGanLight;
    public float R;//半径
    public Vector2 outPos;
    // Start is called before the first frame update
    void Start()
    {
        yaoGanPos = transform.GetChild(0).GetChild(0) as RectTransform;
        yaoGanBGPos = transform.GetChild(0) as RectTransform;
        yaoGanLight = transform.GetChild(0).GetChild(1) as RectTransform;
    }
}
