using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class ESInfoCanvasUI : MonoBehaviour
{
    [BoxGroup("物件設置")]
    public Text month;
    [BoxGroup("物件設置")]
    public Text mode;
    [BoxGroup("物件設置")]
    public Text camPos;

    public float panelYOffset = 2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 rot = new Vector3(0, Camera.main.transform.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rot), 10 * Time.deltaTime);
        transform.position = Camera.main.transform.position - new Vector3(0, panelYOffset, 0);

        if (ESController.ins.desplayMode == ESController.DesplayMode.Ecliptic)
            month.text = ESController.ins.month.ToString() + "月";
        else
            month.text = "";


        switch (ESController.ins.desplayMode)
        {
            case ESController.DesplayMode.Ecliptic:
                mode.text = "黃道";
                break;

            case ESController.DesplayMode.Seasons:
                mode.text = "四季";
                break;
        }

        switch (ESController.ins.seasonsCamPos)
        {
            case ESController.SeasonsCamPos.Default:
                camPos.text = "黃道";
                break;

            case ESController.SeasonsCamPos.Spring:
                camPos.text = "春季";
                break;
            case ESController.SeasonsCamPos.Sumemr:
                camPos.text = "夏季";
                break;
            case ESController.SeasonsCamPos.Full:
                camPos.text = "秋季";
                break;
            case ESController.SeasonsCamPos.Winter:
                camPos.text = "冬季";
                break;
        }

    }
}
