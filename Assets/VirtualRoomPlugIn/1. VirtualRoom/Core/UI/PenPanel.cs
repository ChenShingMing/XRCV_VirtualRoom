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
            bool isPointingAtUI = InputHandler_OpenXR.ins != null
                && InputHandler_OpenXR.ins.rayInteractor != null
                && InputHandler_OpenXR.ins.rayInteractor.TryGetCurrentUIRaycastResult(out var uiResult)
                && uiResult.gameObject != null;

            penTip.SetActive(!isPointingAtUI);

            if (!isPointingAtUI)
            {
                _tipTimer += Time.fixedDeltaTime;
                if (_tipTimer >= TIP_INTERVAL)
                {
                    _tipTimer = 0f;
                    penTip.transform.position = ClassroomManager.ins.inputActionManager.GetInputPointerOnGazeSphere();
                }
            }
        }
        else
        {
            penTip.SetActive(false);
        }
    }
}
