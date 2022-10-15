using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Linq;

public class LobbyPanel : MonoBehaviour
{
    [FoldoutGroup("物件設置")]
    public GameObject lobby;
    [FoldoutGroup("物件設置")]
    public GameObject createRoom;
    [FoldoutGroup("物件設置")]
    public GameObject enterRoomFail;
    [FoldoutGroup("物件設置")]
    public GameObject maxRoomTip;


    [FoldoutGroup("物件設置")]
    public Transform scrollViewContentParent;
    [FoldoutGroup("物件設置")]
    public ToggleGroup scrollToggleGroup;


    [FoldoutGroup("物件設置")]
    public GameObject roomTemplate;

    private IEnumerable<Toggle> activeRooms; // 所有的room
    private RoomInformation activeRoom; //當前選中的item


    public List<RoomInformation> roomInformations;


    private void OnEnable()
    {
        ReflashRoomList();

        lobby.SetActive(true);
        createRoom.SetActive(false);
        enterRoomFail.SetActive(false);
    }

    public void ReflashRoomList()
    {
        //先補齊項目的數量
        UpdateRoomListCount();

        int count = 0;
        foreach (KeyValuePair<string, RoomInfo> item in PunNetworkManager.ins.cachedRoomList)
        {
            roomInformations[count].no = count + 1;
            roomInformations[count].roomName = item.Value.Name;
            roomInformations[count].curremtTopic = (string)item.Value.CustomProperties["CurrentTopic"];
            roomInformations[count].teacherName = (string)item.Value.CustomProperties["TeacherNickName"];
            roomInformations[count].playerCount = item.Value.PlayerCount;
            roomInformations[count].maxPlayers = item.Value.MaxPlayers;

            count++;
        }
    }

    public void OnToggleSelect(bool toggleValue)
    {
        if (toggleValue == false)
        {
            activeRoom = null;
            return;
        }

        //將選重的資料填上InputField
        activeRooms = scrollToggleGroup.ActiveToggles();
        activeRoom = activeRooms.ElementAt(0).GetComponent<RoomInformation>();
    }

    public string GetSelectRoom()
    {
        if(activeRoom != null)
        {
            return activeRoom.roomName;
        }

        return null;
    }

    public void ShowMaxRoomTip()
    {
        lobby.SetActive(false);
        createRoom.SetActive(false);
        maxRoomTip.SetActive(true);
    }



    void UpdateRoomListCount()
    {
        //Debug.Log("UpdateRoomListCount " + roomInformations.Count + " " + PunNetworkManager.ins.cachedRoomList.Count);

        while (roomInformations.Count != PunNetworkManager.ins.cachedRoomList.Count)
        {
            if(roomInformations.Count > PunNetworkManager.ins.cachedRoomList.Count)
            {
                Destroy(scrollViewContentParent.GetChild(scrollViewContentParent.childCount - 1).gameObject);
                roomInformations.RemoveAt(roomInformations.Count-1);
            }

            if (roomInformations.Count < PunNetworkManager.ins.cachedRoomList.Count)
            {
                GameObject temp = Instantiate(roomTemplate, scrollViewContentParent, false);
                //temp.transform.SetParent(scrollViewContentParent);
                //temp.transform.localPosition = new Vector3(temp.transform.localPosition.x, temp.transform.localPosition.y, 0);
                //temp.transform.localRotation = Quaternion.identity;
                temp.SetActive(true);

                roomInformations.Add(temp.GetComponent<RoomInformation>());
            }
        }
    }

}
