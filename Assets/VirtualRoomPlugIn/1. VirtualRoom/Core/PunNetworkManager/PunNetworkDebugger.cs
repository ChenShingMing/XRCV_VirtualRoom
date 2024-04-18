using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class PunNetworkDebugger : MonoBehaviourPunCallbacks
{
    public Text punApplication;
    public Text debugger;

    bool isUsed;

    private void Awake()
    {
        isUsed = false;
        SetActive(false);
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            isUsed = !isUsed;
        }


        if (!isUsed)
        {
            SetActive(false);
            return;
        }

        if (debugger == null) return;
        if (!PhotonNetwork.IsConnected) return;

        SetActive(true);

        // 取得目前的大廳名稱
        string lobbyName = PhotonNetwork.CurrentLobby.Name;

        // 取得房間列表
        int roomCount = PhotonNetwork.CountOfRooms;
        int playerCount = PhotonNetwork.CountOfPlayers;

        string info1 = lobbyName + " 玩家數量：" + playerCount + " 房間數量：" + roomCount;

        string serverAddress = PhotonNetwork.NetworkingClient.LoadBalancingPeer.ServerAddress;
        string info2 = $" 當前連接的Photon伺服器地址：{serverAddress}";

        punApplication.text = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
        debugger.text = info1 + " "+ info2;
    }

    void SetActive(bool active)
    {
        punApplication.gameObject.SetActive(active);
        debugger.gameObject.SetActive(active);
    }

}
