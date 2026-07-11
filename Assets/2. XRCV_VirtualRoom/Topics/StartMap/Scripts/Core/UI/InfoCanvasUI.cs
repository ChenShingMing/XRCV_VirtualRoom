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

    private Camera _mainCamera;
    private float _timer;
    private const float UPDATE_INTERVAL = 0.1f;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector3 rot = new Vector3(0, _mainCamera.transform.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rot), 10 * Time.deltaTime);

        _timer += Time.deltaTime;
        if (_timer < UPDATE_INTERVAL) return;
        _timer = 0f;

        var data = starMapController.starMapControlData;
        year.text  = data.Year.ToString();
        month.text = data.Month.ToString();
        day.text   = data.Day.ToString();
        hour.text  = data.Hour.ToString();

        localicationName.text = data.currentLocalicationName;
        longitude.text        = data.longitude.ToString("F2");
        latitude.text         = data.latitude.ToString("F2");
    }
}
