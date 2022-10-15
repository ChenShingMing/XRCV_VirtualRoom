using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler_OVR : InputHandler
{
    public static OVRCameraRig m_CameraRig;

    private void Start()
    {
        InputHandler_OVR.m_CameraRig = FindObjectOfType<OVRCameraRig>();
    }


    public static Quaternion GetControllerRotation()
    {
        return InputHandler_OVR.m_CameraRig.rightControllerAnchor.rotation;
    }

    public override void InputHandle()
    {
        //主選單
        if (OVRInput.GetUp(OVRInput.Button.Start))
        {
            ClassroomManager.ins.inputActionManager.TriggerMainMenu();
        }

        //教材控制器選單
        if (OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger))
        {
            ClassroomManager.ins.inputActionManager.TriggerMenu();
        }

        //送出鍵，放提示點
        if (!LaserScalePointer.isHitTarget)
        {
            if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                //Debug.Log("GetMouseButtonDown");
                ClassroomManager.ins.inputActionManager.OnSubmitDownTrigger(InputHandler_OVR.m_CameraRig.rightControllerAnchor.rotation);
            }
            if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                //Debug.Log("GetMouseButton");
                ClassroomManager.ins.inputActionManager.OnSubmitTrigger(InputHandler_OVR.m_CameraRig.rightControllerAnchor.rotation);
            }
            if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            {
                //Debug.Log("GetMouseButtonUp");
                ClassroomManager.ins.inputActionManager.OnSubmitUpTrigger(InputHandler_OVR.m_CameraRig.rightControllerAnchor.rotation);
            }
        }

        if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger))
        {
            ClassroomManager.ins.inputActionManager.SetPenPode(true);
        }
        else
        {
            ClassroomManager.ins.inputActionManager.SetPenPode(false);
        }
    }

    public override Vector3 GetInputPointerOnGazeSphere()
    {
        return GazeSphere.RayHitOnSphere(new Ray(Camera.main.transform.position,
                                    GetControllerRotation() * Vector3.forward));
    }
}
