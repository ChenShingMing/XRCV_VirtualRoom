using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LicenseCreater : MonoBehaviour
{
    public string license;

    [Button]
    public void CreateLicense()
    {
        string allChar = "ABCDEFGHIJKLMNOPURSTUVWXYZ1234567890";
        license = string.Empty;

        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < 5; i++)
            {
                license += allChar[Random.Range(0, allChar.Length)];
            }

            if(j != 4)
            {
                license += "-";
            }
            
        }
    }
}
