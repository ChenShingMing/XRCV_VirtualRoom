using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotate : MonoBehaviour {

    public Transform Sun_transform;
    public double AxisAngleDay;
    public float AxisAngle;

    private int _lastDayOfYear = -1;

    
    void Start ()
    {
        SetRotateAngle();
        SetSunAxis();
        Sun_transform.localEulerAngles = new Vector3(AxisAngle, 250f, 0);
    }

    void FixedUpdate()
    {
        SetRotateAngle();
        SetSunAxis();
    }

    void SetRotateAngle()
    {
        if (StarMapController.ins)
        {
            Sun_transform.localEulerAngles = new Vector3(
                AxisAngle, 
                250f - (15 * StarMapController.ins.starMapControlData.dateTime.Hour) 
                - StarMapController.ins.starMapRotate.differEulerAnglesY 
                - StarMapController.ins.starMapRotate.longitudeOffset,
                0 );
        }
    }

    void SetSunAxis()
    {
        if (!StarMapController.ins) return;
        int dayOfYear = StarMapController.ins.starMapControlData.dateTime.DayOfYear;
        if (dayOfYear == _lastDayOfYear) return;
        _lastDayOfYear = dayOfYear;

        AxisAngleDay = dayOfYear;
        AxisAngle = (float)((dayOfYear > 182)
            ? (dayOfYear > 273) ? (dayOfYear - 273) * (-23.5 / 91) : 23.5 - (dayOfYear - 182) * (23.5 / 91)
            : (dayOfYear > 91)  ? (dayOfYear - 91)  * (23.5 / 91)  : -23.5 + dayOfYear * (23.5 / 91));
    }
}
