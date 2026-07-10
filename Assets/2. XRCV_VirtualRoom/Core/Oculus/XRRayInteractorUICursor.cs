using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


//public class LaserScalePointer : OVRCursor
public class XRRayInteractorUICursor :MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;      // �N�A�� XRRayInteractor ��i��
    public GameObject cursorInstance;         // ���Ъ����
    private RectTransform canvasRect;          // ���e UI �� Canvas �x��

    void Start()
    {
        cursorInstance.SetActive(false); // ��l���A�������
    }

    void Update()
    {
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        // �ˬd�O�_�����F UI
        if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult raycastResult))
        {
            if (raycastResult.gameObject != null)
            {
                // ��ܥ��ШñN�䲾�ʨ�g�u������ UI ��m
                cursorInstance.SetActive(true);
                Vector3 worldPosition = raycastResult.worldPosition;
                cursorInstance.transform.position = worldPosition;

                // �����Хû����V�۾�
                FaceCamera(cursorInstance.transform);
            }
        }
        else
        {
            // �p�G�S������ UI�A���å���
            cursorInstance.SetActive(false);
        }
    }

    // �����Э��V�۾�����k
    private void FaceCamera(Transform cursorTransform)
    {
        // �ϥ��Э��V�۾�
        Vector3 directionToCamera = cursorTransform.position - Camera.main.transform.position;
        cursorTransform.rotation = Quaternion.LookRotation(directionToCamera);
    }
}
