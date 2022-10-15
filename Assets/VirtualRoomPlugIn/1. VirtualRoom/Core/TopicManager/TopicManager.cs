using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;

public class TopicManager : SerializedMonoBehaviour
{
    [ReadOnly]
    public Topic currentTopic;

    [ReadOnly]
    public string topicName;

    public float syncFrequency = 0.5f;

    public Dictionary<string, Topic> topicDic = new Dictionary<string, Topic>();

    private void Awake()
    {
        foreach (KeyValuePair<string, Topic> item in topicDic)
        {
            item.Value.OnInit();
        }

        PunNetworkManager.ins.OnDisconnectEvent.AddListener(ExitTopic);
        InvokeRepeating("TopicSyncHandle", syncFrequency, syncFrequency);
    }

    private void Update()
    {
        TopicHandle();
    }


    #region Topic

    public void TopicHandle()
    {
        if (currentTopic != null)
        {
            currentTopic.OnUpdate();
        }
    }

    public void TopicSyncHandle()
    {
        if (currentTopic != null)
        {
            currentTopic.OnSync();
        }
    }


    [FoldoutGroup("功能")]
    [Button]
    public void SetTopic(string topicName)
    {
        if (currentTopic != null)
        {
            if (currentTopic.topicName == topicName)
            {
                return;
            }
            else
            {
                ExitTopic();
            }
        }



        this.topicName = topicName;
        currentTopic = topicDic[topicName];
        currentTopic.OnEnter();
    }

    [FoldoutGroup("功能")]
    [Button]
    public void ExitTopic()
    {
        if (currentTopic != null)
        {
            currentTopic.OnExit();
        }

        this.topicName = string.Empty;

        currentTopic = null;
    }

    public void SetSyncSender(string sender)
    {
        if (currentTopic == null) return;

        currentTopic.SetSyncSenderName(sender);
    }

    #endregion
}
