using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using UnityEngine.Events;
public class ClassroomManager : MonoBehaviourPunCallbacks
{
    public enum JoinType
    {
        Member,
        Monitor,
    }

    public enum TeachingType
    {
        Guidance,
        SelfStudy,
    }

    public static ClassroomManager ins;

    [HideLabel]
    [BoxGroup("Version")]
    public string version;

    [FoldoutGroup("Player")]
    public GameObject playerPrefab;


    [FoldoutGroup("Manager")]
    public NewMonitorManager newMonitorManager;

    [FoldoutGroup("Manager")]
    public InputActionManager inputActionManager;

    [FoldoutGroup("Manager")]
    public TopicManager topicManager;

    [FoldoutGroup("Manager")]
    public FirebaseLicenseInfoManager firebaseLicenseInfoManager;

    [FoldoutGroup("Controller")]
    public LicenseInformation licenseInformation;

    [FoldoutGroup("Controller")]
    public LineController lineController;
    
    [FoldoutGroup("UI")]
    public GameObject mainUICanvas_Current;

    [FoldoutGroup("Others")]
    public FirstPersonCam firstPersonCam;

    [FoldoutGroup("參數設置")]
    public bool useLicense = true;

    [ReadOnly]
    public JoinType joinType;

    [ReadOnly]
    public TeachingType teachingType = TeachingType.Guidance;

    

    [HideInInspector]
    public bool isPenMode;

    [HideInInspector]
    public string currentTopicName;

    [HideInInspector]
    public UnityEvent OnPassLicenseEvent = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnFailLicenseEvent = new UnityEvent();


    [HideInInspector]
    public UnityEvent OnSwitchTeachingTypeToGuidance = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnSwitchTeachingTypeToSelfStudy = new UnityEvent();

    [HideInInspector]
    public UnityEvent OnSetTopic = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnExitTopic = new UnityEvent();

    [HideInInspector]
    public string LICENCE_KEY = "LicenseKey";


