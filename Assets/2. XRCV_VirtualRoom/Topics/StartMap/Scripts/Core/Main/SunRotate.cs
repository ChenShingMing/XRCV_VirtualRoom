using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotate : MonoBehaviour {
    
    public Transform Sun_transform;
    public double AxisAngleDay;
    public float AxisAngle;

    
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
        if (StarMapController.ins)
        {
            double day = StarMapController.ins.starMapControlData.dateTime.DayOfYear;


            AxisAngleDay = day;
            AxisAngle = (float)((day > 182) ? (day > 273) ? (day - 273) * (-23.5 / 91) : 23.5 - (day - 182) * (23.5 / 91)
                : (day > 91) ? (day - 91) * (23.5 / 91) : -23.5 + day * (23.5 / 91));
            
        }

    }
}
