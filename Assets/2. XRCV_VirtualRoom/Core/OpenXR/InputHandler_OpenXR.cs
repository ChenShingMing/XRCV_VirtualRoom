using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class InputHandler_OpenXR : InputHandler
{
    public static InputHandler_OpenXR ins;

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
        
          //主選單
          if (mainMenuAction.WasReleasedThisFrame())
          {
              ClassroomManager.ins.inputActionManager.TriggerMainMenu();
          }

          //教材控制器選單
          if (menuAction.WasReleasedThisFrame())
          {
              ClassroomManager.ins.inputActionManager.TriggerMenu();
          }

          //送出鍵，放提示點
          //if (!LaserScalePointer.isHitTarget)
          //{
          if (triggerAction.WasPressedThisFrame())
          {
              //Debug.Log("GetMouseButtonDown");
              ClassroomManager.ins.inputActionManager.OnSubmitDownTrigger(rightHandRotationActionRef.action.ReadValue<Quaternion>());
          }
          if (triggerAction.IsPressed())
          {
              //Debug.Log("GetMouseButton");
              ClassroomManager.ins.inputActionManager.OnSubmitTrigger(rightHandRotationActionRef.action.ReadValue<Quaternion>());
          }
          if (triggerAction.WasReleasedThisFrame())
          {
              //Debug.Log("GetMouseButtonUp");
              ClassroomManager.ins.inputActionManager.OnSubmitUpTrigger(rightHandRotationActionRef.action.ReadValue<Quaternion>());
          }
          //}

          if (penModeAction.IsPressed())
          {
              ClassroomManager.ins.inputActionManager.SetPenPode(true);
          }
          else
          {
              ClassroomManager.ins.inputActionManager.SetPenPode(false);
          }

      

    }

    public Quaternion GetControllerRotation()
    {
        return rightHandRotationActionRef.action.ReadValue<Quaternion>();
    }

    public override Vector3 GetInputPointerOnGazeSphere()
    {
        return GazeSphere.RayHitOnSphere(new Ray(Camera.main.transform.position,
                                    GetControllerRotation() * Vector3.forward));
    }
}