using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class InputHandler_OpenXR : InputHandler
{
    public static InputHandler_OpenXR ins;

    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
    public LayerMask uiLayerMask;

    public InputActionAsset inputActions; // ��A�� Input Action Asset ���o��
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


        // ���o "XRController" action map ���� Secondary Index Trigger �j�w���ʧ@
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
        
          //�D���
          if (mainMenuAction.WasReleasedThisFrame())
          {
              ClassroomManager.ins.inputActionManager.TriggerMainMenu();
          }

          //�Ч�������
          if (menuAction.WasReleasedThisFrame())
          {
              ClassroomManager.ins.inputActionManager.TriggerMenu();
          }

        //�e�X��A�񴣥��I
        if (!IsPointingAtUI())
        {
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
        }

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

    private bool IsPointingAtUI()
    {
        // �ˬd XRRayInteractor �O�_���V�F UI
        if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult raycastResult))
        {
            if (raycastResult.gameObject != null && IsUILayer(raycastResult.gameObject))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsUILayer(GameObject obj)
    {
        // �ˬd����O�_�ݩ� UI �ϼh
        return (uiLayerMask == (uiLayerMask | (1 << obj.layer)));
    }
}