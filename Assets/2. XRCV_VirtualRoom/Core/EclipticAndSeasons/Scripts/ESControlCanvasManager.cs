using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class ESControlCanvasManager : MonoBehaviour
{
    [FoldoutGroup("物件設置")]
    public ESController controller;
    [FoldoutGroup("物件設置")]
    public ESInfoCanvasUI infoCanvasUI;
    [FoldoutGroup("物件設置")]
    public Toggle autoOrbit;
    [FoldoutGroup("物件設置")]
    public Text monthUI;
    [FoldoutGroup("物件設置")]
    public Toggle desplayMode_E;
    [FoldoutGroup("物件設置")]
    public Toggle desplayMode_S;
    [FoldoutGroup("物件設置")]
    public Toggle seasonCamPos_Default;
    [FoldoutGroup("物件設置")]
    public Toggle seasonCamPos_Spring;
    [FoldoutGroup("物件設置")]
    public Toggle seasonCamPos_Summer;
    [FoldoutGroup("物件設置")]
    public Toggle seasonCamPos_Full;
    [FoldoutGroup("物件設置")]
    public Toggle seasonCamPos_Winter;

    private void OnEnable()
    {
        autoOrbit.isOn = controller.autoOrbit;

        if(controller.desplayMode == ESController.DesplayMode.Ecliptic)
        {
            desplayMode_E.isOn = true;
        }
        else
        {
            desplayMode_S.isOn = true;
        }

        switch(controller.seasonsCamPos)
        {
            case ESController.SeasonsCamPos.Default:
                seasonCamPos_Default.isOn = true;
                break;

            case ESController.SeasonsCamPos.Spring:
                seasonCamPos_Spring.isOn = true;
                break;

            case ESController.SeasonsCamPos.Sumemr:
                seasonCamPos_Summer.isOn = true;
                break;

            case ESController.SeasonsCamPos.Full:
                seasonCamPos_Full.isOn = true;
                break;

            case ESController.SeasonsCamPos.Winter:
                seasonCamPos_Winter.isOn = true;
                break;
        }
    }

    public void Trigger()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        SetControlCanvas(gameObject.activeSelf);
    }
    public void SetControlCanvas(bool value)
    {
        if (value)
        {
            Vector3 rot = new Vector3(0, Camera.main.transform.rotation.eulerAngles.y, 0);
            transform.position = Camera.main.transform.position - new Vector3(0, 1, 0);
            transform.rotation = Quaternion.Euler(rot);

            this.gameObject.SetActive(value);
            infoCanvasUI.gameObject.SetActive(false);
        }
        else
        {
            Vector3 rot = new Vector3(0, Camera.main.transform.rotation.eulerAngles.y, 0);
            infoCanvasUI.transform.rotation = Quaternion.Euler(rot);
            infoCanvasUI.gameObject.SetActive(true);
        }
    }

    //給淡出動畫使用
    public void SetCanvasDeactive()
    {
        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        monthUI.text = controller.month.ToString();
    }

    public void SetAutoOrbit(bool _value)
    {
        controller.autoOrbit = _value;
    }

    public void AddMonth()
    {
        controller.AddMonth();
    }

    public void MinusMonth()
    {
        controller.MinusMonth();
    }

    public void SetDesplay_Ecliptic(bool _value)
    {
        if (_value == false) return;
        controller.SetDesplayMode(ESController.DesplayMode.Ecliptic);
        controller.TriggerControlCanvas();
    }

    public void SetDesplay_Season(bool _value)
    {
        if (_value == false) return;
        controller.SetDesplayMode(ESController.DesplayMode.Seasons);
        controller.TriggerControlCanvas();
    }

    public void SetSeasonCamPos_Default(bool _value)
    {
        if (_value == false) return;
        controller.SetSeasonCamPos(ESController.SeasonsCamPos.Default);
        controller.TriggerControlCanvas();
    }

    public void SetSeasonCamPos_Spring(bool _value)
    {
        if (_value == false) return;
        controller.SetSeasonCamPos(ESController.SeasonsCamPos.Spring);
        controller.TriggerControlCanvas();
    }

    public void SetSeasonCamPos_Summer(bool _value)
    {
        if (_value == false) return;
        controller.SetSeasonCamPos(ESController.SeasonsCamPos.Sumemr);
        controller.TriggerControlCanvas();
    }

    public void SetSeasonCamPos_Full(bool _value)
    {
        if (_value == false) return;
        controller.SetSeasonCamPos(ESController.SeasonsCamPos.Full);
        controller.TriggerControlCanvas();
    }

    public void SetSeasonCamPos_Winter(bool _value)
    {
        if (_value == false) return;
        controller.SetSeasonCamPos(ESController.SeasonsCamPos.Winter);
        controller.TriggerControlCanvas();
    }

}
