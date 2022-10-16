using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MainUIManager : MonoBehaviourPunCallbacks
{
    #region Enum

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

    #endregion

    public static MainUIManager ins;

    [BoxGroup("Network Information")]
    [ReadOnly]
    public ConnectState connectState = ConnectState.Disconnect;
    [BoxGroup("Network Information")]
    [ReadOnly]
    public RoomState roomState = RoomState.Disconnect;

    [FoldoutGroup("Main")]
    public GameObject mainPanel;

    [FoldoutGroup("Connect")]
    public GameObject connectPanel;

    [FoldoutGroup("Connect")]
    public GameObject changeLicenseBtn;

    [FoldoutGroup("ConnectFail")]
    public GameObject connectFailPanel;

    [FoldoutGroup("Connecting")]
    public GameObject connectingPanel;

    [FoldoutGroup("Lobby")]
    public GameObject lobbyPanel;

    [FoldoutGroup("Lobby")]
    public LobbyPanel lobbyPanelComponent;

    [FoldoutGroup("Lobby")]
    public TMP_InputField createRoomName;


    [FoldoutGroup("InRoom")]
    public InRoomPanel inRoomPanel;
    [FoldoutGroup("InRoom")]
    public Room_Teacher room_Teacher;
    [FoldoutGroup("InRoom")]
    public Room_Monitor room_Monitor;

    //[FoldoutGroup("Lobby")]
    //public RoomListUpdater roomListUpdater; //在OnEnable 刷新自身清單，提供刷新清單的方法

    [FoldoutGroup("ReName")]
    public Rename rename;

    private string identity;

    #region MonoBehavior

    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }

        InitSetUp();
    }

    public void InitSetUp()
    {
        connectState = ConnectState.Disconnect;
        roomState = RoomState.Disconnect;

        connectPanel.SetActive(true);
        connectFailPanel.SetActive(false);
        connectingPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        inRoomPanel.gameObject.SetActive(false);

        ReflashTopicList();

        /*
        if (LicenseManager.ins != null && LicenseManager.ins.gameObject.activeSelf)
        {
            if(changeLicenseBtn != null)
                changeLicenseBtn.SetActive(true);

            LicenseManager.ins.OnCheckSuccess.AddListener(OnLicenseCheckSuccess);
            LicenseManager.ins.OnCheckFail.AddListener(OnLicenseCheckFail);
        }
        else
        {
            if (changeLicenseBtn != null)
                changeLicenseBtn.SetActive(false);
            OnLicenseCheckSuccess();
        }
        */

        if (changeLicenseBtn != null)
            changeLicenseBtn.SetActive(false);
        OnLicenseCheckSuccess();
    }



    public void FixedUpdate()
    {
        ReflashMonitorList();
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

    public void Connect()
    {
        PunNetworkManager.ins.Connect();
    }

    public void CreateRoom()
    {
        if (PunNetworkManager.ins.cachedRoomList.Count >= ClassroomManager.ins.licenseInformation.seatInfo_Teacher)
        {
            lobbyPanelComponent.ShowMaxRoomTip();
            return;
        }

        PunNetworkManager.ins.CreateRoom(createRoomName.text);
    }

    public void JoinRoom()
    {
        string roomName = lobbyPanelComponent.GetSelectRoom();

        if (roomName != null)
        {
            PunNetworkManager.ins.OnLeaveRoomWithSameNameEvent.AddListener(OnLeaveRoomWithSameName); 
            PunNetworkManager.ins.JoinRoom(roomName);
        }
    }

    public void leaveRoom()
    {
        PunNetworkManager.ins.LeaveRoom();
    }

    public void DisConnect()
    {
        PunNetworkManager.ins.DisConnect();
    }


    public void ReName()
    {
        SetPlayerName(rename.inputField.text);
    }



    //讓TopicUI 按鈕使用之功能 設定Topic
    public void SetTopic()
    {
        string topicName = room_Teacher.GetSelect();

        if (topicName == null || topicName == string.Empty) return;

        ClassroomManager.ins.SetTopic(room_Teacher.GetSelect());
    }
    //讓TopicUI 按鈕使用之功能 離開Topic
    public void ExitTopic()
    {
        ClassroomManager.ins.ExitTopic();
    }

    public void SetTeachingType(string value)
    {
        switch (value)
        {
            case "Guidance":

                ClassroomManager.ins.SetTeachingType(ClassroomManager.TeachingType.Guidance);

                break;

            case "SelfStudy":

                ClassroomManager.ins.SetTeachingType(ClassroomManager.TeachingType.SelfStudy);

                break;
        }
    }

    public void MonitorRequest()
    {
        string targetName = room_Monitor.GetSelect();

        if (targetName == null || targetName == string.Empty) return;
        ClassroomManager.ins.newMonitorManager.StartReceive(targetName);
    }

    public void StopMonitorRequest()
    {
        ClassroomManager.ins.newMonitorManager.StopReceive();
    }

    public void SetPenColor(string value)
    {
        ClassroomManager.ins.RPCSetPenColor(value);
    }

    public void ClearPenCanvas()
    {
        ClassroomManager.ins.RPCClearCanvas();
    }

    public void SetLicenseKey(string value)
    {
        //LicenseManager.ins.SetLicenseKey(value);
    }

    public void CheckLicense()
    {
        //LicenseManager.ins.CheckLicense();
    }



    #endregion

    #region MonoBehaviourPunCallbacks

    public override void OnConnectedToMaster()
    {
        //Debug.Log("MainUI OnConnectedToMaster");

        connectState = ConnectState.Connect;
        roomState = RoomState.InTheLobby;

        connectPanel.SetActive(false);
        connectingPanel.SetActive(false);
    }

    public override void OnJoinedLobby()
    {
        //Debug.Log("OnJoinedLobby");
        //infoBox.text = "Welcome " + PhotonNetwork.NickName + "\nLobby name : " + PhotonNetwork.CurrentLobby.Name;

        connectPanel.SetActive(false);
        connectingPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        inRoomPanel.gameObject.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        //Debug.Log("MainUI OnJoinedRoom");
        roomState = RoomState.InTheRoom;
        lobbyPanel.SetActive(false);
        inRoomPanel.gameObject.SetActive(true);

        //infoBox.text = "Welcome " + PhotonNetwork.NickName + "\nLobby name : " + PhotonNetwork.CurrentLobby.Name + "\nRoom name : " + PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //Debug.Log("MainUI OnDisconnected");

        connectState = ConnectState.Disconnect;
        roomState = RoomState.Disconnect;


        switch (cause)
        {
            case DisconnectCause.ClientTimeout: //接收不到伺服器的訊息，與伺服器連線中斷
            case DisconnectCause.DisconnectByServerReasonUnknown: //網路連線原因，與伺服器連線中斷
            case DisconnectCause.DnsExceptionOnConnect: 

                connectPanel.SetActive(false);
                connectFailPanel.SetActive(true);
                connectingPanel.SetActive(false);
                lobbyPanel.SetActive(false);
                inRoomPanel.gameObject.SetActive(false);

                break;


            default:

                connectPanel.SetActive(true);
                connectFailPanel.SetActive(false);
                connectingPanel.SetActive(false);
                lobbyPanel.SetActive(false);
                inRoomPanel.gameObject.SetActive(false);

                break;
        }

    }

    #endregion

    #region UI Private Method

    private void OnLicenseCheckSuccess()
    {
        /*
        if(licensePanel != null)
            licensePanel.SetActive(false);
        */

        mainPanel.SetActive(true);
    }

    private void OnLicenseCheckFail()
    {
        /*
        if (licensePanel != null)
            licensePanel.SetActive(true);

        */
        mainPanel.SetActive(false);
    }

    private void ReflashTopicList()
    {
        foreach (KeyValuePair<string, Topic> item in ClassroomManager.ins.topicManager.topicDic)
        {
            //topicDropdown.options.Add(new Dropdown.OptionData(item.Key));
        }
    }

    private void ReflashMonitorList()
    {
        if (PhotonNetwork.InRoom == false) return;
        if (ClassroomManager.ins.GetIdentityInfo() != ClassroomManager.JoinType.Monitor.ToString()) return;

    }

    private void OnLeaveRoomWithSameName()
    {
        lobbyPanelComponent.lobby.SetActive(false);
        lobbyPanelComponent.createRoom.SetActive(false);
        lobbyPanelComponent.enterRoomFail.SetActive(true);
    }

    #endregion
}
