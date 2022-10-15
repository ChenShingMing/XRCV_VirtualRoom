using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Time_Show : MonoBehaviour {

    public Text time_text;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (StarMapController.ins)
		{
			time_text.text = StarMapController.ins.starMapControlData.dateTime.Year.ToString() + " 年 "
				+ StarMapController.ins.starMapControlData.dateTime.Month.ToString() + " 月 "
				+ StarMapController.ins.starMapControlData.dateTime.Day.ToString() + " 日 "
				+ StarMapController.ins.starMapControlData.dateTime.Hour.ToString() + " 時";
		}
	}
}
