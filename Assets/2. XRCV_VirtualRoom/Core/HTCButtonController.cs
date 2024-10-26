using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HTCButtonController : MonoBehaviour
{
    public void OpenViverseBusiness()
    {
        Application.OpenURL("viversebusiness://");
    }

    public void OpenEdu()
    {
        Application.OpenURL("EduVerse://");
    }
}
