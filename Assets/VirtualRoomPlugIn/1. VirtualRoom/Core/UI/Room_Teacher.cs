using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;

public class Room_Teacher : MonoBehaviourPunCallbacks
{
    [FoldoutGroup("Teacher")]
    public TMP_Text roomName_Text;
    [FoldoutGroup("Teacher")]
    public TMP_Text memberNum_Text;
    [FoldoutGroup("Teacher")]
    public TMP_Text currentTopic_Text;

    [FoldoutGroup("Topic")]
    public Transform topicSelectParent;
    [FoldoutGroup("Topic")]
    public GameObject topicSelectTemplate;
    [FoldoutGroup("Topic")]
    public ToggleGroup topicToggleGroup;

    List<TopicSelect> topicSelects = new List<TopicSelect>();

    IEnumerable<Toggle> activeSelects;
    TopicSelect activeSelect;


    private void OnEnable()
    {
        base.OnEnable();
        ReflashTopicSelect();
        Refresh();
    }

    public void Refresh()
    {
        if (PhotonNetwork.CurrentRoom == null) return;
        roomName_Text.text = PhotonNetwork.CurrentRoom.Name;
        memberNum_Text.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();

        string currentTopic = (string)PhotonNetwork.CurrentRoom.CustomProperties["CurrentTopic"];
        currentTopic_Text.text = (currentTopic == null || currentTopic == string.Empty)
            ? "No Topic Selected."
            : currentTopic;
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Refresh();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Refresh();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Refresh();
    }


    public void OnToggleSelect(bool toggleValue)
    {
        if (toggleValue == false)
        {
            activeSelect = null;
            return;
        }

        //�N�ﭫ����ƶ�WInputField
        activeSelects = topicToggleGroup.ActiveToggles();
        activeSelect = activeSelects.ElementAt(0).GetComponent<TopicSelect>();
    }

    public string GetSelect()
    {
        if (activeSelect != null)
        {
            return activeSelect.topicName;
        }

        return null;
    }

    void ReflashTopicSelect()
    {
        UpdateTopicListCount();

        int count = 0;
        foreach (KeyValuePair<string, Topic> item in ClassroomManager.ins.topicManager.topicDic)
        {
            topicSelects[count].no = count + 1;
            topicSelects[count].topicName = item.Key;

            count++;
        }

    }

    void UpdateTopicListCount()
    {
        while (topicSelects.Count != ClassroomManager.ins.topicManager.topicDic.Count)
        {
            if (topicSelects.Count > ClassroomManager.ins.topicManager.topicDic.Count)
            {
                Destroy(topicSelectParent.GetChild(topicSelectParent.childCount - 1).gameObject);
                topicSelects.RemoveAt(topicSelects.Count - 1);
            }

            if (topicSelects.Count < ClassroomManager.ins.topicManager.topicDic.Count)
            {
                GameObject temp = Instantiate(topicSelectTemplate, topicSelectParent, false);
                //temp.transform.SetParent(topicSelectParent);
                //temp.transform.localPosition = new Vector3(temp.transform.localPosition.x, temp.transform.localPosition.y, 0);
                //temp.transform.localRotation = Quaternion.identity;
                temp.SetActive(true);

                topicSelects.Add(temp.GetComponent<TopicSelect>());
            }
        }
    }
}
