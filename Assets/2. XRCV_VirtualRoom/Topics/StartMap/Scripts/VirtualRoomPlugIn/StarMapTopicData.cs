using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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

    public void ApplyTo(StarMapControlData target)
    {
        target.graticule = graticule;
        target.linkLine = linkLine;
        target.nameAndModel = nameAndModel;

        target.Year = Year;
        target.Month = Month;
        target.Day = Day;
        target.Hour = Hour;
        target.rotateSpeed = rotateSpeed;

        target.usePanoramic = usePanoramic;
        target.currentlocalicationIndex = currentlocalicationIndex;
        target.currentLocalicationName = currentLocalicationName;
        target.latitude = latitude;
        target.longitude = longitude;
    }
}
