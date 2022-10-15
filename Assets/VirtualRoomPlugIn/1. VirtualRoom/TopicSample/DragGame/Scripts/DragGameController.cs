using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;
using UnityEngine.UI;

public class DragGameController : TopicController
{
    [ReadOnly]
    public Vector3 boxPosition;

    public DragDrop box;
    public Text posText;

    Vector3 selfStudyBoxPosition;

    public override void OnEnable()
    {
        base.OnEnable();

        boxPosition = new Vector3(0, -1.5f, 3);
        box.transform.position = boxPosition;
        selfStudyBoxPosition = boxPosition;
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log("boxPosition :" + boxPosition + " Time : " + Time.time);
        box.transform.position = Vector3.Lerp(box.transform.position, boxPosition, 20f * Time.deltaTime);
        posText.text = boxPosition.ToString();

        BoxDragHandle();
    }

    public void SetPos(Vector3 vector3)
    {
        //Debug.Log("SetPos :" + vector3 + " Time : " + Time.time);
        boxPosition = vector3;
    }

    private void BoxDragHandle()
    {
        if (Player.localPlayer.isMaster) //如果是老師
        {
            //就觸發選單功能
            box.dragEnable = true;
        }
        else //是學生
        {
            if (ClassroomManager.ins.teachingType == ClassroomManager.TeachingType.SelfStudy //如果在自學
                && Player.localPlayer.identity == Player.Identity.Student) //而且是學生
            {
                box.dragEnable = true;
            }
            else
            {
                box.dragEnable = false;
            }
        }

    }

    public override void SendTopicControllerProperties()
    {
        props.Clear();
        props.Add(topic.topicName + "_boxPosition_" + Player.localPlayer.playerName, this.boxPosition);

        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public override void UpdateTopicControllerProperties()
    {
        ExitGames.Client.Photon.Hashtable temp = PhotonNetwork.CurrentRoom.CustomProperties;
        this.boxPosition = (Vector3)temp[topic.topicName + "_boxPosition_" + topic.syncSenderName];
    }

    public override void OnSwitchTeachingToGuidance()
    {
        selfStudyBoxPosition = boxPosition;
        Debug.Log("切換成導學模式，紀錄當前Box座標");
    }

    public override void OnSwitchTeachingToSelfStudy()
    {
        boxPosition = selfStudyBoxPosition;
        Debug.Log("切換成自學模式，恢復Box座標至上次自學時");
    }
}
