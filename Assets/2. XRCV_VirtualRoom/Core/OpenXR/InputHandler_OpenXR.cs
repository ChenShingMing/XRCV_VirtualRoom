using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class InputHandler_OpenXR : InputHandler
{
    public static InputHandler_OpenXR ins;

    public XRRayInteractor rayInteractor;
    public LayerMask uiLayerMask;

    public InputActionAsset inputActions; // 把你的 Input Action Asset 拖到這裡
    public InputActionReference leftHandPositionActionRef;
    public InputActionReference rightHandPositionActionRef;
    public InputActionReference leftHandRotationActionRef;
    public InputActionReference rightHandRotationActionRef;

    private InputAction mainMenuAction;
    private InputAction menuAction;
    private InputAction triggerAction;
    private InputAction penModeAction;


    private void Awake()
    {
        if(ins == null)
        {
            ins = this;
        }
        else
        {
            if(ins != this)
            {
                Destroy(this.gameObject);
                return;
            }

        }


        // 取得 "XRController" action map 中的 Secondary Index Trigger 綁定的動作
        //var actionMap = inputActions.FindActionMap("XRController");
        mainMenuAction = inputActions.FindActionMap("XRI LeftHand Interaction").FindAction("MainMenu");
        menuAction = inputActions.FindActionMap("XRI LeftHand Interaction").FindAction("Select");
        triggerAction = inputActions.FindActionMap("XRI RightHand Interaction").FindAction("Activate");
        penModeAction = inputActions.FindActionMap("XRI RightHand Interaction").FindAction("Select");

    }


    private void OnEnable()
    {
        mainMenuAction.Enable();
        menuAction.Enable();
        triggerAction.Enable();
        penModeAction.Enable();
    }

    private void OnDisable()
    {
        mainMenuAction.Disable();
        menuAction.Disable();
        triggerAction.Disable();
        penModeAction.Disable();

    }

    public override void InputHandle()
    {
        //教材控制器選單
        if (menuAction.WasReleasedThisFrame())
        {
            StarMapController.ins.TriggerControlCanvas();
        }

    }
}