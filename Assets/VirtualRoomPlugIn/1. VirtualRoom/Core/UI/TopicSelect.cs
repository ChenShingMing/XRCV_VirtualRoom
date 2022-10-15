using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TopicSelect : MonoBehaviour
{
    public TMP_Text no_Text;
    public TMP_Text topicName_Text;

    public int no;
    public string topicName;

    private void FixedUpdate()
    {
        no_Text.text = no.ToString();
        topicName_Text.text = topicName;
    }

}
