using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class MainUI : MonoBehaviourPunCallbacks
{
    public enum ConnectState
    {
        Disconnect,
        Connect,
    }

    public enum RoomState
    {
        Disconnect,
        InTheLobby,
        InTheRoom,
    }

    public static MainUI ins;

    [BoxGroup("Network Information")]
    [ReadOnly]
    public ConnectState connectState = ConnectState.Disconnect;
    [BoxGroup("Network Information")]
    [ReadOnly]
    public RoomState roomState = RoomState.Disconnect;

    [FoldoutGroup("Network")]
    public GameObject connectBtn;
    [FoldoutGroup("Network")]
    public GameObject disconnectBtn;
    [FoldoutGroup("Network")]
    public GameObject nickName;
    [FoldoutGroup("Network")]
    public Text infoBox;
    [FoldoutGroup("Network")]
    public GameObject roomCollection;
    [FoldoutGroup("Network")]
    public InputField roomNameInputField;

    [FoldoutGroup("Classroom")]
    public Text identityInfo;
    [FoldoutGroup("Classroom")]
    public Text playerNumInfo;
    [FoldoutGroup("Classroom")]
    public Text currentTopicInfo;
    [FoldoutGroup("Classroom")]
    public Text teachingTypeInfo;

    [FoldoutGroup("Topic")]
    public GameObject topicUI;
    [FoldoutGroup("Topic")]
    public Dropdown topicDropdown;
    [FoldoutGroup("Topic")]
    public Dropdown teachingTypeDropdown;


    [FoldoutGroup("Tools")]
    public GameObject toolsUI;
    [FoldoutGroup("Tools")]
    public Dropdown currentTipTool;
    [FoldoutGroup("Tools")]
    public GameObject penTools;
    [FoldoutGroup("Tools")]
    public Dropdown colorSelecter;

    [FoldoutGroup("Monitor")]
    public GameObject monitorUI;
    [FoldoutGroup("Monitor")]
    public Dropdown monitorDropdown;


    private string identity;
    private float _uiRefreshTimer;
    private const float UI_REFRESH_INTERVAL = 0.5f;

    #region Monobehavior

    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }

        connectState = ConnectState.Disconnect;
        roomState = RoomState.Disconnect;

        connectBtn.SetActive(true);
        disconnectBtn.SetActive(false);
        nickName.SetActive(true);
        roomCollection.SetActive(false);

        ReflashTopicList();
        infoBox.text = "Disconnect now. \nPlease Connect to PUN Server.";
    }

    public void FixedUpdate()
    {
        ToolsUIHandle();

        _uiRefreshTimer += Time.fixedDeltaTime;
        if (_uiRefreshTimer < UI_REFRESH_INTERVAL) return;
        _uiRefreshTimer = 0f;

        if (PhotonNetwork.InRoom)
        {
            identity = ClassroomManager.ins.GetIdentityInfo();

            identityInfo.text = identity;
            playerNumInfo.text = "There are " + ClassroomManager.ins.GetStudentNumber() + " students in this room.";
            currentTopicInfo.text = ClassroomManager.ins.currentTopicName;
            teachingTypeInfo.text = ClassroomManager.ins.teachingType.ToString();

            switch(ClassroomManager.ins.joinType)
            {
                case ClassroomManager.JoinType.Member:

                    if (identity == Player.Identity.Teacher.ToString())
                    {
                        topicUI.SetActive(true);
                        toolsUI.SetActive(true);
                    }

                    break;

                case ClassroomManager.JoinType.Monitor:

                    monitorUI.SetActive(true);

                    break;
            }
        }
        else
        {
            identityInfo.text = "--";
            playerNumInfo.text = "--";
            currentTopicInfo.text = "--";
            teachingTypeInfo.text = "--";

            topicUI.SetActive(false);
            toolsUI.SetActive(false);
            monitorUI.SetActive(false);
        }
    }

    #endregion

    #region UI Public Method

    public void SetPlayerName(string value)
    {
        // #Important
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }

        //Debug.Log("SetPlayerName : " + value);
        PhotonNetwork.NickName = value;
    }

    public void CreateRoom()
    {
        PunNetworkManager.ins.CreateRoom(roomNameInputField.text);
    }

    //根據使用的 Dropdown values 設定來設置是否啟用畫筆
    public void SetTipTools(int value)
    {
        if(value == 0)
        {
            ClassroomManager.ins.SetPenMode(false);
        }
        else if(value == 1)
        {
            ClassroomManager.ins.SetPenMode(true);
        }
    }

    //讓TopicUI 按鈕使用之功能 設定Topic
    public void SetTopic()
    {
        //ClassroomManager.ins.RPCSetTopic(topicDropdown.options[topicDropdown.value].text);
        ClassroomManager.ins.SetTopic(topicDropdown.options[topicDropdown.value].text);
    }
    //讓TopicUI 按鈕使用之功能 離開Topic
    public void ExitTopic()
    {
        //ClassroomManager.ins.RPCExitTopic();
        ClassroomManager.ins.ExitTopic();
    }

    public void SetTeachingType()
    {
        switch(teachingTypeDropdown.options[teachingTypeDropdown.value].text)
        {
            case "Guidance":

                ClassroomManager.ins.SetTeachingType(ClassroomManager.TeachingType.Guidance);

                break;

            case "SelfStudy":

                ClassroomManager.ins.SetTeachingType(ClassroomManager.TeachingType.SelfStudy);

                break;
        }
    }

    //根據使用的 Dropdown values 設定來設置畫筆顏色
    public void SetPenColor(int value)
    {
        ClassroomManager.ins.RPCSetPenColor(colorSelecter.options[value].text);
    }

    public void ClearPenCanvas()
    {
        ClassroomManager.ins.RPCClearCanvas();
    }

    public void MonitorRequest()
    {
        ClassroomManager.ins.newMonitorManager.StartReceive(monitorDropdown.options[monitorDropdown.value].text);
    }

    public void StopMonitorRequest()
    {
        ClassroomManager.ins.newMonitorManager.StopReceive(monitorDropdown.options[monitorDropdown.value].text);
    }

    #endregion

    #region MonoBehaviourPunCallbacks

    public override void OnConnectedToMaster()
    {
        //Debug.Log("MainUI OnConnectedToMaster");

        connectState = ConnectState.Connect;
        roomState = RoomState.InTheLobby;

        connectBtn.SetActive(false);
        nickName.SetActive(true);
        disconnectBtn.SetActive(true);
        roomCollection.SetActive(true);

        infoBox.text = "Welcome.";
    }

    public override void OnJoinedLobby()
    {
        //Debug.Log("MainUI OnJoinedLobby");

        infoBox.text = "Welcome "+ PhotonNetwork.NickName + "\nLobby name : " + PhotonNetwork.CurrentLobby.Name;
    }

    public override void OnJoinedRoom()
    {
        //Debug.Log("MainUI OnJoinedRoom");

        roomState = RoomState.InTheRoom;

        connectBtn.SetActive(false);
        disconnectBtn.SetActive(true);
        nickName.SetActive(true);
        roomCollection.SetActive(false);

        infoBox.text = "Welcome " + PhotonNetwork.NickName + "\nLobby name : " + PhotonNetwork.CurrentLobby.Name + "\nRoom name : " + PhotonNetwork.CurrentRoom.Name;

        ReflashMonitorList();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player other)
    {
        ReflashMonitorList();
        ClassroomManager.ins.newMonitorManager.InvalidateGazeControllerCache();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player other)
    {
        ReflashMonitorList();
        ClassroomManager.ins.newMonitorManager.InvalidateGazeControllerCache();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //Debug.Log("MainUI OnDisconnected");

        connectState = ConnectState.Disconnect;
        roomState = RoomState.Disconnect;

        connectBtn.SetActive(true);
        nickName.SetActive(true);
        disconnectBtn.SetActive(false);
        roomCollection.SetActive(false);

        infoBox.text = "Disconnect now. \nPlease Connect to PUN Server.";
    }

    #endregion

    #region UI Private Method

    private void ReflashTopicList()
    {
        foreach (KeyValuePair<string, Topic> item in ClassroomManager.ins.topicManager.topicDic)
        {
            topicDropdown.options.Add(new Dropdown.OptionData(item.Key));
        }
    }

    public void ToolsUIHandle()
    {
        if(currentTipTool.options[currentTipTool.value].text == "Pen")
        {
            penTools.SetActive(true);
        }
        else
        {
            penTools.SetActive(false);
        }
    }

    private void ReflashMonitorList()
    {
        if (PhotonNetwork.InRoom == false) return;
        if (identity != ClassroomManager.JoinType.Monitor.ToString()) return;

        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            bool needToAdd = true;

            for (int j = 0; j < monitorDropdown.options.Count; j++)
            {
                if (monitorDropdown.options[j].text == PhotonNetwork.PlayerList[i].NickName)
                {
                    needToAdd = false;
                    break;
                }
            }

            if (needToAdd)
            {
                if(PhotonNetwork.PlayerList[i].NickName.Contains("Monitor"))
                {
                    continue;
                }

                monitorDropdown.options.Add(new Dropdown.OptionData(PhotonNetwork.PlayerList[i].NickName));
            }
        }

        for (int i = 0; i < monitorDropdown.options.Count; i++)
        {
            bool needToRemove = true;

            for (int j = 0; j < PhotonNetwork.PlayerList.Length; j++)
            {
                if (PhotonNetwork.PlayerList[j].NickName == monitorDropdown.options[i].text)
                {
                    needToRemove = false;
                    break;
                }
            }

            if (needToRemove)
            {
                monitorDropdown.options.RemoveAt(i);
            }
        }

        monitorDropdown.RefreshShownValue();

        if (monitorDropdown.options.Count != 0)
        {
            if (monitorDropdown.onValueChanged != null)
            {
                monitorDropdown.onValueChanged.Invoke(monitorDropdown.value);
            }
        }

    }


    #endregion
}

