using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class InfoPanel : MonoBehaviourPunCallbacks
{
    public GameObject infoPanel;
    public TMP_Text schoolName_Text;
    public TMP_Text nickName_Text;
    public GameObject leaveRoom;
    public GameObject disconneck;

    private void OnEnable()
    {
        base.OnEnable();
        Refresh();
    }

    public void Refresh()
    {
        schoolName_Text.text = ClassroomManager.ins.licenseInformation.schoolName;
        nickName_Text.text = PhotonNetwork.NickName;
        if (infoPanel != null)
            infoPanel.SetActive(PhotonNetwork.IsConnected);
    }

    public override void OnConnectedToMaster()
    {
        Refresh();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Refresh();
    }
}
