using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class TopicController : MonoBehaviourPunCallbacks
{
    public Topic topic;
    public GameObject menu;
    public ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();

    public override void OnEnable()
    {
        //若使用者身分是學生，就監聽教學模式切換的事件
        if (Player.localPlayer
            && Player.localPlayer.identity == Player.Identity.Student)
        {
            ClassroomManager.ins.OnSwitchTeachingTypeToGuidance.AddListener(OnSwitchTeachingToGuidance);
            ClassroomManager.ins.OnSwitchTeachingTypeToSelfStudy.AddListener(OnSwitchTeachingToSelfStudy);
        }
    }

    public override void OnDisable()
    {
        //若使用者身分是學生，就移除監聽教學模式切換的事件
        if (Player.localPlayer
            && Player.localPlayer.identity == Player.Identity.Student)
        {
            ClassroomManager.ins.OnSwitchTeachingTypeToGuidance.RemoveListener(OnSwitchTeachingToGuidance);
            ClassroomManager.ins.OnSwitchTeachingTypeToSelfStudy.RemoveListener(OnSwitchTeachingToSelfStudy);
        }
    }


    /// <summary>
    /// 當自己是老師 或是 被監控的對象時，就發送同步參數
    /// </summary>
    public abstract void SendTopicControllerProperties();

    /// <summary>
    /// 根據同步目標的名稱來同步參數
    /// </summary>
    public abstract void UpdateTopicControllerProperties();

    /// <summary>
    /// 當系統切換成導學模式時
    /// </summary>
    public abstract void OnSwitchTeachingToGuidance();

    /// <summary>
    /// 當系統切換成自學模式時
    /// </summary>
    public abstract void OnSwitchTeachingToSelfStudy();
}
