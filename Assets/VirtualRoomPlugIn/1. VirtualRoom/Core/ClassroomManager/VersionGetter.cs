using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VersionGetter : MonoBehaviour
{
    public TMP_Text text;
    public string defaultText;

    private bool _versionSet;

    private void Update()
    {
        if (_versionSet) return;
        if (ClassroomManager.ins != null)
        {
            text.text = defaultText + " " + ClassroomManager.ins.version;
            _versionSet = true;
        }
    }
}
