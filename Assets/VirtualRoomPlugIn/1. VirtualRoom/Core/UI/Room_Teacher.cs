using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;

public class Room_Teacher : MonoBehaviour
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
        ReflashTopicSelect();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        roomName_Text.text = PhotonNetwork.CurrentRoom.Name;
        memberNum_Text.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();

        string currentTopic = (string)PhotonNetwork.CurrentRoom.CustomProperties["CurrentTopic"];

        if (currentTopic == null || currentTopic == string.Empty)
        {
            currentTopic_Text.text = "No Topic Selected.";
        }
        else
        {
            currentTopic_Text.text = currentTopic;
        }

    }


    public void OnToggleSelect(bool toggleValue)
    {
        if (toggleValue == false)
        {
            activeSelect = null;
            return;
        }

        //將選重的資料填上InputField
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
