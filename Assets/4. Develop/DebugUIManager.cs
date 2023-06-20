using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class DebugUIManager : MonoBehaviourPunCallbacks
{
    public GameObject panel;

    public Text graticule;
    public Text graticule_Server;
    public Text linkLine;
    public Text linkLine_Server;
    public Text nameAndModel;
    public Text nameAndModel_Server;
    public Text Year;
    public Text Year_Server;
    public Text Month;
    public Text Month_Server;
    public Text Day;
    public Text Day_Server;
    public Text Hour;
    public Text Hour_Server;
    public Text rotateSpeed;
    public Text rotateSpeed_Server;
    public Text usePanoramic;
    public Text usePanoramic_Server;
    public Text currentlocalicationIndex;
    public Text currentlocalicationIndex_Server;
    public Text currentLocalicationName;
    public Text currentLocalicationName_Server;
    public Text latitude;
    public Text latitude_Server;
    public Text longitude;
    public Text longitude_Server;

    private void Awake()
    {
        InvokeRepeating("DataUpdater", 0, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.F1))
        {
            panel.SetActive(!panel.activeSelf);
        }
    }

    void DataUpdater()
    {
        if (!panel.activeSelf) return;
        if (ClassroomManager.ins == null) return;
        if (ClassroomManager.ins.topicManager == null) return;
        if (ClassroomManager.ins.topicManager.currentTopic == null) return;


        if (ClassroomManager.ins.topicManager.currentTopic.topicName == "虛擬星象館")
        {
            StarMapTopicController controller = (StarMapTopicController)ClassroomManager.ins.topicManager.currentTopic.controller;
            ExitGames.Client.Photon.Hashtable temp = PhotonNetwork.CurrentRoom.CustomProperties;

            graticule.text = controller.starMapController.starMapControlData.graticule.ToString();
            linkLine.text = controller.starMapController.starMapControlData.linkLine.ToString();
            nameAndModel.text = controller.starMapController.starMapControlData.nameAndModel.ToString();
            Year.text = controller.starMapController.starMapControlData.Year.ToString();
            Month.text = controller.starMapController.starMapControlData.Month.ToString();
            Day.text = controller.starMapController.starMapControlData.Day.ToString();
            Hour.text = controller.starMapController.starMapControlData.Hour.ToString();
            rotateSpeed.text = controller.starMapController.starMapControlData.rotateSpeed.ToString();
            usePanoramic.text = controller.starMapController.starMapControlData.usePanoramic.ToString();
            currentlocalicationIndex.text = controller.starMapController.starMapControlData.currentlocalicationIndex.ToString();
            currentLocalicationName.text = controller.starMapController.starMapControlData.currentLocalicationName;
            latitude.text = controller.starMapController.starMapControlData.latitude.ToString();
            longitude.text = controller.starMapController.starMapControlData.longitude.ToString();

            graticule_Server.text = ((bool)temp["虛擬星象館_starMapControlData_graticule_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            linkLine_Server.text = ((bool)temp["虛擬星象館_starMapControlData_linkLine_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            nameAndModel_Server.text = ((bool)temp["虛擬星象館_starMapControlData_nameAndModel_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            Year_Server.text = ((int)temp["虛擬星象館_starMapControlData_Year_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            Month_Server.text = ((int)temp["虛擬星象館_starMapControlData_Month_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            Day_Server.text = ((int)temp["虛擬星象館_starMapControlData_Day_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            Hour_Server.text = ((int)temp["虛擬星象館_starMapControlData_Hour_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            rotateSpeed_Server.text = ((float)temp["虛擬星象館_starMapControlData_rotateSpeed_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            usePanoramic_Server.text = ((bool)temp["虛擬星象館_starMapControlData_usePanoramic_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            currentlocalicationIndex_Server.text = ((int)temp["虛擬星象館_starMapControlData_currentlocalicationIndex_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            currentLocalicationName_Server.text = ((string)temp["虛擬星象館_starMapControlData_currentLocalicationName_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            latitude_Server.text = ((float)temp["虛擬星象館_starMapControlData_latitude_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();
            longitude_Server.text = ((float)temp["虛擬星象館_starMapControlData_longitude_" + ClassroomManager.ins.topicManager.currentTopic.syncSenderName]).ToString();

        }
        else
        {
            ClearText();
        }
    }

    void ClearText()
    {
        graticule.text = string.Empty;
        linkLine.text = string.Empty;
        nameAndModel.text = string.Empty;
        Year.text = string.Empty;
        Month.text = string.Empty;
        Day.text = string.Empty;
        Hour.text = string.Empty;
        rotateSpeed.text = string.Empty;
        usePanoramic.text = string.Empty;
        currentlocalicationIndex.text = string.Empty;
        currentLocalicationName.text = string.Empty;
        latitude.text = string.Empty;
        longitude.text = string.Empty;

        graticule_Server.text = string.Empty;
        linkLine_Server.text = string.Empty;
        nameAndModel_Server.text = string.Empty;
        Year_Server.text = string.Empty;
        Month_Server.text = string.Empty;
        Day_Server.text = string.Empty;
        Hour_Server.text = string.Empty;
        rotateSpeed_Server.text = string.Empty;
        usePanoramic_Server.text = string.Empty;
        currentlocalicationIndex_Server.text = string.Empty;
        currentLocalicationName_Server.text = string.Empty;
        latitude_Server.text = string.Empty;
        longitude_Server.text = string.Empty;
    }

}
