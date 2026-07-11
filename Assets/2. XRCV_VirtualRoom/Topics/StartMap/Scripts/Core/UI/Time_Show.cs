using UnityEngine;
using UnityEngine.UI;

public class Time_Show : MonoBehaviour
{
    public Text time_text;

    private float _timer;
    private const float INTERVAL = 0.5f;
    private int _lastYear = -1, _lastMonth, _lastDay, _lastHour;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < INTERVAL) return;
        _timer = 0f;

        if (!StarMapController.ins) return;
        var dt = StarMapController.ins.starMapControlData.dateTime;
        if (dt.Year == _lastYear && dt.Month == _lastMonth &&
            dt.Day == _lastDay && dt.Hour == _lastHour) return;

        _lastYear  = dt.Year;
        _lastMonth = dt.Month;
        _lastDay   = dt.Day;
        _lastHour  = dt.Hour;
        time_text.text = $"{_lastYear} 年 {_lastMonth} 月 {_lastDay} 日 {_lastHour} 時";
    }
}
