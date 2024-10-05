using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignPosToController : MonoBehaviour
{
    public GameObject align;
    public Vector3 offset;
    public float lerpSpeed;

    private void OnEnable()
    {
        align.transform.position = InputHandler_OpenXR.ins.leftHandPositionActionRef.action.ReadValue<Vector3>() + offset;
    }

    // Update is called once per frame
    void Update()
    {
        AlignHandle();
    }

    void AlignHandle()
    {
        /*
        bool controllersActive = OVRInput.GetActiveController() == OVRInput.Controller.Touch ||
          OVRInput.GetActiveController() == OVRInput.Controller.LTouch ||
          OVRInput.GetActiveController() == OVRInput.Controller.RTouch;
        */

        //if (!controllersActive) return;

        Vector3 menuPosition = InputHandler_OpenXR.ins.leftHandPositionActionRef.action.ReadValue<Vector3>() + (InputHandler_OpenXR.ins.rightHandRotationActionRef.action.ReadValue<Quaternion>().normalized * offset);

        align.transform.position = Vector3.Lerp(align.transform.position, menuPosition, lerpSpeed * Time.deltaTime);
        align.transform.rotation = Quaternion.LookRotation(menuPosition - Camera.main.transform.position);
    }
    
}
