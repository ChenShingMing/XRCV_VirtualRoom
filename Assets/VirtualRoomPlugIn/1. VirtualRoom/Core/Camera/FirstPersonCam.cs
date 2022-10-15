using UnityEngine;
using System.Collections;

public class FirstPersonCam : MonoBehaviour
{
    public enum Type
    {
        Use_P,
        Use_Mouse
    }

    public Type type;

    //本腳本單機時起動
    public float speedH = 2.0f;
    public float speedV = 2.0f;

    public bool active;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Update()
    {
        switch (type)
        {
            case Type.Use_P:
                if (Input.GetKey(KeyCode.P))
                {
                    yaw += speedH * Input.GetAxis("Mouse X");
                    pitch -= speedV * Input.GetAxis("Mouse Y");

                    transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                }
                break;

            case Type.Use_Mouse:

                if (active)
                {
                    yaw += speedH * Input.GetAxis("Mouse X");
                    pitch -= speedV * Input.GetAxis("Mouse Y");

                    transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

                    //Cursor.lockState = CursorLockMode.Locked;
                    //Cursor.visible = false;
                }
                else
                {
                    //Cursor.lockState = CursorLockMode.None;
                    //Cursor.visible = true;
                }

                break;
        }
    }
}