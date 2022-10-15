using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MenuUITip : MonoBehaviour
{
    public Transform targetTrans;
    public AlignPosToController alignPosToController;
    public GameObject menuTip;

    // Update is called once per frame
    void Update()
    {
        Handle();
    }

    void Handle()
    {
        bool controllersActive = true;


        if (controllersActive
            && PhotonNetwork.InRoom
            && Player.localPlayer != null)
        {
            alignPosToController.targetTrans = targetTrans;
            menuTip.SetActive(!ClassroomManager.ins.mainUICanvas_Current.activeSelf);
        }
        else
        {
            menuTip.SetActive(false);
        }
    }
}

