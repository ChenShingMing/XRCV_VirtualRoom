using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro; // 引入 TextMeshPro 命名空間

public class LocalicationSelectController : MonoBehaviour
{
    [FoldoutGroup("Other 其他設置")]
    public TMP_InputField latitude; //緯度
    [FoldoutGroup("Other 其他設置")]
    public TMP_InputField longitude; //經度


    public void OnSubmit()
    {
        // call StarMapControlData.SetLatitudeAndLongitude();
        if (StarMapController.ins != null)
        {
            float latitudeF = StarMapController.ins.starMapControlData.latitude; // 預設值為目前的緯度
            float longitudeF = StarMapController.ins.starMapControlData.longitude; // 預設值為目前的經度

            // 嘗試解析 latitude
            if (!float.TryParse(latitude.text, out latitudeF))
            {
                Debug.LogWarning("無法解析緯度，使用原本的數值。");
            }

            // 嘗試解析 longitude
            if (!float.TryParse(longitude.text, out longitudeF))
            {
                Debug.LogWarning("無法解析經度，使用原本的數值。");
            }

            // 設定天球經緯度
            // -1 表示數據內無目前地點
            StarMapController.ins.starMapControlData.SetLatitudeAndLongitude(-1, "自訂地點", latitudeF, longitudeF);
        }
    }

}
