using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;

public class MenuUITip : MonoBehaviour
{
    
    //public OVRInput.Controller targetPos;
    public AlignPosToController alignPosToController;
    public GameObject menuTip;
    public GameObject controllerTip;

    // Update is called once per frame
    void Update()
    {
        Handle();
    }

    void Handle()
    {
        bool controllersActive = XRSettings.isDeviceActive;

        if (controllersActive
            && PhotonNetwork.InRoom
            && Player.localPlayer != null)
        {
            controllerTip.SetActive(false);

            //alignPosToController.targetPos = targetPos;
            menuTip.SetActive(!ClassroomManager.ins.mainUICanvas_Current.activeSelf);
        }
        else
        {
            menuTip.SetActive(false);
            controllerTip.SetActive(true);
        }
    }

}

