using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasGetMainCamera : MonoBehaviour
{
    public Canvas canvas;
    private void Awake()
    {
        canvas.worldCamera = Camera.main;
    }
}
