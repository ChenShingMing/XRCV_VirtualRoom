using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class PunNetworkManager : MonoBehaviourPunCallbacks
{
    public enum LoadingLevel
    {
        Easy,
        Normal,//大於20人
        Hard, //大於40人
        VeryHard, //大於60人
    }


    public static PunNetworkManager ins;

    [TitleGroup("參數設定")]

    [FoldoutGroup("參數設定/房間")]
    public string lobbyName;

    [FoldoutGroup("參數設定/房間")]
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    public byte maxPlayersPerRoom = 4;

    [HideInInspector]
    public Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    //用於Odin Inspector
    [HideInInspector]
    public bool isConnected = false;

    [HideInInspector]
    public UnityEvent OnJoinRoomEvent = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnDisconnectEvent = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnLeaveRoomWithSameNameEvent = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnRoomListUpdateEvent = new UnityEvent();

    [SerializeField]
    private string roomName;
    private TypedLobby typedLobby;

    string gameVersion = "1";


    #region MonoBehavior

    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }

        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        //PhotonNetwork.AutomaticallySyncScene = true;
        typedLobby = new TypedLobby(lobbyName, LobbyType.Default);
    }

    private void Update()
    {
        isConnected = PhotonNetwork.IsConnected;
    }

    #endregion


    #region MonoBehaviourPunCallbacks Callbacks


    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN");
        PhotonNetwork.JoinLobby(typedLobby);
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        cachedRoomList.Clear();
        Debug.LogFormat("OnDisconnected() was called by PUN with reason {0}", cause);

        if (OnDisconnectEvent != null)
        {
            OnDisconnectEvent.Invoke();
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed() was called by PUN. Cannot Find room :" + message);
    }


    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. Room name : " + PhotonNetwork.CurrentRoom.Name);

        bool canJoinRoom = true; ;

        foreach (KeyValuePair<int, Photon.Realtime.Player> item in PhotonNetwork.CurrentRoom.Players)
        {
            if (item.Value.NickName == PhotonNetwork.NickName)
            {
                if (item.Value != PhotonNetwork.LocalPlayer)
                {
                    Debug.Log("There are Player with same NickName : " + PhotonNetwork.NickName + " in the room : " + PhotonNetwork.CurrentRoom.Name);
                    canJoinRoom = false;
                    PhotonNetwork.LeaveRoom();

                    if (OnLeaveRoomWithSameNameEvent != null)
                    {
                        OnLeaveRoomWithSameNameEvent.Invoke();
                    }
                }
            }
        }

        //不論有無觸發，都必須移除所有監聽。
        OnLeaveRoomWithSameNameEvent.RemoveAllListeners();

        if (OnJoinRoomEvent != null && canJoinRoom)
        {
            OnJoinRoomEvent.Invoke();
        }
    }

    /// <summary>
    /// 有人加入房間時
    /// </summary>
    /// <param name="other"></param>
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            ClassroomManager.ins.RPCUpdatePunLoadingLevel();
        }
    }

    /// <summary>
    /// 有人離開房間時
    /// </summary>
    /// <param name="other"></param>
    public override void OnPlayerLeftRoom(Photon.Realtime.Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            ClassroomManager.ins.RPCUpdatePunLoadingLevel();
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        cachedRoomList.Clear();
    }

    public override void OnLeftLobby()
    {
        base.OnJoinedLobby();
        cachedRoomList.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate room list count : " + roomList.Count);
        UpdateCachedRoomList(roomList);
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

        Debug.Log("Master 更換人");
        DisConnect();
    }


    #endregion


    #region Private Method

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }

            Debug.Log("Find : " + roomList[i].Name);
        }

        OnRoomListUpdateEvent?.Invoke();
    }

    #endregion


    #region Public Method

    /// <summary>
    /// 連線到PUN伺服器
    /// </summary>
    [Button]
    [FoldoutGroup("Method")]
    [HideIf("isConnected", true)]
    public void Connect()
    {
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected == false)
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    /// <summary>
    /// 退出PUN伺服器
    /// </summary>
    [Button]
    [FoldoutGroup("Method")]
    [ShowIf("isConnected", true)]
    public void DisConnect()
    {
        PhotonNetwork.Disconnect();
    }

    /// <summary>
    /// 房間加入
    /// </summary>
    public void CreateRoom(string roomName)
    {
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = maxPlayersPerRoom }, typedLobby);
        }
        else
        {
            Debug.Log("Cannot join room because PhotonNetwork is DisConnect.");
        }
    }

    /// <summary>
    /// 房間加入
    /// </summary>
    [Button]
    [FoldoutGroup("Method")]
    [ShowIf("isConnected", true)]
    public void CreateRoom()
    {
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = maxPlayersPerRoom }, typedLobby);
        }
        else
        {
            Debug.Log("Cannot join room because PhotonNetwork is DisConnect.");
        }
    }

    /// <summary>
    /// 房間加入
    /// </summary>
    public void JoinRoom(string roomName)
    {
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.Log("Cannot join room because PhotonNetwork is DisConnect.");
        }
    }

    [Button]
    [FoldoutGroup("Method")]
    [ShowIf("isConnected", true)]
    public void JoinRoom()
    {
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.Log("Cannot join room because PhotonNetwork is DisConnect.");
        }
    }

    public void SetRoomName(string roomName)
    {
        this.roomName = roomName;
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeaveRoom()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("User is LeaveRoom");
        }
        else
        {
            Debug.Log("Not in any room.");
        }
    }

    public void CallRpMethod()
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("RpMethod", RpcTarget.All);
    }

    public void SetPhotonAppID(string value)
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = value;
    }

    public void SetLoadingLevel(LoadingLevel targetLevel)
    {
        int serializationRate = 0, sendRate = 0;

        switch (targetLevel)
        {
            case LoadingLevel.Easy:

                serializationRate = 10;
                sendRate = 10;

                break;
            case LoadingLevel.Normal:

                serializationRate = 5;
                sendRate = 5;

                break;
            case LoadingLevel.Hard:

                serializationRate = 3;
                sendRate = 3;

                break;
            case LoadingLevel.VeryHard:

                serializationRate = 1;
                sendRate = 1;

                break;
        }

        PhotonNetwork.SerializationRate = serializationRate;
        PhotonNetwork.SendRate = sendRate;

        Debug.Log("PunNetworkManager LoadingLevel : " + targetLevel);
    }


    [Button]
    public void DebugInformation()
    {
        // 取得目前的大廳名稱
        string lobbyName = PhotonNetwork.CurrentLobby.Name;

        // 取得房間列表
        int roomCount = PhotonNetwork.CountOfRooms;
        int playerCount = PhotonNetwork.CountOfPlayers;

        Debug.Log(lobbyName + " 玩家數量：" + playerCount + "房間數量：" + roomCount);
    }

    [Button]
    void DogIPAddress()
    {
        // 打印當前連接的Photon伺服器地址
        string serverAddress = PhotonNetwork.NetworkingClient.LoadBalancingPeer.ServerAddress;
        Debug.Log($"當前連接的Photon伺服器地址：{serverAddress}");
    }


    #endregion


    #region RPC Method

    [PunRPC]
    void RpMethod()
    {
        //Do something
    }

    #endregion

    #region Static Method

    public static Photon.Realtime.Player GetPhotonPlayerByNickName(string nickName)
    {
        foreach (KeyValuePair<int, Photon.Realtime.Player> item in PhotonNetwork.CurrentRoom.Players)
        {
            if (item.Value.NickName == nickName)
            {
                return item.Value;
            }
        }

        Debug.Log("Cannot Find NickName :" + nickName + " In Current Room : " + PhotonNetwork.CurrentRoom.Name);
        return null;
    }

    #endregion

}
