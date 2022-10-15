using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PenPanel : MonoBehaviour
{
    public GameObject penPanel;
    public GameObject penTip;


    void FixedUpdate()
    {
        if (ClassroomManager.ins != null
            && ClassroomManager.ins.isPenMode
            && PhotonNetwork.InRoom
            && Player.localPlayer != null
            && Player.localPlayer.isMaster)
        {
            penPanel.SetActive(true);
        }
        else
        {
            penPanel.SetActive(false);
        }


        if (ClassroomManager.ins != null
            && PhotonNetwork.InRoom
            && Player.localPlayer != null
            && Player.localPlayer.isMaster)
        {
            penTip.SetActive(true);
            penTip.transform.position = ClassroomManager.ins.inputActionManager.GetInputPointerOnGazeSphere();
        }
        else
        {
            penTip.SetActive(false);
        }

    }
}
