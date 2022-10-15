using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarMapTopicData
{
    public bool graticule;
    public bool linkLine;
    public bool nameAndModel;

    public int Year;
    public int Month;
    public int Day;
    public int Hour;
    public float rotateSpeed;

    public bool usePanoramic;
    public int currentlocalicationIndex;
    public string currentLocalicationName;
    public float latitude;
    public float longitude;

    public void SetData(StarMapControlData value)
    {
        graticule = value.graticule;
        linkLine = value.linkLine;
        nameAndModel = value.nameAndModel;

        Year = value.Year;
        Month = value.Month;
        Day = value.Day;
        Hour = value.Hour;
        rotateSpeed = value.rotateSpeed;

        usePanoramic = value.usePanoramic;
        currentlocalicationIndex = value.currentlocalicationIndex;
        currentLocalicationName = value.currentLocalicationName;
        latitude = value.latitude;
        longitude = value.longitude;
    }
}
