using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class DayNightEnvironmentControl : MonoBehaviour
{
    public static DayNightEnvironmentControl ins;

    public Material mySkyBoxMat;
    public Transform sunTrans;

    public bool isNight = false;

    private Material defaultSkyBox;
    private float sunY;

    private void Awake()
    {
        if (ins == null)
            ins = this;

        defaultSkyBox = RenderSettings.skybox;
    }

    private void OnDestroy()
    {
        if (ins == this) ins = null;
    }

    private void OnEnable()
    {
        RenderSettings.skybox = mySkyBoxMat;

        sunY = sunTrans.position.y;
        //白天
        if (sunY > 0f)
        {
            SetDay();
            isNight = false;
        }
        //晚上
        else
        {
            SetNight();
            isNight = true;
        }
    }

    private void OnDisable()
    {
        RenderSettings.skybox = defaultSkyBox;
        //ClassroomManager.ins.panoramicPhotoManager.Fade(ClassroomManager.ins.panoramicPhotoManager.initTexture);
    }


    private void FixedUpdate()
    {
        SkyBoxHandle();
    }

    void SkyBoxHandle()
    {
        sunY = sunTrans.position.y;
        //Debug.Log("sunY:" + sunY);

        //白天
        if (sunY > 0f)
        {
            if (isNight)
            {
                SetDay();
                isNight = false;
            }
        }
        //晚上
        else
        {
            if (!isNight)
            {
                SetNight();
                isNight = true;
            }
        }

    }

    [Button]
    private void SetDay()
    {
        //Debug.Log("SetDay");

        mySkyBoxMat.DOKill();
        mySkyBoxMat.DOFloat(1f, "_Blend", 0.5f);
    }

    [Button]
    private void SetNight()
    {
        //Debug.Log("SetNight");

        mySkyBoxMat.DOKill();
        mySkyBoxMat.DOFloat(0f, "_Blend", 0.5f);
    }

}
