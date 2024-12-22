using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;


public class ESController : TMControllerBase
{
    public static ESController ins;

    public enum DesplayMode
    {
        Ecliptic,
        Seasons
    }

    public enum SeasonsCamPos
    {
        Default,
        Spring,
        Sumemr,
        Full,
        Winter
    }

    [FoldoutGroup("設置")]
    public Transform pcCamRoot;
    [FoldoutGroup("設置")]
    public Transform openXRCamRoot;

    [FoldoutGroup("設置")]
    public ESControlCanvasManager controlCanvasManager;
    [FoldoutGroup("設置")]
    public Material mySkyBoxMat;
    [FoldoutGroup("設置")]
    public EarthOrbit earthOrbit;
    [FoldoutGroup("設置")]
    public GameObject eclipticEarth;
    [FoldoutGroup("設置")]
    public GameObject seasonsEarth;
    [FoldoutGroup("設置")]
    public Transform camPos_Default;
    [FoldoutGroup("設置")]
    public Transform camPos_Spring;
    [FoldoutGroup("設置")]
    public Transform camPos_Summer;
    [FoldoutGroup("設置")]
    public Transform camPos_Full;
    [FoldoutGroup("設置")]
    public Transform camPos_Winter;

    [FoldoutGroup("Controller")]
    [InlineButton("MinusMonth", "-")]
    [InlineButton("AddMonth", "+")]
    public int month;

    [FoldoutGroup("Controller")]
    public bool autoOrbit;

    [FoldoutGroup("Controller")]
    [OnValueChanged("SetDesplayModeEditor")]
    public DesplayMode desplayMode = DesplayMode.Ecliptic;

    [FoldoutGroup("Controller")]
    [OnValueChanged("SetSeasonCamPosEditor")]
    public SeasonsCamPos seasonsCamPos = SeasonsCamPos.Default;

    private Material defaultSkyBox;

    #region MonoBehavior

    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }

        defaultSkyBox = RenderSettings.skybox;
    }

    private void OnEnable()
    {
        RenderSettings.skybox = mySkyBoxMat;
        desplayMode = DesplayMode.Ecliptic;
        seasonsCamPos = SeasonsCamPos.Default;

        SetDesplayMode(desplayMode);
        SetSeasonCamPos(seasonsCamPos);
    }

    private void OnDisable()
    {
        RenderSettings.skybox = defaultSkyBox;
    }

    private void Update()
    {
        earthOrbit.autoOrbit = autoOrbit;
    }


    #endregion

    #region Public Method

    [Button]
    public override void TriggerControlCanvas()
    {
        controlCanvasManager.Trigger();
    }

    public void DisableCanvas()
    {
        controlCanvasManager.SetControlCanvas(false);
    }

    public void AddMonth()
    {
        autoOrbit = false;
        month++;

        if (month > 12)
        {
            month = 1;
        }

        earthOrbit.SetMonthPosition(month);
    }

    public void MinusMonth()
    {
        autoOrbit = false;
        month--;

        if (month < 1)
        {
            month = 12;
        }

        earthOrbit.SetMonthPosition(month);
    }

    public void SetDesplayMode(DesplayMode _desplayMode)
    {
        desplayMode = _desplayMode;

        if(desplayMode == DesplayMode.Ecliptic)
        {
            eclipticEarth.SetActive(true);
            seasonsEarth.SetActive(false);
            SetSeasonCamPos(SeasonsCamPos.Default);
        }
        else
        {
            eclipticEarth.SetActive(false);
            seasonsEarth.SetActive(true);
        }
    }

    private void SetDesplayModeEditor()
    {
        SetDesplayMode(desplayMode);
    }

    public void SetSeasonCamPos(SeasonsCamPos _seasonsCamPos)
    {
        seasonsCamPos = _seasonsCamPos;

        switch (seasonsCamPos)
        {
            case SeasonsCamPos.Default:
                pcCamRoot.SetPositionAndRotation(camPos_Default.position, camPos_Default.rotation);
                openXRCamRoot.SetPositionAndRotation(camPos_Default.position, camPos_Default.rotation);
                break;

            case SeasonsCamPos.Spring:
                pcCamRoot.SetPositionAndRotation(camPos_Spring.position, camPos_Spring.rotation);
                openXRCamRoot.SetPositionAndRotation(camPos_Spring.position, camPos_Spring.rotation);
                break;

            case SeasonsCamPos.Sumemr:
                pcCamRoot.SetPositionAndRotation(camPos_Summer.position, camPos_Summer.rotation);
                openXRCamRoot.SetPositionAndRotation(camPos_Summer.position, camPos_Summer.rotation);
                break;

            case SeasonsCamPos.Full:
                pcCamRoot.SetPositionAndRotation(camPos_Full.position, camPos_Full.rotation);
                openXRCamRoot.SetPositionAndRotation(camPos_Full.position, camPos_Full.rotation);
                break;

            case SeasonsCamPos.Winter:
                pcCamRoot.SetPositionAndRotation(camPos_Winter.position, camPos_Winter.rotation);
                openXRCamRoot.SetPositionAndRotation(camPos_Winter.position, camPos_Winter.rotation);
                break;
        }
    }

    private void SetSeasonCamPosEditor()
    {
        SetSeasonCamPos(seasonsCamPos);
    }

    #endregion

    #region Private Method



    #endregion

}
