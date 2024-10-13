using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class ControlCanvasManager : MonoBehaviour
{

    [FoldoutGroup("物件設置")]
    public Animator animator;
    [FoldoutGroup("物件設置")]
    public StarMapController starMapController;
    [FoldoutGroup("物件設置")]
    public InfoCanvasUI infoCanvasUI;

    [BoxGroup("物件設置/星空")]
    public Toggle graticule;
    [BoxGroup("物件設置/星空")]
    public Toggle linkLine;
    [BoxGroup("物件設置/星空")]
    public Toggle nameAndModel;

    [BoxGroup("物件設置/時間")]
    public Toggle rotateSpeed_None;
    [BoxGroup("物件設置/時間")]
    public Toggle rotateSpeed_Fast;
    [BoxGroup("物件設置/時間")]
    public Toggle rotateSpeed_Normal;
    [BoxGroup("物件設置/時間")]
    public Toggle rotateSpeed_Slow;

    [BoxGroup("物件設置/時間")]
    public Text year;
    [BoxGroup("物件設置/時間")]
    public Text month;
    [BoxGroup("物件設置/時間")]
    public Text day;
    [BoxGroup("物件設置/時間")]
    public Text hour;

    [BoxGroup("物件設置/地點")]
    public Text latitude;
    [BoxGroup("物件設置/地點")]
    public Text longitude;


    #region MonoBehavior

    // Update is called once per frame
    void Update()
    {
        //Vector3 rot = new Vector3(0, Camera.main.transform.rotation.eulerAngles.y, 0);
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rot), 10 * Time.deltaTime);

        UIUpdateHandle();
    }

    #endregion

    #region Public

    public void Trigger()
    {
        bool value = !animator.GetBool("isUse");
        SetControlCanvas(value);
    }
    public void SetControlCanvas(bool value)
    {
        if (value)
        {
            Vector3 rot = new Vector3(0, Camera.main.transform.rotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Euler(rot);

            this.gameObject.SetActive(value);
            infoCanvasUI.gameObject.SetActive(false);
        }
        else
        {
            Vector3 rot = new Vector3(0, Camera.main.transform.rotation.eulerAngles.y, 0);
            infoCanvasUI.transform.rotation = Quaternion.Euler(rot);
            infoCanvasUI.gameObject.SetActive(true);
        }
        
        animator.SetBool("isUse", value);
    }

    //給淡出動畫使用
    public void SetCanvasDeactive()
    {
        this.gameObject.SetActive(false);
    }

    #endregion

    #region Private

    void UIUpdateHandle()
    {
        graticule.isOn = starMapController.starMapControlData.graticule;
        linkLine.isOn = starMapController.starMapControlData.linkLine;
        nameAndModel.isOn = starMapController.starMapControlData.nameAndModel;

        rotateSpeed_None.isOn = starMapController.starMapControlData.rotateSpeed == 0;
        rotateSpeed_Fast.isOn = starMapController.starMapControlData.rotateSpeed == 8f;
        rotateSpeed_Normal.isOn = starMapController.starMapControlData.rotateSpeed == 3f;
        rotateSpeed_Slow.isOn = starMapController.starMapControlData.rotateSpeed == 0.5f;

        year.text = starMapController.starMapControlData.dateTime.Year.ToString();
        month.text = starMapController.starMapControlData.dateTime.Month.ToString();
        day.text = starMapController.starMapControlData.dateTime.Day.ToString();
        hour.text = starMapController.starMapControlData.dateTime.Hour.ToString();

        latitude.text = starMapController.starMapControlData.latitude.ToString(); //緯度
        longitude.text = starMapController.starMapControlData.longitude.ToString(); //經度
    }

    #endregion


}
