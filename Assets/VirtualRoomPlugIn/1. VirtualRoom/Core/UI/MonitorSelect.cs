using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonitorSelect : MonoBehaviour
{
    public TMP_Text no_Text;
    public TMP_Text targetName_Text;

    public int no;
    public string targetName;

    private void FixedUpdate()
    {
        no_Text.text = no.ToString();
        targetName_Text.text = targetName;
    }
}
