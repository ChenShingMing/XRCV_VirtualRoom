using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class InfoCanvasUI : MonoBehaviour
{
    [FoldoutGroup("物件設置")]
    public StarMapController starMapController;
    [FoldoutGroup("物件設置")]
    [BoxGroup("物件設置/Date")]
    public Text year;
    [BoxGroup("物件設置/Date")]
    public Text month;
    [BoxGroup("物件設置/Date")]
    public Text day;
    [BoxGroup("物件設置/Date")]
    public Text hour;

    [BoxGroup("物件設置/location")]
    public Text localicationName;
    [BoxGroup("物件設置/location")]
    public Text longitude;
    [BoxGroup("物件設置/location")]
    public Text latitude;
    

    private void Update()
    {
        Vector3 rot = new Vector3(0, Camera.main.transform.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rot), 10 * Time.deltaTime);

        year.text = starMapController.starMapControlData.dateTime.Year.ToString();
        month.text = starMapController.starMapControlData.dateTime.Month.ToString();
        day.text = starMapController.starMapControlData.dateTime.Day.ToString();
        hour.text = starMapController.starMapControlData.dateTime.Hour.ToString();

        localicationName.text = starMapController.starMapControlData.currentLocalicationName;
        longitude.text = starMapController.starMapControlData.longitude.ToString(); //經度
        latitude.text = starMapController.starMapControlData.latitude.ToString(); //緯度
    }

}
