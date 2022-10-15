using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;

public class StarMapTopicController : TopicController
{
    public StarMapController starMapController;
    private StarMapTopicData selfStarMapTopicData = new StarMapTopicData();

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
        starMapController.starMapControlData.graticule = selfStarMapTopicData.graticule;
        starMapController.starMapControlData.linkLine = selfStarMapTopicData.linkLine;
        starMapController.starMapControlData.nameAndModel = selfStarMapTopicData.nameAndModel;

        starMapController.starMapControlData.Year = selfStarMapTopicData.Year;
        starMapController.starMapControlData.Month = selfStarMapTopicData.Month;
        starMapController.starMapControlData.Day = selfStarMapTopicData.Day;
        starMapController.starMapControlData.Hour = selfStarMapTopicData.Hour;
        starMapController.starMapControlData.rotateSpeed = selfStarMapTopicData.rotateSpeed;

        starMapController.starMapControlData.usePanoramic = selfStarMapTopicData.usePanoramic;
        starMapController.starMapControlData.currentlocalicationIndex = selfStarMapTopicData.currentlocalicationIndex;
        starMapController.starMapControlData.currentLocalicationName = selfStarMapTopicData.currentLocalicationName;
        starMapController.starMapControlData.latitude = selfStarMapTopicData.latitude;
        starMapController.starMapControlData.longitude = selfStarMapTopicData.longitude;
    }

    #region TopicController

    public override void SendTopicControllerProperties()
    {
        props.Clear();

        props.Add(topic.topicName + "_starMapControlData_graticule_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.graticule);
        props.Add(topic.topicName + "_starMapControlData_linkLine_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.linkLine);
        props.Add(topic.topicName + "_starMapControlData_nameAndModel_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.nameAndModel);
        props.Add(topic.topicName + "_starMapControlData_Year_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.Year);
        props.Add(topic.topicName + "_starMapControlData_Month_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.Month);
        props.Add(topic.topicName + "_starMapControlData_Day_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.Day);
        props.Add(topic.topicName + "_starMapControlData_Hour_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.Hour);
        props.Add(topic.topicName + "_starMapControlData_rotateSpeed_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.rotateSpeed);
        props.Add(topic.topicName + "_starMapControlData_usePanoramic_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.usePanoramic);
        props.Add(topic.topicName + "_starMapControlData_currentlocalicationIndex_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.currentlocalicationIndex);
        props.Add(topic.topicName + "_starMapControlData_currentLocalicationName_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.currentLocalicationName);
        props.Add(topic.topicName + "_starMapControlData_latitude_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.latitude);
        props.Add(topic.topicName + "_starMapControlData_longitude_" + Player.localPlayer.playerName, this.starMapController.starMapControlData.longitude);
        

        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public override void UpdateTopicControllerProperties()
    {
        ExitGames.Client.Photon.Hashtable temp = PhotonNetwork.CurrentRoom.CustomProperties;
        
        this.starMapController.starMapControlData.graticule = (bool)temp[topic.topicName + "_starMapControlData_graticule_" + topic.syncSenderName];
        this.starMapController.starMapControlData.linkLine = (bool)temp[topic.topicName + "_starMapControlData_linkLine_" + topic.syncSenderName];
        this.starMapController.starMapControlData.nameAndModel = (bool)temp[topic.topicName + "_starMapControlData_nameAndModel_" + topic.syncSenderName];
        this.starMapController.starMapControlData.Year = (int)temp[topic.topicName + "_starMapControlData_Year_" + topic.syncSenderName];
        this.starMapController.starMapControlData.Month = (int)temp[topic.topicName + "_starMapControlData_Month_" + topic.syncSenderName];
        this.starMapController.starMapControlData.Day = (int)temp[topic.topicName + "_starMapControlData_Day_" + topic.syncSenderName];
        this.starMapController.starMapControlData.Hour = (int)temp[topic.topicName + "_starMapControlData_Hour_" + topic.syncSenderName];
        this.starMapController.starMapControlData.rotateSpeed = (float)temp[topic.topicName + "_starMapControlData_rotateSpeed_" + topic.syncSenderName];
        this.starMapController.starMapControlData.usePanoramic = (bool)temp[topic.topicName + "_starMapControlData_usePanoramic_" + topic.syncSenderName];
        this.starMapController.starMapControlData.currentlocalicationIndex = (int)temp[topic.topicName + "_starMapControlData_currentlocalicationIndex_" + topic.syncSenderName];
        this.starMapController.starMapControlData.currentLocalicationName = (string)temp[topic.topicName + "_starMapControlData_currentLocalicationName_" + topic.syncSenderName];
        this.starMapController.starMapControlData.latitude = (float)temp[topic.topicName + "_starMapControlData_latitude_" + topic.syncSenderName];
        this.starMapController.starMapControlData.longitude = (float)temp[topic.topicName + "_starMapControlData_longitude_" + topic.syncSenderName];
    }

    public override void OnSwitchTeachingToGuidance()
    {
        selfStarMapTopicData.SetData(starMapController.starMapControlData);
        Debug.Log("切換成導學模式，紀錄當前score");
    }

    public override void OnSwitchTeachingToSelfStudy()
    {
        SetStarMapControlData();
        Debug.Log("切換成自學模式，恢復score至上次自學時");
    }

    #endregion
}
