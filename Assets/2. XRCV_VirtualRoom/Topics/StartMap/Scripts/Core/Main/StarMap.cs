using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class StarMap : MonoBehaviour
{
    #region inner Class

    //數據類型的定義
    public struct HipData
    {
        public int hipId;
        public Vector3 pos;
        public Color color;
        public float magnitude; // 等級
        public float parallax; // 視差
        public HipData(int _id, Vector3 _pos, Color _color, float _magnitude, float _parallax)
        {
            hipId = _id;
            pos = _pos;
            color = _color;
            magnitude = _magnitude;
            parallax = _parallax;
        }
        public HipData(HipData _data)
        {
            hipId = _data.hipId;
            pos = _data.pos;
            color = _data.color;
            magnitude = _data.magnitude;
            parallax = _data.parallax;
        }
    }

    //星座線數據類型的定義
    public struct HipLine
    {
        public string constellationNameShort;
        public HipData sttData;
        public HipData endData;
        public HipLine(string _name, HipData _sttData, HipData _endData)
        {
            constellationNameShort = _name;
            sttData = _sttData;
            endData = _endData;
        }
    }

    #endregion


    //要讀取的數據
    [FoldoutGroup("資料設置")]
    [SerializeField]
    public TextAsset starFile = null;
    [FoldoutGroup("資料設置")]
    [SerializeField]
    public TextAsset lineFile = null;

    [FoldoutGroup("物件設置")]
    public ParticleSystem starParticle;
    [FoldoutGroup("物件設置")]
    public CreateHipHierarchy createHipHierarchy;
    [FoldoutGroup("物件設置")]
    public Transform starLineRenderParent;
    [FoldoutGroup("物件設置")]
    public GameObject starLineRenderPrefab;


    //恆星的距離（相等）
    [FoldoutGroup("控制數值")]
    public float distance = 100f;
    [FoldoutGroup("控制數值")]
    public bool OnLineRender = false; //開啟關閉連線
    [FoldoutGroup("控制數值")]
    public bool On3DRender = false; //開啟關閉圖示


    public List<HipLine> hipLineList; //存儲可顯示連線數據的列表


    List<HipData> hipList; //存儲讀取數據的列表
    

    GameObject zodiacs_GameObject;
    bool _close_zodiacs_flag = false;

    List<LineRenderer> starLineRenderers;
    internal bool _linesDirty = true;  // CreateHipHierarchy 增刪連線時設為 true
    bool _prevOnLineRender = false;

    #region MonoBehavior

    private void Awake()
    {
        //初始化各項陣列
        hipList = CreateHipList(starFile);
        //從csv讀出所有連線的資訊，為所有連線
        hipLineList = CreateHipLineList(lineFile, hipList);

        createHipHierarchy.ScanHipData();
        zodiacs_GameObject = createHipHierarchy.hip_Parent;

        starLineRenderers = new List<LineRenderer>(hipLineList.Count);

        //產生連線LineRenderer的物件，數量對齊 CSV 總行數
        for (int i = 0; i < hipLineList.Count; i++)
        {
            starLineRenderers.Add(Instantiate(starLineRenderPrefab, starLineRenderParent).GetComponent<LineRenderer>());
        }
    }

    // Use this for initialization
    private void OnEnable()
    {
        ShowStarMap();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Show3DModelHandle();
        StarLinkLineRenderHandle(hipLineList, distance);
    }

    #endregion

    #region Private Method

    [Button]
    void ShowStarMap()
    {
        SetEfcStars(hipList, starParticle, distance);
    }

    void Show3DModelHandle()
    {
        if (zodiacs_GameObject == null) return;

        if (On3DRender)
        {
            //Debug.Log("picturerender true");
            if (!zodiacs_GameObject.activeSelf)
            {
                zodiacs_GameObject.gameObject.SetActive(true);
                _close_zodiacs_flag = false;
            }
        }
        else
        {
            //Debug.Log("picturerender false");
            if (zodiacs_GameObject.gameObject.activeSelf)
            {
                zodiacs_GameObject.gameObject.SetActive(false);

                if (!_close_zodiacs_flag)
                {
                    for (int i = 0; i < createHipHierarchy.zodiac_Datas.Count; i++)
                    {
                        createHipHierarchy.zodiac_Datas[i].hip_name_obj.SetActive(false);
                    }
                    _close_zodiacs_flag = true;
                }

            }
        }
    }

    void StarLinkLineRenderHandle(List<HipLine> lineList, float distance)
    {
        if (OnLineRender)
        {
            if (lineList == null) return;

            if (!_prevOnLineRender)
            {
                starLineRenderParent.gameObject.SetActive(true);
                _prevOnLineRender = true;
                _linesDirty = true;
            }

            if (!_linesDirty) return;
            _linesDirty = false;

            // 擴容：修正原本反向計算導致擴容永遠不執行的 bug
            int deficit = lineList.Count - starLineRenderers.Count;
            if (deficit > 0)
            {
                for (int j = 0; j < deficit; j++)
                    starLineRenderers.Add(Instantiate(starLineRenderPrefab, starLineRenderParent).GetComponent<LineRenderer>());
            }

            for (int i = 0; i < starLineRenderers.Count; ++i)
            {
                if (i < lineList.Count)
                {
                    starLineRenderers[i].SetPosition(0, lineList[i].sttData.pos * distance);
                    starLineRenderers[i].SetPosition(1, lineList[i].endData.pos * distance);
                }
                else
                {
                    starLineRenderers[i].SetPosition(0, Vector3.zero);
                    starLineRenderers[i].SetPosition(1, Vector3.zero);
                }
            }
        }
        else
        {
            if (_prevOnLineRender)
            {
                starLineRenderParent.gameObject.SetActive(false);
                _prevOnLineRender = false;
            }
        }
    }

    #region Create Star Map

    #region Data Mapping

    //從文件中讀取數據並將其存儲在數據列表中
    List<HipData> CreateHipList(TextAsset _lightsFile)
    {
        List<HipData> list = new List<HipData>();
        StringReader sr = new StringReader(_lightsFile.text);
        while (sr.Peek() > -1)
        {
            string lineStr = sr.ReadLine();
            HipData data;

            if (StringToHipData(lineStr, out data))
            {
                list.Add(data);
            }
        }
        sr.Close();
        return list;
    }

    //從文件讀取數據並將其存儲在行數據列表中
    List<HipLine> CreateHipLineList(TextAsset _linesFile, List<HipData> _hipList)
    {
        List<HipLine> list = new List<HipLine>();
        StringReader sr = new StringReader(_linesFile.text);
        while (sr.Peek() > -1)
        {
            string lineStr = sr.ReadLine();
            HipLine data;

            if (StringToHipLine(lineStr, _hipList, out data))
            {
                list.Add(data);
            }
        }
        sr.Close();
        return list;
    }


    //將CSV字符串轉換為數據類型
    bool StringToHipData(string _hipStr, out HipData data)
    {
        bool ret = true;
        data = new HipData();
        // 將逗號分隔的數據轉換為字符串數組
        string[] dataArr = _hipStr.Split(',');
        try
        {
            //將字符串轉換為int，float
            int hipId = int.Parse(dataArr[0]);

            //赤經(時)
            float hlH = float.Parse(dataArr[1]);
            //赤經(分)
            float hlM = float.Parse(dataArr[2]);
            //赤經(秒)
            float hlS = float.Parse(dataArr[3]);
            //赤緯(符號)
            int hsSgn = int.Parse(dataArr[4]);
            //赤緯(時)
            float hsH = float.Parse(dataArr[5]);
            //赤緯(分)
            float hsM = float.Parse(dataArr[6]);
            //赤緯(秒)
            float hsS = float.Parse(dataArr[7]);
            //視星等
            float mag = float.Parse(dataArr[8]);

            //Color col = Color.gray;
            float hDeg = (360f / 24f) * (hlH + hlM / 60f + hlS / 3600f);
            float sDeg = (hsH + hsM / 60f + hsS / 3600f) * (hsSgn == 0 ? -1f : 1f);
            Quaternion rotL = Quaternion.AngleAxis(hDeg, Vector3.up);
            Quaternion rotS = Quaternion.AngleAxis(sDeg, Vector3.right);
            Vector3 pos = rotL * rotS * Vector3.forward;
            float parallax = 0f;
            if (dataArr.Length > 9)
            {   // parallax
                float.TryParse(dataArr[9], out parallax);
            }
            data = new HipData(hipId, pos, BVColorTest.Bv2rgb(float.Parse(dataArr[12])), mag, parallax);
        }
        catch
        {
            ret = false;
            Debug.Log("data err");
        }
        return ret;
    }

    //將CSV字符串轉換為線數據類型
    bool StringToHipLine(string _hipLineStr, List<HipData> _hipList, out HipLine data)
    {
        bool ret = true;
        data = new HipLine();
        string[] dataArr = _hipLineStr.Split(',');
        string shortName = dataArr[0];
        try
        {
            int sttId = int.Parse(dataArr[1]);
            int endId = int.Parse(dataArr[2]);
            HipData sttData = _hipList.First(d => (d.hipId == sttId));
            HipData endData = _hipList.First(d => (d.hipId == endId));
            data = new HipLine(shortName, sttData, endData);
        }
        catch
        {
            ret = false;
            //Debug.Log("linedataerr:" + shortName);
        }
        return ret;
    }

    #endregion

    //用顆粒顯示星星
    void SetEfcStars(List<HipData> hipList, ParticleSystem particleSystem, float distance)
    {
        if (hipList == null) return;
        if (particleSystem == null) return;

        particleSystem.Clear();

        ParticleSystem.MainModule pmm = particleSystem.main;
        pmm.maxParticles = 2000000;
        pmm.simulationSpace = ParticleSystemSimulationSpace.Local;
        pmm.scalingMode = ParticleSystemScalingMode.Hierarchy;

        particleSystem.Emit(hipList.Count);
        ParticleSystem.Particle[] stars = new ParticleSystem.Particle[hipList.Count];
        particleSystem.GetParticles(stars);

        for (int i = 0; i < hipList.Count; ++i)
        {
            // 人類最小可看到星等，數值越小越大
            if (hipList[i].magnitude < 6.5f)
            {
                stars[i].position = hipList[i].pos * distance;

                stars[i].startSize = SetStarSize(hipList[i].magnitude);
                stars[i].startColor = hipList[i].color;  // 修正了 `color` 成 `startColor`
            }
            else
            {
                stars[i].remainingLifetime = 0;
            }
        }

        particleSystem.Play();
        particleSystem.SetParticles(stars, hipList.Count);
    }

    float SetStarSize(float mag)
    {
        if (mag < -1.3f)
        {
            return 15f;
        }

        if (mag < 0)
        {
            return 10f;
        }

        if (mag > 0 && mag < 1)
        {
            return 6.5f;
        }

        if (mag > 1 && mag < 4)
        {
            return 2.5f;
        }

        //return 1 + (6.5f - mag) * 8 / 7.94f;

        if (mag > 4 && mag < 5.5)
        {
            return 0.65f;
        }

        return 0.45f;
    }

    #endregion

    #endregion
}