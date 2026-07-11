using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;

//此Class提供所有操作的功能實作
public class InputActionManager : MonoBehaviour
{
    [FoldoutGroup("物件設置")]
    public InputHandler currentInputHandler;

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    #region Public Method

    public void SetCurrentInputHandler(InputHandler value)
    {
        currentInputHandler = value;
    }

    public Vector3 GetInputPointerOnGazeSphere()
    {
        return currentInputHandler.GetInputPointerOnGazeSphere();
    }

    #endregion

    #region Actions

    /// <summary>
    /// 打開 主選單
    /// </summary>
    public void TriggerMainMenu()
    {
        ClassroomManager.ins.SetMainMenuActive(!ClassroomManager.ins.mainUICanvas_Current.activeSelf);
    }

    /// <summary>
    /// 打開 課程選單
    /// </summary>
    public void TriggerMenu()
    {
        if (PhotonNetwork.IsConnected == false) return;
        if (Player.localPlayer == null) return;


        if (Player.localPlayer.isMaster) //如果是老師
        {
            //就觸發選單功能
            ClassroomManager.ins.TriggerMenu();
        }
        else
        {
            if(ClassroomManager.ins.teachingType == ClassroomManager.TeachingType.SelfStudy)
            {
                ClassroomManager.ins.TriggerMenu();
            }
        }
    }

    /// <summary>
    /// 當觸發 按下送出 時
    /// </summary>
    /// <param name="rotation"></param>
    public void OnSubmitDownTrigger(Quaternion rotation)
    {
        if (PhotonNetwork.IsConnected == false) return;
        if (Player.localPlayer == null) return;

        if (PhotonNetwork.IsConnected && Player.localPlayer.isMaster)
        {
            //if (ClassroomManager.ins.topicManager.currentTopic == null) return;

            //當沒有啟用控制UI時
            //if (ClassroomManager.ins.GetCurrentMainMenuActive() == false && ClassroomManager.ins.GetCurrentTopicMenuActive() == false)
           
            if (ClassroomManager.ins.isPenMode)
            {
                ClassroomManager.ins.RPCSetTipLinePointViwe(true, GazeSphere.RayHitOnSphereLine(new Ray(_mainCamera.transform.position,
                                                                             rotation * Vector3.forward)));
            }
            else
            {


                ClassroomManager.ins.RPCSetTipPointViwe(GazeSphere.RayHitOnSphere(new Ray(_mainCamera.transform.position,
                                                                                                           rotation * Vector3.forward)));
            }

        }
    }

    /// <summary>
    /// 當觸發 按住送出 時
    /// </summary>
    /// <param name="rotation"></param>
    public void OnSubmitTrigger(Quaternion rotation)
    {
        if (PhotonNetwork.IsConnected == false) return;
        if (Player.localPlayer == null) return;

        if (PhotonNetwork.IsConnected && Player.localPlayer.isMaster)
        {
            //if (ClassroomManager.ins.topicManager.currentTopic == null) return;

            //當沒有啟用控制UI時
            //if (ClassroomManager.ins.GetCurrentMainMenuActive() == false && ClassroomManager.ins.GetCurrentTopicMenuActive() == false)

            if (ClassroomManager.ins.isPenMode)
            {
                Vector3 pos = GazeSphere.RayHitOnSphereLine(new Ray(_mainCamera.transform.position,
                                                                         rotation * Vector3.forward));

                if (Vector3.Distance(pos, ClassroomManager.ins.lineController.pos) > 0.15f)
                {
                    ClassroomManager.ins.RPCSetLinePoint(pos);
                }
            }
        }
    }

    /// <summary>
    /// 當觸發 放開送出 時
    /// </summary>
    /// <param name="rotation"></param>
    public void OnSubmitUpTrigger(Quaternion rotation)
    {
        if (PhotonNetwork.IsConnected == false) return;
        if (Player.localPlayer == null) return;

        if (PhotonNetwork.IsConnected && Player.localPlayer.isMaster)
        {
            //if (ClassroomManager.ins.topicManager.currentTopic == null) return;

            //當沒有啟用控制UI時
            //當沒有啟用控制UI時
            //if (ClassroomManager.ins.GetCurrentMainMenuActive() == false && ClassroomManager.ins.GetCurrentTopicMenuActive() == false)

            if (ClassroomManager.ins.isPenMode)
            {
                //Debug.Log("isPenMode Up");
                ClassroomManager.ins.RPCSetTipLinePointViwe(false, GazeSphere.RayHitOnSphereLine(new Ray(_mainCamera.transform.position,
                                                                             rotation * Vector3.forward)));
            }

        }
    }

    public void SetPenPode(bool value)
    {
        ClassroomManager.ins.isPenMode = value;
    }

    public void TriggerPenPode()
    {
        ClassroomManager.ins.isPenMode = !ClassroomManager.ins.isPenMode;
    }

    public void TriggerFirstPersonView(bool value)
    {
        ClassroomManager.ins.firstPersonCam.active = value;
    }

    public void TriggerMonitorPointerClick()
    {
        if (ClassroomManager.ins.newMonitorManager.type != NewMonitorManager.Type.Sender) return;
        ClassroomManager.ins.RPCSendMonitorPointerClick();
    }

    #endregion
}
