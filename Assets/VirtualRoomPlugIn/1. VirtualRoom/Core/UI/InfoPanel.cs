using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class InfoPanel : MonoBehaviour
{
    public GameObject infoPanel;
    public TMP_Text schoolName_Text;
    public TMP_Text nickName_Text;
    public GameObject leaveRoom;
    public GameObject disconneck;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //schoolName_Text.text = LicenseManager.ins == null? string.Empty : LicenseManager.ins.elementData.name;
        nickName_Text.text = PhotonNetwork.NickName;

        if (PhotonNetwork.IsConnected)
        {
            if (infoPanel != null)
            {
                infoPanel.SetActive(true);
            }
        }
        else
        {
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }
    }
}
