using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Photon.Pun;
using System;

public class NewMonitorManager : MonoBehaviour
{
    public enum PlayerGazeMode
    {
        None,
        Teacher,
        Student,
    }

    public enum Type
    {
        None,
        Sender,
        Receiver,
    }

    public Type type = Type.None;
    public string senderPlayerName;
    public Pointer pointer;
    
    public List<string> monitorReceivers = new List<string>();

    private Transform mainCamTransform;
    private Camera _mainCamera;
    private ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
    private List<GazeController> _cachedGazeControllers;
    private float _sendTimer;

    private void Awake()
    {
        _mainCamera = Camera.main;
        mainCamTransform = _mainCamera.transform;
    }

    // Update is called once per frame
    void Update()
    {
        MonitorHandle();
    }

    #region Public 

    public void StartReceive(string sender)
    {
        if(senderPlayerName != string.Empty)
        {
            ClassroomManager.ins.RPCStopMonitorRequest(Player.localPlayer.playerName, senderPlayerName);
        }

        senderPlayerName = sender;
        ClassroomManager.ins.RPCMonitorRequest(Player.localPlayer.playerName, senderPlayerName);

        if (Player.GetIdentityByName(sender) == Player.Identity.Teacher)
        {
            SetPlayerGazeMode(PlayerGazeMode.Teacher);
        }
        else if(Player.GetIdentityByName(sender) == Player.Identity.Student)
        {
            SetPlayerGazeMode(PlayerGazeMode.Student);
        }
        
        SetType(Type.Receiver);
    }

    public void StopReceive()
    {
        StopReceive(senderPlayerName);
    }

    public void StopReceive(string sender)
    {
        ClassroomManager.ins.RPCStopMonitorRequest(Player.localPlayer.playerName, sender);
        SetPlayerGazeMode(PlayerGazeMode.Student);
        senderPlayerName = string.Empty;
        SetType(Type.None);
    }

    public void StartSend(string reseiver)
    {
        if (monitorReceivers.Contains(reseiver)) return;

        monitorReceivers.Add(reseiver);

        if(type != Type.Sender)
        {
            SetType(Type.Sender);
        }
    }

    public void StopSend(string reseiver)
    {
        if (monitorReceivers.Contains(reseiver) == false) return;

        monitorReceivers.Remove(reseiver);

        if (monitorReceivers.Count == 0)
        {
            SetType(Type.None);
        }
    }

    public void ResetType()
    {
        SetType(Type.None);
    }

    /// <summary>
    /// 傳入一個Vecter3，其中x和y均為0~1的值，z為0
    /// </summary>
    /// <param name="pos"></param>
    public void SetMousePointer(Vector3 pos)
    {
        pos = new Vector3(pos.x * Screen.width, pos.y * Screen.height, 0);
        pointer.pointerPos = pos;
    }

    public void SetMousePointerClick()
    {
        pointer.Click();
    }

    #endregion

    #region Private

    private void SetType(Type type)
    {
        switch (type)
        {
            case Type.None:
                pointer.gameObject.SetActive(false);
                break;

            case Type.Sender:
                pointer.gameObject.SetActive(false);
                break;

            case Type.Receiver:
                pointer.gameObject.SetActive(true);
                break;
        }

        this.type = type;
    }

    private void MonitorHandle()
    {
        switch (type)
        {
            case Type.None:
                //不做事
                break;

            case Type.Sender:

                SendHandle();

                break;

            case Type.Receiver:

                ReceiveHandle();

                break;
        }
    }

    private void SendHandle()
    {
        _sendTimer += Time.deltaTime;
        if (_sendTimer < 0.1f) return;
        _sendTimer = 0f;

        props.Clear();
        props.Add(Player.localPlayer.playerName + "_MainCameraTransform_Position", mainCamTransform.position);
        props.Add(Player.localPlayer.playerName + "_MainCameraTransform_Rotation", mainCamTransform.rotation);
        props.Add(Player.localPlayer.playerName + "_TopicMenuActive", ClassroomManager.ins.GetCurrentTopicMenuActive());

        if (ClassroomManager.ins != null)
        {
            Vector3 pointWorldPos = ClassroomManager.ins.inputActionManager.GetInputPointerOnGazeSphere();
            Vector3 pointViewPos = _mainCamera.WorldToViewportPoint(pointWorldPos);
            props.Add(Player.localPlayer.playerName + "_MonitorPointerPos", new Vector3(pointViewPos.x, pointViewPos.y, 0));
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    private void ReceiveHandle()
    {
        ExitGames.Client.Photon.Hashtable temp = PhotonNetwork.CurrentRoom.CustomProperties;

        try
        {
            //相機位置
            mainCamTransform.position = Vector3.Lerp(mainCamTransform.position
                                                    , (Vector3)temp[senderPlayerName + "_MainCameraTransform_Position"]
                                                    , Time.deltaTime * 15);
            //相機旋轉
            mainCamTransform.rotation = Quaternion.Lerp(mainCamTransform.rotation
                                                    , (Quaternion)temp[senderPlayerName + "_MainCameraTransform_Rotation"]
                                                    , Time.deltaTime * 15);

            //Topic選單
            ClassroomManager.ins.SetTopicMenuActive((bool)temp[senderPlayerName + "_TopicMenuActive"]);
            //Topic內需要同步的變數同步成sender的
            ClassroomManager.ins.topicManager.SetSyncSender(senderPlayerName);

            //同步鼠標位置
            ClassroomManager.ins.newMonitorManager.SetMousePointer((Vector3)temp[senderPlayerName + "_MonitorPointerPos"]);
        }
        catch(Exception e)
        {
            Debug.LogWarning(e, this);
        }

    }

    public void InvalidateGazeControllerCache()
    {
        _cachedGazeControllers = null;
    }

    private void SetPlayerGazeMode(PlayerGazeMode mode)
    {
        if (_cachedGazeControllers == null)
            _cachedGazeControllers = FindObjectsOfTypeAll<GazeController>();
        List<GazeController> gazeControllers = _cachedGazeControllers;

        switch (mode)
        {
            case PlayerGazeMode.None:

                //不做事

                break;

            case PlayerGazeMode.Teacher:

                for(int i = 0; i < gazeControllers.Count; i++)
                {
                    switch(gazeControllers[i].pointViewType)
                    {
                        case GazeController.PointViewType.None:

                            gazeControllers[i].SetPointViewActive(false);

                            break;

                        case GazeController.PointViewType.Teacher:

                            gazeControllers[i].SetPointViewActive(false);

                            break;

                        case GazeController.PointViewType.Student:

                            gazeControllers[i].SetPointViewActive(true);

                            break;
                    }
                }

                break;

            case PlayerGazeMode.Student:

                for (int i = 0; i < gazeControllers.Count; i++)
                {
                    switch (gazeControllers[i].pointViewType)
                    {
                        case GazeController.PointViewType.None:

                            gazeControllers[i].SetPointViewActive(false);

                            break;

                        case GazeController.PointViewType.Teacher:

                            gazeControllers[i].SetPointViewActive(true);

                            break;

                        case GazeController.PointViewType.Student:

                            gazeControllers[i].SetPointViewActive(false);

                            break;
                    }
                }


                break;

        }
    }

    #endregion


    public List<T> FindObjectsOfTypeAll<T>()
    {
        List<T> results = new List<T>();
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    var go = allGameObjects[j];
                    results.AddRange(go.GetComponentsInChildren<T>(true));
                }
            }
        }
        return results;
    }
}
