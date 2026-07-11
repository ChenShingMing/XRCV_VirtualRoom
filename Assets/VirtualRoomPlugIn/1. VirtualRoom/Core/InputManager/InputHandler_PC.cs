using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InputHandler_PC : InputHandler
{
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    public override Vector3 GetInputPointerOnGazeSphere()
    {
        return GazeSphere.RayHitOnSphere(_mainCamera.ScreenPointToRay(Input.mousePosition));
    }

    public override void InputHandle()
    {
        //主選單
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ClassroomManager.ins.inputActionManager.TriggerMainMenu();
        }

        //課程選單鍵
        if (Input.GetKeyDown(KeyCode.O))
        {
            ClassroomManager.ins.inputActionManager.TriggerMenu();
        }

        //轉視角
        if (Input.GetMouseButton(0))
        {
            ClassroomManager.ins.inputActionManager.TriggerFirstPersonView(true);
        }
        else
        {
            ClassroomManager.ins.inputActionManager.TriggerFirstPersonView(false);
        }

        //送出鍵，放提示點
        if (Input.GetMouseButtonDown(2))
        {
            //Debug.Log("GetMouseButtonDown");
            ClassroomManager.ins.inputActionManager.OnSubmitDownTrigger(_mainCamera.transform.rotation);
        }
        if (Input.GetMouseButton(2))
        {
            //Debug.Log("GetMouseButton");
            ClassroomManager.ins.inputActionManager.OnSubmitTrigger(_mainCamera.transform.rotation);
        }
        if (Input.GetMouseButtonUp(2))
        {
            //Debug.Log("GetMouseButtonUp");
            ClassroomManager.ins.inputActionManager.OnSubmitUpTrigger(_mainCamera.transform.rotation);
        }

        if(Input.GetMouseButtonUp(0))
        {
            ClassroomManager.ins.inputActionManager.TriggerMonitorPointerClick();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ClassroomManager.ins.inputActionManager.TriggerPenPode();
        }

    }
}
