using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

//public class LaserScalePointer : OVRCursor
public class XRRayInteractorUICursor :MonoBehaviour
{
    public XRRayInteractor rayInteractor;      // 將你的 XRRayInteractor 拖進來
    public GameObject cursorInstance;         // 光標的實例
    private RectTransform canvasRect;          // 當前 UI 的 Canvas 矩形

    void Start()
    {
        cursorInstance.SetActive(false); // 初始狀態為不顯示
    }

    void Update()
    {
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        // 檢查是否擊中了 UI
        if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult raycastResult))
        {
            if (raycastResult.gameObject != null)
            {
                // 顯示光標並將其移動到射線擊中的 UI 位置
                cursorInstance.SetActive(true);
                Vector3 worldPosition = raycastResult.worldPosition;
                cursorInstance.transform.position = worldPosition;

                // 讓光標永遠面向相機
                FaceCamera(cursorInstance.transform);
            }
        }
        else
        {
            // 如果沒有擊中 UI，隱藏光標
            cursorInstance.SetActive(false);
        }
    }

    // 讓光標面向相機的方法
    private void FaceCamera(Transform cursorTransform)
    {
        // 使光標面向相機
        Vector3 directionToCamera = cursorTransform.position - Camera.main.transform.position;
        cursorTransform.rotation = Quaternion.LookRotation(directionToCamera);
    }
}
