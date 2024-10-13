using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

[System.Serializable]
public class StarMapControlData
{
    public enum RotateSpeed
    {
        None,
        Fast,
        Normal,
        Slow
    }

    enum PanoramicStatus
    {
        InitTexture,
        Day,
        Night
    }

    #region 星座 變數

    [FoldoutGroup("星座")]
    public bool graticule;
    [FoldoutGroup("星座")]
    public bool linkLine;
    [FoldoutGroup("星座")]
    public bool nameAndModel;

    #endregion

    #region 日期 變數

    [FoldoutGroup("日期")]
    [InlineButton("SetEarthRotateSpeed", "$rotateSpeedStauts")]
    public float rotateSpeed;
    string rotateSpeedStauts = "None";
    int rotateStautsCount = 0;

    [PropertySpace]

    [FoldoutGroup("日期")]
    public DateTime dateTime;

    [FoldoutGroup("日期")]
    [InlineButton("AddYear", "+")]
    [InlineButton("MinusYear", "-")]
    public int Year;


    [FoldoutGroup("日期")]
    [InlineButton("MinusMonth", "-")]
    [InlineButton("AddMonth", "+")]
    public int Month;

    [FoldoutGroup("日期")]
    [InlineButton("AddDay", "+")]
    [InlineButton("MinusDay", "-")]
    public int Day;


    [FoldoutGroup("日期")]
    [InlineButton("MinusHour", "-")]
    [InlineButton("AddHour", "+")]
    public int Hour;

    #endregion

    #region 位置 變數

    [FoldoutGroup("位置")]
    public PanoramicController panoramicController;

    [FoldoutGroup("位置")]
    public int currentlocalicationIndex;

    [FoldoutGroup("位置")]
    public string currentLocalicationName;

    [FoldoutGroup("位置")]
    public float latitude;

    [FoldoutGroup("位置")]
    public float longitude;

    bool currentPanoramicBool;
    PanoramicStatus panoramicStatus;

    public Texture currentDay360;
    public Texture currentNight360;

    #endregion

    public delegate void StarMapControlDataEventHandler();
    public StarMapControlDataEventHandler OnInitCompleteEvent;

    bool isInit;

    #region 執行緒

    public void OnInit()
    {
        graticule = false;
        linkLine = false;
        nameAndModel = false;

        SetDateTimeNow();
        rotateSpeed = 0;

        currentlocalicationIndex = -1;
        currentLocalicationName = "自訂地點";
        latitude = 23.9738f;
        longitude = 120.982f;

        if (OnInitCompleteEvent != null)
        {
            OnInitCompleteEvent.Invoke();
        }

        isInit = true;
    }

    public void OnDisable()
    {
        isInit = false;
    }

    public void OnFixedUpdate()
    {
        if (!isInit) return;

        SetDateTime(Year, Month, Day, Hour);
        PanoramicHandle();
    }

    #endregion

    #region Public

    #region 日期 方法
    
    public void SetRotateSpeed(RotateSpeed rotateSpeed)
    {
        switch(rotateSpeed)
        {
            case RotateSpeed.None:
                this.rotateSpeed = 0;
                break;
            case RotateSpeed.Fast:
                this.rotateSpeed = 8f;
                break;
            case RotateSpeed.Normal:
                this.rotateSpeed = 3f;
                break;
            case RotateSpeed.Slow:
                this.rotateSpeed = 0.5f;
                break;
        }
    }


    [FoldoutGroup("日期")]
    [Button]
    public void SetDateTimeNow()
    {
        string Time = string.Format("{0}-{1}-{2} {3}:00:00",
                                DateTime.Now.Year,
                                DateTime.Now.Month,
                                DateTime.Now.Day,
                                DateTime.Now.Hour);

        dateTime = DateTime.Parse(Time);
        SetIntValueFromDateTime();
    }

    public void AddYear()
    {
        dateTime = dateTime.AddYears(1);
        SetIntValueFromDateTime();
    }

    public void MinusYear()
    {
        dateTime = dateTime.AddYears(-1);
        SetIntValueFromDateTime();
    }

    public void AddMonth()
    {
        dateTime = dateTime.AddMonths(1);
        SetIntValueFromDateTime();
    }

    public void MinusMonth()
    {
        dateTime = dateTime.AddMonths(-1);
        SetIntValueFromDateTime();
    }

    public void AddDay()
    {
        dateTime = dateTime.AddDays(1);
        SetIntValueFromDateTime();
    }

    public void MinusDay()
    {
        dateTime = dateTime.AddDays(-1);
        SetIntValueFromDateTime();
    }

    public void AddHour()
    {
        dateTime = dateTime.AddHours(1);
        SetIntValueFromDateTime();
    }

    public void MinusHour()
    {
        dateTime = dateTime.AddHours(-1);
        SetIntValueFromDateTime();
    }

    #endregion

    #region 位置 方法

    //提供設定的方法
    public void SetLatitudeAndLongitude(int id, Location data)
    {
        currentlocalicationIndex = id;
        currentLocalicationName = data.name;
        latitude = data.latitudeAndLongitude.x;
        longitude = data.latitudeAndLongitude.y;
    }
    public void SetLatitudeAndLongitude(int id, string name, float latitude, float longitude)
    {
        this.currentlocalicationIndex = id;
        this.currentLocalicationName = name;
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public void SetPanoramicPhoto()
    {
        if (DayNightEnvironmentControl.ins.isNight)
        {
            panoramicStatus = PanoramicStatus.Night;
            panoramicController.Use(currentNight360);
        }
        else
        {
            panoramicStatus = PanoramicStatus.Day;
            panoramicController.Use(currentDay360);
        }
    }

    #endregion

    #endregion

    #region Private

    //Inspector 使用
    void SetEarthRotateSpeed()
    {
        rotateStautsCount++;

        switch (rotateStautsCount % 4)
        {
            case 0:

                SetRotateSpeed(RotateSpeed.None);
                rotateSpeedStauts = "None";
                break;

            case 1:

                SetRotateSpeed(RotateSpeed.Slow);
                rotateSpeedStauts = "Slow";

                break;

            case 2:

                SetRotateSpeed(RotateSpeed.Normal);
                rotateSpeedStauts = "Normal";

                break;

            case 3:

                SetRotateSpeed(RotateSpeed.Fast);
                rotateSpeedStauts = "Fast";

                break;

            default:
                rotateSpeedStauts = "None";
                break;
        }

    }
    void PanoramicHandle()
    {
        //如果日夜不一樣
        if (DayNightEnvironmentControl.ins.isNight)
        {
            if (panoramicStatus != PanoramicStatus.Night)
            {
                panoramicStatus = PanoramicStatus.Night;
                panoramicController.Use(currentNight360);
            }
        }
        else
        {
            if (panoramicStatus != PanoramicStatus.Day)
            {
                panoramicStatus = PanoramicStatus.Day;
                panoramicController.Use(currentDay360);
            }
        }

    }


    void SetDateTime(int Year, int Month, int Day, int Hour)
    {
        //Debug.Log("SetDateTime : " + Year + " " + Month + " " + Day + " " + Hour);

        string Time = string.Format("{0}-{1}-{2} {3}:00:00", Year, Month, Day, Hour);
        dateTime = DateTime.Parse(Time);
    }

    void SetIntValueFromDateTime()
    {
        Year = dateTime.Year;
        Month = dateTime.Month;
        Day = dateTime.Day;
        Hour = dateTime.Hour;
    }

    #endregion


}