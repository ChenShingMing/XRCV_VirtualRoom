using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum Identity
    {
        Teacher,
        Student,
        Monitor,
        None,
    }

    public static Player localPlayer;
    private static readonly List<Player> _allPlayers = new List<Player>();

    public string playerName;
    public bool isMaster;

    public Identity identity;

    [HideInInspector]
    public GazeController gazeController;

    public Vector3 gazePoint;

    private void Awake()
    {
        _allPlayers.Add(this);
        gazeController = GetComponentInChildren<GazeController>();

        playerName = photonView.Owner.NickName;
        isMaster = photonView.Owner.IsMasterClient;

        if (isMaster)
        {
            identity = Identity.Teacher;

            if (photonView.IsMine)
            {
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props.Add("TeacherNickName", PhotonNetwork.NickName);
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);

                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            }
        }
        else
        {
            if (playerName.Contains("Monitor"))
            {
                identity = Identity.Monitor;
                Debug.Log("Monitor");
            }
            else
            {
                identity = Identity.Student;
                Debug.Log("Student");
            }
        }


        if (photonView.IsMine)
        {
            localPlayer = this;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        GazeControllerInit();
    }

    // Update is called once per frame
    void Update()
    {
        GazeControllerHandle();
    }

    private void OnDestroy()
    {
        _allPlayers.Remove(this);
    }

    public static Identity GetIdentityByName(string searchName)
    {
        for (int i = 0; i < _allPlayers.Count; i++)
        {
            if (_allPlayers[i].playerName == searchName)
                return _allPlayers[i].identity;
        }
        return Identity.None;
    }


    #region Private 

    void GazeControllerInit()
    {
        switch(identity)
        {
            case Identity.Teacher:

                gazeController.pointViewType = GazeController.PointViewType.Teacher;

                break;

            case Identity.Student:

                gazeController.pointViewType = GazeController.PointViewType.Student;
                Debug.Log("gazeController Student");
                break;

            case Identity.Monitor:

                gazeController.pointViewType = GazeController.PointViewType.None;
                Debug.Log("gazeController None");
                break;
        }



        //關閉不用計算的 gazeController
        if (!PhotonNetwork.IsMasterClient)
        {
            if (!isMaster && !photonView.IsMine)
            {
                gazeController.gameObject.SetActive(false);
            }

        }

        if (photonView.IsMine)
        {
            gazeController.isLocalPlayer = true;
            gazeController.isSmooth = false;
        }
        else
        {
            gazeController.isLocalPlayer = false;
            gazeController.isSmooth = true;
        }
    }

    void GazeControllerHandle()
    {
        if (photonView.IsMine)
        {
            gazePoint = gazeController.point;
        }
        else
        {
            gazeController.point = gazePoint;
        }
    }

    

    #endregion
    
    #region IPunObservable implementation

    //可同步參數
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(gazePoint);
        }
        else
        {
            // Network player, receive data
            this.gazePoint = (Vector3)stream.ReceiveNext();
        }
    }

    #endregion
}
