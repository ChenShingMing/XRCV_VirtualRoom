using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second.")]
    public float rotationSpeed = 30f;

    [Tooltip("Set true for clockwise rotation, false for counterclockwise.")]
    public bool clockwise = true;

    void Update()
    {
        // 計算旋轉方向
        float direction = clockwise ? -1 : 1;

        // 按照本地Y軸進行旋轉
        transform.Rotate(0, direction * rotationSpeed * Time.deltaTime, 0, Space.Self);
    }
}
