using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

public class RoomInformation : MonoBehaviour
{
    [FoldoutGroup("����]�m")]
    public TMP_Text no_text;
    [FoldoutGroup("����]�m")]
    public TMP_Text roomName_text;
    [FoldoutGroup("����]�m")]
    public TMP_Text curremtTopic_text;
    [FoldoutGroup("����]�m")]
    public TMP_Text teacherName_text;
    [FoldoutGroup("����]�m")]
    public TMP_Text memberNum_text;

    [FoldoutGroup("�ѼƳ]�m")]
    public int no;
    [FoldoutGroup("�ѼƳ]�m")]
    public string roomName;
    [FoldoutGroup("�ѼƳ]�m")]
    public string curremtTopic;
    [FoldoutGroup("�ѼƳ]�m")]
    public string teacherName;
    [FoldoutGroup("�ѼƳ]�m")]
    public int playerCount;
    [FoldoutGroup("�ѼƳ]�m")]
    public int maxPlayers;

    private float _timer;
    private const float REFRESH_INTERVAL = 0.5f;

    private void FixedUpdate()
    {
        _timer += Time.fixedDeltaTime;
        if (_timer < REFRESH_INTERVAL) return;
        _timer = 0f;

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
