using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class OpenXRCanvasRaycasterAdder : MonoBehaviour
{
    public GameObject pointer;

    private void Start()
    {
        AddXRRaycaster();
    }

    void AddXRRaycaster()
    {
        
        Canvas[] allCanvas = Resources.FindObjectsOfTypeAll<Canvas>();

        for (int i = 0; i < allCanvas.Length; i++)
        {
            if (allCanvas[i].gameObject.scene.name == null) continue;

            TrackedDeviceGraphicRaycaster raycaster = allCanvas[i].GetComponent<TrackedDeviceGraphicRaycaster>();

            if (raycaster == null)
            {
                raycaster = allCanvas[i].gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
                //raycaster.pointer = pointer;
            }
        }

        Dropdown[] allDropdown = Resources.FindObjectsOfTypeAll<Dropdown>();

        for (int i = 0; i < allDropdown.Length; i++)
        {
            if (allDropdown[i].gameObject.scene.name == null) continue;

            TrackedDeviceGraphicRaycaster raycaster = allDropdown[i].template.GetComponent<TrackedDeviceGraphicRaycaster>();

            if (raycaster == null)
            {
                raycaster = allDropdown[i].template.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
                //raycaster.pointer = pointer;
            }
        }
        
    }
}
