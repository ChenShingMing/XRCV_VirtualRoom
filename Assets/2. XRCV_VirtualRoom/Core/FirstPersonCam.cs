using UnityEngine;

public class FirstPersonCam : MonoBehaviour
{
    public enum Type
    {
        Use_P,
        Use_Mouse
    }

    public Type type;

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    public bool active;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private void Start()
    {
        // 從當前旋轉的四元數初始化 yaw 和 pitch
        Vector3 angles = transform.rotation.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        // 確保 yaw 和 pitch 與 Transform 同步
        SyncRotation();

        switch (type)
        {
            case Type.Use_P:
                if (Input.GetKey(KeyCode.P))
                {
                    UpdateCameraRotation();
                }
                break;

            case Type.Use_Mouse:
                if (active)
                {
                    UpdateCameraRotation();
                }
                break;
        }
    }

    private void SyncRotation()
    {
        // 確保 yaw 和 pitch 與當前旋轉同步，並避免 Gimbal Lock 問題
        Quaternion currentRotation = transform.rotation;
        Vector3 angles = currentRotation.eulerAngles;

        yaw = angles.y;
        pitch = angles.x;
    }

    private void UpdateCameraRotation()
    {
        // 根據滑鼠輸入更新 yaw 和 pitch
        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");


        // 更新 Transform 的旋轉
        transform.rotation = Quaternion.Euler(pitch, yaw, 0.0f);
    }
}