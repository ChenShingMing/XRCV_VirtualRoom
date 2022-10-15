using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;
using UnityEngine.UI;

public class NumberGameController : TopicController
{
    [ReadOnly]
    public int score;

    [FoldoutGroup("UI")]
    public Text scoreText;

    private int selfStudyScore;


    public override void OnEnable()
    {
        base.OnEnable();
        selfStudyScore = score;
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public void Update()
    {
        scoreText.text = score.ToString();
    }

    public void AddScore()
    {
        score += 1;
    }

    public void MinusScore()
    {
        score -= 1;
    }



    public override void SendTopicControllerProperties()
    {
        props.Clear();
        props.Add(topic.topicName + "_score_" + Player.localPlayer.playerName, this.score);

        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public override void UpdateTopicControllerProperties()
    {
        ExitGames.Client.Photon.Hashtable temp = PhotonNetwork.CurrentRoom.CustomProperties;
        this.score = (int)temp[topic.topicName + "_score_" + topic.syncSenderName];
    }

    public override void OnSwitchTeachingToGuidance()
    {
        selfStudyScore = score;
        Debug.Log("切換成導學模式，紀錄當前score");
    }

    public override void OnSwitchTeachingToSelfStudy()
    {
        score = selfStudyScore;
        Debug.Log("切換成自學模式，恢復score至上次自學時");
    }
}
