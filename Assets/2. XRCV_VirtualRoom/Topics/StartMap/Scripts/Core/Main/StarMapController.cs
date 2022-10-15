using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;


public class StarMapController : MonoBehaviour
{
    public static StarMapController ins;

    [TitleGroup("設置")]
    [FoldoutGroup("設置/物件設定")]
    public StarMapRotate starMapRotate;
    [FoldoutGroup("設置/物件設定")]
    public StarMap starMap;
    [FoldoutGroup("設置/物件設定")]
    public CreateHipHierarchy createHipHierarchy;
    [FoldoutGroup("設置/物件設定")]
    public ControlCanvasManager controlCanvasManager;

    [FoldoutGroup("設置/物件設定")]
    public GameObject graticuleObject;
    [FoldoutGroup("設置/物件設定")]
    public GameObject compassPoint;

    [Title("控制選項")]
    [SerializeField]
    [HideLabel]
    public StarMapControlData starMapControlData;


    #region Private 參數

    
    private Transform north_transform;

    #endregion

    #region MonoBehavior

    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }
    }

    private void OnEnable()
    {
        starMapControlData.OnInit();
    }

    private void OnDisable()
    {
        starMapControlData.OnDisable();
    }


    void FixedUpdate()
    {
        starMapControlData.OnFixedUpdate();
        StarMapHandle();
    }

    #endregion

    #region Public Method

    [Button]
    public void TriggerControlCanvas()
    {
        controlCanvasManager.Trigger();
    }

    public void DisableCanvas()
    {
        controlCanvasManager.SetControlCanvas(false);
    }

    public void TriggerGraticule(bool value)
    {
        starMapControlData.graticule = value;
    }

    public void TriggerLinkLine(bool value)
    {
        starMapControlData.linkLine = value;
    }

    public void TriggerNameAndModel(bool value)
    {
        starMapControlData.nameAndModel = value;
    }

    public void AddYear()
    {
        starMapControlData.AddYear();
    }
    public void MinusYear()
    {
        starMapControlData.MinusYear();
    }

    public void AddMonth()
    {
        starMapControlData.AddMonth();
    }
    public void MinusMonth()
    {
        starMapControlData.MinusMonth();
    }

    public void AddDay()
    {
        starMapControlData.AddDay();
    }
    public void MinusDay()
    {
        starMapControlData.MinusDay();
    }

    public void AddHour()
    {
        starMapControlData.AddHour();
    }
    public void MinusHour()
    {
        starMapControlData.MinusHour();
    }

    public void SetNonRotate(bool value)
    {
        if (!value) return;
        starMapControlData.SetRotateSpeed(StarMapControlData.RotateSpeed.None);
    }

    public void SetRotateSlow(bool value)
    {
        if (!value) return;
        starMapControlData.SetRotateSpeed(StarMapControlData.RotateSpeed.Slow);
    }

    public void SetRotateMiddle(bool value)
    {
        if (!value) return;
        starMapControlData.SetRotateSpeed(StarMapControlData.RotateSpeed.Normal);
    }

    public void SetRotateFast(bool value)
    {
        if (!value) return;
        starMapControlData.SetRotateSpeed(StarMapControlData.RotateSpeed.Fast);
    }

    public void SetUsePanoramic(bool value)
    {
        starMapControlData.usePanoramic = value;
    }

    #endregion

    #region Private Method

    void StarMapHandle()
    {
        //星座 變數處理
        graticuleObject.SetActive(starMapControlData.graticule);
        starMap.OnLineRender = starMapControlData.linkLine;
        starMap.On3DRender = starMapControlData.nameAndModel;

        //日期 變數處理
        starMapRotate.SetDateTime(starMapControlData.Year, starMapControlData.Month, starMapControlData.Day, starMapControlData.Hour);
        starMapRotate.rotateSpeed = starMapControlData.rotateSpeed;

        //位置 變數處理
        starMapRotate.latitude = starMapControlData.latitude; //緯度
        starMapRotate.longitude = starMapControlData.longitude; //經度

        //讓地上指北針指向小熊座
        SetLatitudeDirection();
    }

    void SetLatitudeDirection()
    {
        if (createHipHierarchy.zodiac_Datas != null)
        {
            if (createHipHierarchy.zodiac_Datas.Count != 0 && north_transform == null)
            {
                for (int i = 0; i < createHipHierarchy.zodiac_Datas.Count; i++)
                {
                    if (createHipHierarchy.zodiac_Datas[i].name == "UMi 小熊座")
                    {
                        north_transform = createHipHierarchy.zodiac_Datas[i].world_transform;
                    }
                }
            }
        }

        if (north_transform != null)
        {
            compassPoint.transform.forward = (this.transform.position - new Vector3(north_transform.position.x,
                                                                                this.transform.position.y,
                                                                                north_transform.position.z));
        }
    }


    #endregion

}
