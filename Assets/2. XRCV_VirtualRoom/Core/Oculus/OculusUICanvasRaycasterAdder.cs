using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OculusUICanvasRaycasterAdder : MonoBehaviour
{
    public GameObject pointer;

    private void Start()
    {
        if (XRSettings.isDeviceActive)
        {
            AddOVRRaycaster();
        }

        AddOVRRaycaster();
    }

    void AddOVRRaycaster()
    {
        Canvas[] allCanvas = Resources.FindObjectsOfTypeAll<Canvas>();

        for (int i = 0; i < allCanvas.Length; i++)
        {
            if (allCanvas[i].gameObject.scene.name == null) continue;

            OVRRaycaster raycaster = allCanvas[i].GetComponent<OVRRaycaster>();

            if (raycaster == null)
            {
                raycaster = allCanvas[i].gameObject.AddComponent<OVRRaycaster>();
                raycaster.pointer = pointer;
            }
        }

        Dropdown[] allDropdown = Resources.FindObjectsOfTypeAll<Dropdown>();

        for (int i = 0; i < allDropdown.Length; i++)
        {
            if (allDropdown[i].gameObject.scene.name == null) continue;

            OVRRaycaster raycaster = allDropdown[i].template.GetComponent<OVRRaycaster>();

            if (raycaster == null)
            {
                raycaster = allDropdown[i].template.gameObject.AddComponent<OVRRaycaster>();
                raycaster.pointer = pointer;
            }
        }
    }
}
