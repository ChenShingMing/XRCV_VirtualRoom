using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;

public class StarMapTopicController : TopicController
{
    public StarMapController starMapController;
    private StarMapTopicData selfStarMapTopicData = new StarMapTopicData();

    // 單一同步 key：topic名稱 + 固定標識 + sender名稱
    private string SyncKey(string senderName) => topic.topicName + "_starMapData_" + senderName;

    #region MonoBehavior

    public override void OnEnable()
    {
        base.OnEnable();
        SetSelfStarMapTopicData();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    #endregion

    void SetSelfStarMapTopicData()
    {
        selfStarMapTopicData.SetData(starMapController.starMapControlData);
    }

    void SetStarMapControlData()
    {
        selfStarMapTopicData.ApplyTo(starMapController.starMapControlData);
    }

    #region TopicController

    public override void SendTopicControllerProperties()
    {
        // 13 個欄位打包成單一 JSON，減少 Photon 封包數
        var snapshot = new StarMapTopicData();
        snapshot.SetData(starMapController.starMapControlData);

        props.Clear();
        props.Add(SyncKey(Player.localPlayer.playerName), JsonUtility.ToJson(snapshot));
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public override void UpdateTopicControllerProperties()
    {
        string key = SyncKey(topic.syncSenderName);
        var temp = PhotonNetwork.CurrentRoom.CustomProperties;

        if (!temp.ContainsKey(key)) return;

        var snapshot = JsonUtility.FromJson<StarMapTopicData>((string)temp[key]);
        snapshot.ApplyTo(starMapController.starMapControlData);
    }

    public override void OnSwitchTeachingToGuidance()
    {
        selfStarMapTopicData.SetData(starMapController.starMapControlData);
        Debug.Log("切換到導學模式，儲存當前星圖狀態");
    }

    public override void OnSwitchTeachingToSelfStudy()
    {
        SetStarMapControlData();
        Debug.Log("切換到自學模式，還原自學狀態");
    }

    #endregion
}
