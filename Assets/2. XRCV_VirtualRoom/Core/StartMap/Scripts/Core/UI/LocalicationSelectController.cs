using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Linq;
using System;

public class LocalicationSelectController : MonoBehaviour
{
    [FoldoutGroup("Other 其他設置")]
    public InputField latitude; //緯度
    [FoldoutGroup("Other 其他設置")]
    public InputField longitude; //經度


    //用於修改 InputField 時
    public void OnInputFieldChange(string value)
    {
        Vector2 temp;
        try
        {
            temp = new Vector2(float.Parse(latitude.text), float.Parse(longitude.text));
        }
        catch (Exception e)
        {
            Debug.Log(e);
            temp = new Vector2();
        }
    }

    public void OnSubmit()
    {
        // call StarMapControlData.SetLatitudeAndLongitude();
        if (StarMapController.ins != null)
        {
            //設定天球經緯度
            // -1 表示數據內無目前地點
            StarMapController.ins.starMapControlData.SetLatitudeAndLongitude(-1, "自訂地點", float.Parse(latitude.text), float.Parse(longitude.text));

        }
    }

}
