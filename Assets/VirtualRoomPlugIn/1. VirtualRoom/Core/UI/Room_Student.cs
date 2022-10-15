using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;

public class Room_Student : MonoBehaviour
{
    [FoldoutGroup("Student")]
    public TMP_Text roomName_Text;
    [FoldoutGroup("Student")]
    public TMP_Text teacherName_Text;
    [FoldoutGroup("Student")]
    public TMP_Text memberNum_Text;
    [FoldoutGroup("Student")]
    public TMP_Text currentTopic_Text;
    [FoldoutGroup("Student")]
    public TMP_Text teachingType_Text;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        roomName_Text.text = PhotonNetwork.CurrentRoom.Name;
        teacherName_Text.text = (string)PhotonNetwork.CurrentRoom.CustomProperties["TeacherNickName"];
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

        ClassroomManager.TeachingType teachingType = (ClassroomManager.TeachingType)PhotonNetwork.CurrentRoom.CustomProperties["TeachingType"];

        if (teachingType == ClassroomManager.TeachingType.Guidance)
        {
            teachingType_Text.text = "導學模式";
        }
        else if(teachingType == ClassroomManager.TeachingType.SelfStudy)
        {
            teachingType_Text.text = "自學模式";
        }
    }
}
