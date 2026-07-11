using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class StarMapRotate : MonoBehaviour
{
    #region Public 參數

    public Transform latitudeTrans;
    public Transform StarTrans;

	public float latitude = 23.5f;//緯度
    public float longitude = 120.5f;//經度


    public float rotateSpeed = 0;

    public DateTime dateTime;

    [HideInInspector]
    public float Spindle;//自轉軸
    [HideInInspector]
	public double SpindleSampleValue; //單位 : 小時
	[HideInInspector]
	public float SpindleSampleAngle = 15; //單位角度

    [HideInInspector]
    public float longitudeOffset;

    //[HideInInspector]
    public float differEulerAnglesY;

    #endregion

    private TimeSpan timeSpan;
    private Vector3 prev_transform;
    private static readonly System.DateTime _baseline = new System.DateTime(2020, 1, 1, 0, 0, 0);
    
	
	// Update is called once per frame
	void FixedUpdate()
    {
        SetAxis();
        RotateHandle();
    }


    #region Public Method

    public void SetDateTime(int Year, int Month, int Day, int Hour)
    {
        dateTime = new DateTime(Year, Month, Day, Hour, 0, 0);
    }

    #endregion


    #region Private Method

    //根據緯度轉動緯度層
    void SetAxis()
    {
        // 120.5 是因為一開始天球以台灣作為基準，所以要符合格林威至，就必須扣掉120.5的基礎經度
        longitudeOffset = longitude - 120.5f;

        latitudeTrans.localRotation = Quaternion.Euler(0, 0, 90 + latitude);
    }

    //將時間轉成自轉單位（以 2020-01-01 00:00 為基準）
    void TimeToSpindleSample()
    {
        timeSpan = dateTime - _baseline;
        SpindleSampleValue = timeSpan.TotalHours;
    }

    void RotateHandle()
    {
        TimeToSpindleSample();

        if (rotateSpeed != 0)
        {
            prev_transform = StarTrans.localEulerAngles;
            StarTrans.Rotate(-Vector3.up * Time.deltaTime * rotateSpeed);

            float minues = prev_transform.y - StarTrans.localEulerAngles.y;
            if (Mathf.Abs(minues) > 1f) return; //自轉經過360，導致差異過大，這幀變動需要被忽略。


            differEulerAnglesY += (prev_transform.y - StarTrans.localEulerAngles.y);
            //Debug.Log("prev_transform.y : " + prev_transform.y + " " + " StarTrans.localEulerAngles.y : " + StarTrans.localEulerAngles.y);


            //判斷旋轉有沒有超過一小時
            if (differEulerAnglesY >= 15f)
            {
                Debug.Log("AddHour");
                StarMapController.ins.starMapControlData.AddHour();
                differEulerAnglesY = 0f;
            }
        }
        else
        {
            if (differEulerAnglesY != 0)
            {
                differEulerAnglesY = 0;
            }

            float targetSpindle = (-((float)SpindleSampleValue) * SpindleSampleAngle - (float)timeSpan.TotalDays - 15f - longitudeOffset);
            Spindle = Mathf.Lerp(Spindle, targetSpindle, Time.deltaTime * 3);
            //Spindle = -((float)SpindleSampleValue) * SpindleSampleAngle - (float)timeSpan.TotalDays - 15f - longitudeOffset;
            StarTrans.localEulerAngles = new Vector3(0, Spindle, 0);
        }

    }


    #endregion


}
