using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;

public class Room_Monitor : MonoBehaviour
{
    [FoldoutGroup("Monitor")]
    public TMP_Text roomName_Text;
    [FoldoutGroup("Monitor")]
    public TMP_Text memberNum_Text;
    [FoldoutGroup("Monitor")]
    public TMP_Text currentTarget_Text;

    [FoldoutGroup("TargetSelect")]
    public Transform selectParent;
    [FoldoutGroup("TargetSelect")]
    public GameObject selectTemplate;
    [FoldoutGroup("TargetSelect")]
    public ToggleGroup selectToggleGroup;

    IEnumerable<Toggle> activeSelects;
    MonitorSelect activeSelect;

    List<MonitorSelect> monitorSelects = new List<MonitorSelect>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        roomName_Text.text = PhotonNetwork.CurrentRoom.Name;
        memberNum_Text.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();

        string targetName = ClassroomManager.ins.newMonitorManager.senderPlayerName;

        if(targetName != string.Empty)
        {
            currentTarget_Text.text = targetName;
        }
        else
        {
            currentTarget_Text.text = "尚未選擇";
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
        activeSelects = selectToggleGroup.ActiveToggles();
        activeSelect = activeSelects.ElementAt(0).GetComponent<MonitorSelect>();
    }

    public string GetSelect()
    {
        if (activeSelect != null)
        {
            return activeSelect.targetName;
        }

        return null;
    }

    public void ReflashTargetSelect()
    {
        UpdateTargetListCount();

        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            monitorSelects[i].no = i + 1;
            monitorSelects[i].targetName = PhotonNetwork.PlayerList[i].NickName;
        }
    }

    void UpdateTargetListCount()
    {
        while (monitorSelects.Count != PhotonNetwork.PlayerList.Length)
        {
            if (monitorSelects.Count > PhotonNetwork.PlayerList.Length)
            {
                Destroy(selectParent.GetChild(selectParent.childCount - 1).gameObject);
                monitorSelects.RemoveAt(monitorSelects.Count - 1);
            }

            if (monitorSelects.Count < PhotonNetwork.PlayerList.Length)
            {
                GameObject temp = Instantiate(selectTemplate, selectParent, false);
                //temp.transform.SetParent(selectParent);
                //temp.transform.localPosition = new Vector3(temp.transform.localPosition.x, temp.transform.localPosition.y, 0);
                //temp.transform.localRotation = Quaternion.identity;
                temp.SetActive(true);

                monitorSelects.Add(temp.GetComponent<MonitorSelect>());
            }
        }
    }
}
