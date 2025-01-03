using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InputHandler_PC : InputHandler
{
    public FirstPersonCam cam;
    private void Awake()
    {
        
    }

    public override void InputHandle()
    {
        //課程選單鍵
        if (Input.GetKeyDown(KeyCode.O))
        {
            TMControllerBase.instance.TriggerControlCanvas();
            //StarMapController.ins.TriggerControlCanvas();
        }

        //轉視角
        cam.active = Input.GetMouseButton(0);
    }
}
