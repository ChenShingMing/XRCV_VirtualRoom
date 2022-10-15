using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Rename : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_InputField inputField;

    public string newName;

    private void OnEnable()
    {
        nameText.text = PhotonNetwork.NickName;
    }
}
