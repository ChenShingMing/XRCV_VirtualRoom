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
    public Text localicationName;
    [BoxGroup("物件設置/地點")]
    public Text latitude;
    [BoxGroup("物件設置/地點")]
    public Text longitude;
    [BoxGroup("物件設置/地點")]
    public Toggle usePanoramic;


    #region MonoBehavior

    private float _uiTimer;
    private const float UI_UPDATE_INTERVAL = 0.1f;
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        _uiTimer += Time.deltaTime;
        if (_uiTimer < UI_UPDATE_INTERVAL) return;
        _uiTimer = 0f;
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
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (value)
        {
            Vector3 rot = new Vector3(0, _mainCamera.transform.rotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Euler(rot);

            this.gameObject.SetActive(value);
            infoCanvasUI.gameObject.SetActive(false);
        }
        else
        {
            Vector3 rot = new Vector3(0, _mainCamera.transform.rotation.eulerAngles.y, 0);
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
        var data = starMapController.starMapControlData;

        // Toggle 加值比對，避免值未改變時觸發 onValueChanged 事件
        SetToggle(graticule,          data.graticule);
        SetToggle(linkLine,           data.linkLine);
        SetToggle(nameAndModel,       data.nameAndModel);
        SetToggle(usePanoramic,       data.usePanoramic);
        SetToggle(rotateSpeed_None,   data.rotateSpeed == 0f);
        SetToggle(rotateSpeed_Fast,   data.rotateSpeed == 8f);
        SetToggle(rotateSpeed_Normal, data.rotateSpeed == 3f);
        SetToggle(rotateSpeed_Slow,   data.rotateSpeed == 0.5f);

        year.text             = data.Year.ToString();
        month.text            = data.Month.ToString();
        day.text              = data.Day.ToString();
        hour.text             = data.Hour.ToString();
        localicationName.text = data.currentLocalicationName;
        latitude.text         = data.latitude.ToString("F2");
        longitude.text        = data.longitude.ToString("F2");
    }

    private static void SetToggle(Toggle toggle, bool value)
    {
        if (toggle.isOn != value) toggle.isOn = value;
    }

    #endregion


}
