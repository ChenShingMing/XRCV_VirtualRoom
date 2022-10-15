using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudentTipMethod : MonoBehaviour
{
    public StarMapController starMapController;

    public void SetStudentTipText(string tip)
    {
        //Player.localPlayer.RpcSetSutdentTip(tip);
    }

    private void FixedUpdate()
    {
        /*
        CanvasNetworkHUD.ins.studentTipText_2.text = starMapController.starMapControlData.dateTime.Year.ToString() + "年" +
                                                        starMapController.starMapControlData.dateTime.Month.ToString() + "月" +
                                                        starMapController.starMapControlData.dateTime.Day.ToString() + "日" +
                                                        starMapController.starMapControlData.dateTime.Hour.ToString() + "時";
        */
    }

}
