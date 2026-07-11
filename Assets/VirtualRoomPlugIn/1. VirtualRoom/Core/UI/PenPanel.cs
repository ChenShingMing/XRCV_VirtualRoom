using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PenPanel : MonoBehaviour
{
    public GameObject penPanel;
    public GameObject penTip;

    private float _tipTimer;
    private const float TIP_INTERVAL = 0.033f; // ~30 Hz

    void FixedUpdate()
    {
        bool isMasterInRoom = ClassroomManager.ins != null
            && PhotonNetwork.InRoom
            && Player.localPlayer != null
            && Player.localPlayer.isMaster;

        penPanel.SetActive(isMasterInRoom && ClassroomManager.ins.isPenMode);

        if (isMasterInRoom)
        {
            penTip.SetActive(true);
            _tipTimer += Time.fixedDeltaTime;
            if (_tipTimer >= TIP_INTERVAL)
            {
                _tipTimer = 0f;
                penTip.transform.position = ClassroomManager.ins.inputActionManager.GetInputPointerOnGazeSphere();
            }
        }
        else
        {
            penTip.SetActive(false);
        }
    }
}