    #region MonoBehavior

    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }

        ResetClassroom();
        mainUICanvas_Current.GetComponentInParent<MainUIManager>(true).InitSetUp();
        OnPassLicenseEvent.AddListener(InitSetUp);


        if(useLicense)
        {
            CheckListence();
        }
        else
        {
            InitSetUp();
        }

    }

    private void InitSetUp()
    {
        InitPhotonInfo();

        currentTopicName = "No Topic Selected.";
        PunNetworkManager.ins.OnJoinRoomEvent.AddListener(InstantiatePlayer);
        PunNetworkManager.ins.OnJoinRoomEvent.AddListener(MappingClassroomProperties);
        PunNetworkManager.ins.OnDisconnectEvent.AddListener(ResetClassroom);
    }

    #endregion

    #region MonoBehaviourPunCallbacks

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        MappingClassroomProperties();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        SetMainMenuActive(true);
    }

    #endregion

    #region Private Method

    private void InstantiatePlayer()
    {
        if (PhotonNetwork.LocalPlayer.NickName.Contains("Monitor"))
        {
            //monitorManager.SetType(MonitorManager.Type.Decoder);
            
            joinType = JoinType.Monitor;

            PhotonNetwork.Instantiate("PunPrefabs/" + playerPrefab.name, Vector3.zero, Quaternion.identity);
        }
        else
        {
            joinType = JoinType.Member;
            PhotonNetwork.Instantiate("PunPrefabs/" + playerPrefab.name, Vector3.zero, Quaternion.identity);

            if(Player.localPlayer != null && Player.localPlayer.isMaster)
            {
                SetTeachingType(TeachingType.Guidance);
            }

        }
    }



    #endregion

    #region Public Method PunRpc不可為Public，所以若要由別的Class執行就必須多寫一個對應的 public method

    [BoxGroup("Version")]
    [Button]
    public void UpdateVersion()
    {
        version = "v" + System.DateTime.Now.ToString("yyyyMMdd");
    }

    #region InputAction

    public bool GetCurrentMainMenuActive()
    {
        return mainUICanvas_Current.activeSelf;
    }

    public bool GetCurrentTopicMenuActive()
    {
        
        if(topicManager.currentTopic == null)
        {
            return false;
        }
        else
        {
            return topicManager.currentTopic.menu.activeSelf;
        }

    }

    public void SetMainMenuActive(bool value)
    {
        mainUICanvas_Current.SetActive(value);
    }

    public void SetTopicMenuActive(bool value)
    {
        if (topicManager.currentTopic != null)
        {
            topicManager.currentTopic.menu.SetActive(value);
        }
    }


    public void TriggerMenu()
    {
        if (topicManager.currentTopic != null)
        {
            topicManager.currentTopic.TriggerMenu();
        }
    }

    public void DisableMenu()
    {
        if (topicManager.currentTopic != null)
        {
            topicManager.currentTopic.DisableMenu();
        }
    }

    public void SetPenMode(bool active)
    {
        isPenMode = active;
    }

    public void RPCSetTipPointViwe(Vector3 point)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetTipPointViwe", RpcTarget.All, point);
    }

    public void RPCSetTipLinePointViwe(bool value, Vector3 point)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetTipLinePointViwe", RpcTarget.All, value, point);
    }

    public void RPCSetLinePoint(Vector3 point)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetLinePoint", RpcTarget.All, point);
    }

    #endregion

    #region UI

    public void RPCSetPenColor(string colorString)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetPenColor", RpcTarget.All, colorString);
    }

    public void RPCClearCanvas()
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("ClearCanvas", RpcTarget.All);
    }


    

    #endregion

    #region Topic

    public void SetTopic(string topicName)
    {
        topicManager.SetTopic(topicName);

        if(OnSetTopic != null)
        {
            OnSetTopic.Invoke();
        }

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props.Add("CurrentTopic", topicManager.currentTopic.topicName);
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    
    public void ExitTopic()
    {
        topicManager.ExitTopic();

        if (OnExitTopic != null)
        {
            OnExitTopic.Invoke();
        }

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props.Add("CurrentTopic", string.Empty);
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    #endregion

    #region Classroom

    public void InitPhotonInfo()
    {
        PunNetworkManager.ins.maxPlayersPerRoom = (byte)(licenseInformation.seatInfo_Teacher + licenseInformation.seatInfo_Stu);
        PunNetworkManager.ins.SetPhotonAppID(licenseInformation.photonAppID);
    }

    public int GetStudentNumber()
    {
        int count = 0;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].NickName.Contains("Monitor")) continue;
            if (PhotonNetwork.PlayerList[i].IsMasterClient) continue;

            count++;
        }

        return count;
    }

    /// <summary>
    /// 將所有房間資料做 Mapping
    /// </summary>
    public void MappingClassroomProperties()
    {
        ExitGames.Client.Photon.Hashtable temp = PhotonNetwork.CurrentRoom.CustomProperties;

        foreach(DictionaryEntry properties in temp) 
        {
            switch((string)properties.Key)
            {
                case "CurrentTopic":

                    if ((string)properties.Value == currentTopicName) break;

                    if ((string)properties.Value == string.Empty)
                    {
                        if (currentTopicName == "No Topic Selected.") break;

                        currentTopicName = "No Topic Selected.";
                        topicManager.ExitTopic();
                    }
                    else
                    {
                        currentTopicName = (string)properties.Value;

                        topicManager.SetTopic(currentTopicName);
                    }

                    break;

                case "TeachingType":

                    if(this.teachingType != (TeachingType)properties.Value)
                    {
                        if((TeachingType)properties.Value == TeachingType.Guidance)
                        {
                            if(OnSwitchTeachingTypeToGuidance != null)
                            {
                                OnSwitchTeachingTypeToGuidance.Invoke();
                            }
                        }
                        else
                        {
                            if (OnSwitchTeachingTypeToSelfStudy != null)
                            {
                                OnSwitchTeachingTypeToSelfStudy.Invoke();
                            }
                        }

                        this.teachingType = (TeachingType)properties.Value;

                        if (this.teachingType == TeachingType.Guidance)
                        {
                            if (Player.localPlayer != null && Player.localPlayer.isMaster == false)
                            {
                                DisableMenu();
                            }
                        }
                    }

                    break;
            }
        }
    }

    public void SetTeachingType(TeachingType teachingType)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props.Add("TeachingType", teachingType);
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public string GetIdentityInfo()
    {
        return Player.localPlayer.identity.ToString();
    }

    public void ResetClassroom()
    {
        currentTopicName = "No Topic Selected.";
        newMonitorManager.ResetType();
        lineController.PenClear();
    }

    public void RPCMonitorRequest(string requester, string targetNickName)
    {
        if (PhotonNetwork.IsConnected == false) return;
        if (PhotonNetwork.InRoom == false) return;

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("MonitorRequest", PunNetworkManager.GetPhotonPlayerByNickName(targetNickName), requester);
    }

    public void RPCStopMonitorRequest(string requester, string targetNickName)
    {
        if (PhotonNetwork.IsConnected == false) return;
        if (PhotonNetwork.InRoom == false) return;

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("StopMonitorRequest", PunNetworkManager.GetPhotonPlayerByNickName(targetNickName), requester);
    }

    public void RPCSendMonitorData(byte[] viewData)
    {
        if (PhotonNetwork.IsConnected == false) return;
        if (PhotonNetwork.InRoom == false) return;
    }

    public void RPCSendMonitorPointerClick()
    {
        if (PhotonNetwork.IsConnected == false) return;
        if (PhotonNetwork.InRoom == false) return;

        PhotonView photonView = PhotonView.Get(this);

        for (int i = 0; i < newMonitorManager.monitorReceivers.Count; i++)
        {
            photonView.RPC("SendMoniterPointerClick", PunNetworkManager.GetPhotonPlayerByNickName(newMonitorManager.monitorReceivers[i]));
        }
    }

    
    //更新當前PhotonNetworkManager Loading 等級
    public void RPCUpdatePunLoadingLevel()
    {
        PunNetworkManager.LoadingLevel targetLevel = PunNetworkManager.LoadingLevel.Easy;

        int playerCount = PhotonNetwork.CurrentRoom.Players.Count;

        switch(playerCount)
        {
            case > 60:
                targetLevel = PunNetworkManager.LoadingLevel.VeryHard;

                break;

            case > 40:
                targetLevel = PunNetworkManager.LoadingLevel.Hard;

                break;

            case > 20:
                targetLevel = PunNetworkManager.LoadingLevel.Normal;

                break;

            case > 0:
                targetLevel = PunNetworkManager.LoadingLevel.Easy;

                break;
        }


        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetLoadingLevel", RpcTarget.All, targetLevel);
    }


    public void CheckListence()
    {
        if (PlayerPrefs.HasKey(LICENCE_KEY) && PlayerPrefs.GetString(LICENCE_KEY) != string.Empty)
        {
            SetLicense(PlayerPrefs.GetString(LICENCE_KEY));
            firebaseLicenseInfoManager.ReadData(OnPassLicense, OnFailLicense);
        }
        else
        {
            OnFailLicense();
        }
    }


    public void SetLicense(string key)
    {
        PlayerPrefs.SetString("LicenseKey", key);
        firebaseLicenseInfoManager.SetDocumentID(key);
    }

    private void OnPassLicense()
    {
        Debug.Log("ClassroomManager OnPassLicense");

        if(OnPassLicenseEvent != null)
        {
            OnPassLicenseEvent.Invoke();
        }    
    }

    private void OnFailLicense()
    {
        Debug.Log("ClassroomManager OnFailLicense");

        if (OnFailLicenseEvent != null)
        {
            OnFailLicenseEvent.Invoke();
        }
    }

    #endregion

    #endregion

    #region RPC Method 

    [PunRPC]
    void SetTipPointViwe(Vector3 point)
    {
        if (Player.localPlayer == null) return;

        Player.localPlayer.gazeController.SetTipPointView(point);
    }

    [PunRPC]
    void SetTipLinePointViwe(bool value, Vector3 point)
    {
        if (Player.localPlayer == null) return;

        if (value)
        {
            lineController.moving = true;
            lineController.pos = point;
            lineController.InitLine();
            //Debug.Log("InitLine");
        }
        else
        {
            lineController.moving = false;
            lineController.pos = new Vector3();
            lineController.worldPos.Clear();
            //Debug.Log("Clear");
        }
    }

    [PunRPC]
    void SetLinePoint(Vector3 point)
    {
        if (Player.localPlayer == null) return;

        lineController.pos = point;
    }

    [PunRPC]
    void SetPenColor(string colorString)
    {
        if (Player.localPlayer == null) return;

        switch (colorString)
        {
            case "black":
                lineController.penColor = Color.black;
                break;

            case "red":
                lineController.penColor = Color.red;
                break;

            case "green":
                lineController.penColor = Color.green;
                break;

            case "white":
                lineController.penColor = Color.white;
                break;

        }
    }

    [PunRPC]
    void ClearCanvas()
    {
        if (Player.localPlayer == null) return;

        lineController.PenClear();
    }

    [PunRPC]
    void MonitorRequest(string requester)
    {
        newMonitorManager.StartSend(requester);
    }

    [PunRPC]
    void StopMonitorRequest(string requester)
    {
        newMonitorManager.StopSend(requester);
    }

    [PunRPC]
    void SendMoniterPointerClick()
    {
        newMonitorManager.SetMousePointerClick();
    }

    [PunRPC]
    void SetLoadingLevel(PunNetworkManager.LoadingLevel targetLevel)
    {
        PunNetworkManager.ins.SetLoadingLevel(targetLevel);
    }


    #endregion

}
