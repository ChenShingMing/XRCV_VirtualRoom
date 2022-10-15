using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

public class RoomInformation : MonoBehaviour
{
    [FoldoutGroup("ン砞竚")]
    public TMP_Text no_text;
    [FoldoutGroup("ン砞竚")]
    public TMP_Text roomName_text;
    [FoldoutGroup("ン砞竚")]
    public TMP_Text curremtTopic_text;
    [FoldoutGroup("ン砞竚")]
    public TMP_Text teacherName_text;
    [FoldoutGroup("ン砞竚")]
    public TMP_Text memberNum_text;

    [FoldoutGroup("把计砞竚")]
    public int no;
    [FoldoutGroup("把计砞竚")]
    public string roomName;
    [FoldoutGroup("把计砞竚")]
    public string curremtTopic;
    [FoldoutGroup("把计砞竚")]
    public string teacherName;
    [FoldoutGroup("把计砞竚")]
    public int playerCount;
    [FoldoutGroup("把计砞竚")]
    public int maxPlayers;

    private void FixedUpdate()
    {
        no_text.text = no.ToString();
        roomName_text.text = roomName;

        if (curremtTopic != string.Empty)
        {
            curremtTopic_text.text = curremtTopic;
        }
        else
        {
            curremtTopic_text.text = "No Topic Selected.";
        }
        
        teacherName_text.text = teacherName;
        memberNum_text.text = playerCount.ToString() + "/" + maxPlayers.ToString();
    }

}
