using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DayNightEnvironmentControl : MonoBehaviour
{
    public static DayNightEnvironmentControl ins;

    public Material mySkyBoxMat;
    public Transform sunTrans;

    public bool isNight = false;

    private Material defaultSkyBox;
    private float sunY;

    bool isInit;

    private void Awake()
    {
        if(ins == null)
        {
            ins = this;
        }

        defaultSkyBox = RenderSettings.skybox;
    }

    private void OnEnable()
    {
        RenderSettings.skybox = mySkyBoxMat;
        isInit = true;
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
            if (isNight == true || isInit)
            {
                mySkyBoxMat.DOKill();
                mySkyBoxMat.DOFloat(1f, "_Blend", 0.5f);
                
                isNight = false;
            }
        }
        //晚上
        else
        {
            if (isNight == false || isInit)
            {
                mySkyBoxMat.DOKill();
                mySkyBoxMat.DOFloat(0f, "_Blend", 0.5f);

                isNight = true;
            }
        }

    }
    
}
