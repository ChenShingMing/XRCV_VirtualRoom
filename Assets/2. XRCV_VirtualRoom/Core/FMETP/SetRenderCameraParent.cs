using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRenderCameraParent : MonoBehaviour
{
    public Camera renderCamera;

    private void Awake()
    {
        renderCamera.transform.SetParent(Camera.main.transform);
        renderCamera.transform.localPosition = new Vector3();
        renderCamera.transform.localRotation = new Quaternion();
    }
}
