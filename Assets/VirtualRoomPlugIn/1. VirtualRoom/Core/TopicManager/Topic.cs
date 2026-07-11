using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;
using System;

public class Topic
{
    [FoldoutGroup("通用參數")]
    public string topicName;
    [FoldoutGroup("通用參數")]
    public TopicController controller;

    [HideInInspector]
    public GameObject menu;

    [HideInInspector]
    public string syncSenderName;
    private object topicManager;
    private string _cachedMasterNickName;

    public virtual void OnInit()
    {
        controller = GameObject.Instantiate(controller, ClassroomManager.ins.topicManager.transform);
        controller.topic = this;
        menu = controller.menu;

        controller.gameObject.SetActive(false);
    }

    public virtual void OnEnter()
    {
        controller.gameObject.SetActive(true);

        _cachedMasterNickName = PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.CurrentRoom.MasterClientId).NickName;
        SetSyncSenderName(_cachedMasterNickName);
    }

    public virtual void OnUpdate()
    {
        if (ClassroomManager.ins.teachingType == ClassroomManager.TeachingType.Guidance)
        {
            SetSyncSenderName(_cachedMasterNickName);
        }
        else if (ClassroomManager.ins.teachingType == ClassroomManager.TeachingType.SelfStudy)
        {
            if (Player.localPlayer.identity == Player.Identity.Student)
            {
                SetSyncSenderName(string.Empty);
            }
        }
    }

    public virtual void OnExit()
    {
        controller.gameObject.SetActive(false);
    }

    public virtual void OnSync()
    {
        //如果local端是老師或是被監控者，就必須傳送自己的Topic參數
        if (Player.localPlayer.isMaster || ClassroomManager.ins.newMonitorManager.type == NewMonitorManager.Type.Sender)
        {
            controller.SendTopicControllerProperties();
        }

        //其餘者做同步
        if (Player.localPlayer.isMaster == false)
        {
            //Debug.Log(syncSenderName);
            //如果當前的目標不等於空的就要同步，狀況為，導學模式或監看模式，並處於自學模式時
            if (syncSenderName != string.Empty)
            {
                //因有可能需要同步時，目標值還沒傳出來
                try
                {
                    controller.UpdateTopicControllerProperties();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
        }
    }


    public virtual void TriggerMenu()
    {
        menu.SetActive(!menu.activeSelf);
    }

    public virtual void DisableMenu()
    {
        menu.SetActive(false);
    }

    public void SetSyncSenderName(string senderName)
    {
        syncSenderName = senderName;
    }

}
