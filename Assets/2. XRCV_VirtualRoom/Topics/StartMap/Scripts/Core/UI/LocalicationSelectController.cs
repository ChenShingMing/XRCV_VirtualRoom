using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Linq;
using System;

public class LocalicationSelectController : MonoBehaviour
{
    [FoldoutGroup("ScrollView 物件設置")]
    public ToggleGroup scrollToggleGroup;
    [FoldoutGroup("ScrollView 物件設置")]
    public LocalicationItem itemTemplate;
    [FoldoutGroup("ScrollView 物件設置")]
    public Transform scrollContent;

    [FoldoutGroup("Other 其他設置")]
    public InputField latitude; //緯度
    [FoldoutGroup("Other 其他設置")]
    public InputField longitude; //經度

    private List<Location> locationDataList;

    private List<LocalicationItem> allItems = new List<LocalicationItem>(); //儲存當前所有的地點
    private IEnumerable<Toggle> activeItems; // 所有的地點Toggle
    private LocalicationItem activeItem; //當前選中的item

    public void Awake()
    {
        locationDataList = StarMapController.ins.starMapControlData.locationData.locationList;

        //初始化生成Scroll View 裡面的Item
        for (int i = 0; i < locationDataList.Count; i++)
        {
            itemTemplate = Instantiate(itemTemplate, scrollContent);
            itemTemplate.localicationName.text = locationDataList[i].name;
            itemTemplate.rawImage.texture = locationDataList[i].picture;
            itemTemplate.dataID = i;

            allItems.Add(itemTemplate);

            itemTemplate.gameObject.SetActive(true);
        }
    }

    //用於Toggle被選擇時
    public void OnToggleSelect(bool toggleValue)
    {
        if (toggleValue == false) return;

        //將選重的資料填上InputField

        activeItems = scrollToggleGroup.ActiveToggles();
        activeItem = activeItems.ElementAt(0).GetComponent<LocalicationItem>();


        latitude.text = locationDataList[activeItem.dataID].latitudeAndLongitude.x.ToString();
        longitude.text = locationDataList[activeItem.dataID].latitudeAndLongitude.y.ToString();

        //Debug.Log(starMapPanoramicDatas[activeItem.dataID].latitudeAndLongitude.x.ToString());
    }

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

        //比對全部的資料，抓出符合輸入的選項並啟用，若沒有就全部取消
        for (int i = 0; i < locationDataList.Count; i++)
        {
            if(temp != locationDataList[i].latitudeAndLongitude)
            {
                continue;
            }
            else
            {
                activeItem = allItems[i];
                allItems[i].GetComponent<Toggle>().isOn = true;
                return;
            }
           
        }

        activeItem = null;
        scrollToggleGroup.SetAllTogglesOff();
    }

    public void OnSubmit()
    {
        // call StarMapControlData.SetLatitudeAndLongitude();
        if (StarMapController.ins != null)
        {
            if (activeItem != null)
            {
                StarMapController.ins.starMapControlData.SetLatitudeAndLongitude(activeItem.dataID, locationDataList[activeItem.dataID]);
                StarMapController.ins.starMapControlData.SetPanoramicPhoto();
            }
            else
            {
                //設定天球經緯度
                // -1 表示數據內無目前地點
                StarMapController.ins.starMapControlData.SetLatitudeAndLongitude(-1, "自訂地點", float.Parse(latitude.text), float.Parse(longitude.text));
            }
        }
    }

}
