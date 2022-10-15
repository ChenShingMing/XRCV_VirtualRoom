using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateHipHierarchy : MonoBehaviour 
{
    #region Inner Class

    //十二星座數據類型的定義
    public class Zodiac_Data
    {
        //星座名稱
        public string name;
        //中心座標
        public Vector3 center_pos;
        //世界座標
        public Transform world_transform;
        //連接星座清單
        public List<StarMap.HipData> link_hip_data;
        //星座美術圖材質
        public Sprite hip_sprite;
        //星座美術模型
        public GameObject hip_modelobj;
        //星座GameObject
        public GameObject hip_obj;
        //名稱的GameObject
        public GameObject hip_name_obj;

        //建構子
        public Zodiac_Data(string _name, List<StarMap.HipData> _linkhipdata, float distance)
        {
            name = _name;
            link_hip_data = _linkhipdata;
            center_pos = new Vector3();
            hip_sprite = null;
            hip_modelobj = null;
            hip_obj = null;
            hip_name_obj = null;

            //初始化中心座標
            center_pos = Calculatio_centerPos(link_hip_data) * distance;
        }

        //方法
        public Vector3 Calculatio_centerPos(List<StarMap.HipData> _hipdata)
        {

            float _x = 0;
            float _y = 0;
            float _z = 0;

            for (int i = 0; i < _hipdata.Count; i++)
            {
                _x += _hipdata[i].pos.x;
                _y += _hipdata[i].pos.y;
                _z += _hipdata[i].pos.z;
                          
            }

            return new Vector3(_x / _hipdata.Count, _y / _hipdata.Count, _z / _hipdata.Count);
        }

    }

    #endregion


    //所有星座資料
    public List<Zodiac_Data> zodiac_Datas;

    [HideInInspector]
    //階層物件父類別
    public GameObject hip_Parent;
    public StarMap starMap;

    //移除連線之清單
    List<StarMap.HipLine> hipLineList_Remove = new List<StarMap.HipLine>();

    public void ScanHipData()
    {
        zodiac_Datas = FindAllHipKind(starMap.hipLineList);
        zodiac_Datas = CreateHipGameObject(zodiac_Datas);
    }

    #region Private Method

    //由清單找出全部有幾種星座的方法
    List<Zodiac_Data> FindAllHipKind(List<StarMap.HipLine> _hiplinelistall)
    {
        List<Zodiac_Data> zodiac_hip = new List<Zodiac_Data>();
        string hip_shortname = string.Empty;

        //用於計數是否走到最後一個
        int count = 0;

        //走訪所有連線的鏈結找出星星種類並新增至十二星座的星星鏈結
        while(count < _hiplinelistall.Count)
        {
            //將當前的星空名存於暫存
            hip_shortname = _hiplinelistall[count].constellationNameShort;
            //存該星座所有組成星星的暫存
            List<StarMap.HipData> hipDatas = new List<StarMap.HipData>();
            //判斷該連線的兩顆點是否已經存在於組成星星暫存內
            while(count < _hiplinelistall.Count && _hiplinelistall[count].constellationNameShort == hip_shortname)
            {
                bool stt_isexist = false;
                bool end_isexist = false;

                for (int i = 0; i < hipDatas.Count; i++)
                {
                    if(_hiplinelistall[count].sttData.hipId == hipDatas[i].hipId) stt_isexist = true;
                    if(_hiplinelistall[count].endData.hipId == hipDatas[i].hipId) end_isexist = true;
                }

                if(!stt_isexist)
                    hipDatas.Add(new StarMap.HipData(_hiplinelistall[count].sttData));

                if (!end_isexist)
                    hipDatas.Add(new StarMap.HipData(_hiplinelistall[count].endData));

                //換下一筆資料
                count++;
            }//當下一筆資料的星座名不相等時跳出

            //將目前資料新增成十二星座資料並存於list
            zodiac_hip.Add(new Zodiac_Data(hip_shortname, hipDatas, starMap.distance));
        }

        //Debug.Log(zodiac_hip.Count);
        //Debug.Log(zodiac_hip[11].name);
        //Debug.Log(zodiac_hip[11].link_hip_data.Count);

        //回傳十二星座清單
        return zodiac_hip;

    }

    //創建物件階層方法
    List<Zodiac_Data> CreateHipGameObject(List<Zodiac_Data> _zodiacDatas)
    {

        List<Zodiac_Data> zodiacs = new List<Zodiac_Data>();
        hip_Parent = new GameObject("Zodiac");
        hip_Parent.transform.SetParent(this.gameObject.transform);
        hip_Parent.transform.localPosition = new Vector3();
        hip_Parent.transform.localRotation = Quaternion.identity;
        hip_Parent.gameObject.SetActive(false);

        hip_Parent.transform.parent.gameObject.AddComponent<ZodiacsController>().createHipHierarchy = this;

        for (int i = 0; i < _zodiacDatas.Count; i++)
        {
            //生成物件階層
            GameObject newhip = new GameObject(_zodiacDatas[i].name);
            newhip.transform.SetParent(hip_Parent.transform);
            newhip.transform.localPosition = _zodiacDatas[i].center_pos ;
            newhip.transform.localRotation = Quaternion.identity;

            GameObject newmodelObject = null;
            Object obj = Resources.Load("Prefabs/" + _zodiacDatas[i].name);

            if (obj != null)
            {
                newmodelObject = Instantiate(obj) as GameObject;
                newmodelObject.name = _zodiacDatas[i].name;
                newmodelObject.transform.SetParent(newhip.transform);
                newmodelObject.transform.localPosition = new Vector3();
                newmodelObject.transform.LookAt(new Vector3());
            }

            //設定世界做標
            _zodiacDatas[i].world_transform = newhip.transform;

            //設定剩餘參數
            _zodiacDatas[i].hip_name_obj = StarMapHUDController.ins.setZodiac_name(_zodiacDatas[i]);
            _zodiacDatas[i].hip_modelobj = newmodelObject;
            _zodiacDatas[i].hip_obj = newhip;
            
            /*
            if (_zodiacDatas[i].hip_modelobj)
            {
                Debug.Log(_zodiacDatas[i].name + " " + _zodiacDatas[i].hip_modelobj.transform.rotation);
            }
            */

            //將更改完的變數加入鏈結
            zodiacs.Add(_zodiacDatas[i]);
            
        }

        return zodiacs;
    }

    #endregion


    #region Public Method

    //在下方新增個別物件方法
    public void OpenZodiac(Zodiac_Data data)
    {
        data.hip_obj.SetActive(true);

        if (starMap.On3DRender)
        {
            data.hip_name_obj.SetActive(true);
        }

        AddZodiacListByName(data.name);
    }

    public void CloseZodiac(Zodiac_Data data)
    {
        data.hip_obj.SetActive(false);
        data.hip_name_obj.SetActive(false);

        RemoveZodiacListByName(data.name);
    }

    public void AddZodiacListByName(string str)
    {
        for (int i = 0; i < hipLineList_Remove.Count; i++)
        {
            if (hipLineList_Remove[i].constellationNameShort == str)
            {
                starMap.hipLineList.Add(new StarMap.HipLine(hipLineList_Remove[i].constellationNameShort,
                    hipLineList_Remove[i].sttData, hipLineList_Remove[i].endData));


                hipLineList_Remove.RemoveAt(i);
                i--;
            }
        }

    }

    public void RemoveZodiacListByName(string str)
    {
        for (int i = 0; i < starMap.hipLineList.Count; i++)
        {
            if (starMap.hipLineList[i].constellationNameShort == str)
            {
                hipLineList_Remove.Add(new StarMap.HipLine(starMap.hipLineList[i].constellationNameShort,
                    starMap.hipLineList[i].sttData, starMap.hipLineList[i].endData));



                starMap.hipLineList.RemoveAt(i);
                i--;
            }
        }

    }

    #endregion
}
