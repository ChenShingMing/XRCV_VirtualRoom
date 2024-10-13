using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudentTipController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (StarMapController.ins)
        {
            string timeValue = StarMapController.ins.starMapControlData.dateTime.Year.ToString() + " 年 "
                + StarMapController.ins.starMapControlData.dateTime.Month.ToString() + " 月 "
                + StarMapController.ins.starMapControlData.dateTime.Day.ToString() + " 日 "
                + StarMapController.ins.starMapControlData.dateTime.Hour.ToString() + " 時";

            string localication = "緯度：" + StarMapController.ins.starMapControlData.latitude + "\n" 
                + "經度：" + StarMapController.ins.starMapControlData.longitude;

            //ClientUI.ins.SetTipTitle(StarMapController.ins.starMapControlData.currentLocalicationName);
            //ClientUI.ins.SetTipContent(localication);


            //ClientUI.ins.SetTip2Title("時間");
            //ClientUI.ins.SetTip2Content(timeValue);



        }

        
    }
}
