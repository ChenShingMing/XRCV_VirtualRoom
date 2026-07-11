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
    //public RoomListUpdater roomListUpdater; //�bOnEnable ��s�ۨ��M��A���Ѩ�s�M�檺��k

    [FoldoutGroup("Other")]
    public Rename rename;

    [FoldoutGroup("Other")]
    public TMP_InputField licsenceText;

    [FoldoutGroup("Other")]
    public GameObject licensePanel;

    private string identity;
    private bool isInit;

    #region MonoBehavior

    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }

        //InitSetUp();
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

        licsenceText.text = PlayerPrefs.GetString(ClassroomManager.ins.LICENCE_KEY);

        ReflashTopicList();

        ClassroomManager.ins.OnPassLicenseEvent.AddListener(OnPassLicense);
        ClassroomManager.ins.OnFailLicenseEvent.AddListener(OnFailLicense);

        isInit = true;
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



    //��TopicUI ���s�ϥΤ��\�� �]�wTopic
    public void SetTopic()
    {
        string topicName = room_Teacher.GetSelect();

        if (topicName == null || topicName == string.Empty) return;

        ClassroomManager.ins.SetTopic(room_Teacher.GetSelect());
    }
    //��TopicUI ���s�ϥΤ��\�� ���}Topic
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
        room_Monitor.Refresh();
    }

    public void StopMonitorRequest()
    {
        ClassroomManager.ins.newMonitorManager.StopReceive();
        room_Monitor.Refresh();
    }

    public void SetPenColor(string value)
    {
        ClassroomManager.ins.RPCSetPenColor(value);
    }

    public void ClearPenCanvas()
    {
        ClassroomManager.ins.RPCClearCanvas();
    }

    public void SetLicenseKey(string key)
    {
        ClassroomManager.ins.SetLicense(licsenceText.text);
    }

    public void CheckLicense()
    {
        ClassroomManager.ins.CheckListence();
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
            case DisconnectCause.ClientTimeout: //����������A�����T���A�P���A���s�u���_
            case DisconnectCause.DisconnectByServerReasonUnknown: //�����s�u��]�A�P���A���s�u���_
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

    private void OnPassLicense()
    {
        
        if(licensePanel != null)
            licensePanel.SetActive(false);
        

        mainPanel.SetActive(true);
    }

    private void OnFailLicense()
    {
        if (licensePanel != null)
            licensePanel.SetActive(true);

        
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
