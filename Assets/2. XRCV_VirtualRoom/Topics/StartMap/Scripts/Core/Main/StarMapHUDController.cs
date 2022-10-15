using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarMapHUDController : MonoBehaviour
{

    public GameObject ScreenCanvas;
    public GameObject WorldCanvs;
    public GameObject zodiac_nameText;
    public GameObject zodiac_nameTextSpecial;
    public Text DeBugText;
    public Text ActionText;

    public float textLerpSpeed;

    public static StarMapHUDController ins;
    RectTransform canvasRT;
    List<Zodiac_name_info> zodiac_name_infoList =new List<Zodiac_name_info>();

    string[] specialStar = {
        "北極星",
        "天狼星"
    };

    private void Awake()
    {
        ins = this;
        canvasRT = GetComponent<RectTransform>();
    }
    // Use this for initialization
    void Start() 
    {
        
        if(GameObject.FindGameObjectWithTag("UICamera") == null)
        {
            ScreenCanvas.GetComponent<Canvas>().worldCamera = Camera.main;
            WorldCanvs.GetComponent<Canvas>().worldCamera = Camera.main;
        }
        else
        {
            Camera uiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
            ScreenCanvas.GetComponent<Canvas>().worldCamera = uiCamera;
            WorldCanvs.GetComponent<Canvas>().worldCamera = uiCamera;
        }
        
        
    }

    // Update is called once per frame
    void Update() {
        
        Zodiac_nameHandle();
    }

    public GameObject setZodiac_name(CreateHipHierarchy.Zodiac_Data zodiac_Data)
    {

        //生成
        Zodiac_name_info info = new Zodiac_name_info();
        GameObject temp;
        
        if(IsSpecialStar(zodiac_Data.name))
        {
            temp = Instantiate(zodiac_nameTextSpecial, transform.position, transform.rotation);
        }
        else
        {
            temp = Instantiate(zodiac_nameText, transform.position, transform.rotation);
        }
        

        //Info給予
        info.obj = temp;
        info.zodiac_Data = zodiac_Data;
        info.rt = temp.GetComponentInChildren<RectTransform>();

        //info Handle
        info.obj.transform.parent = WorldCanvs.transform;
        info.obj.GetComponentInChildren<Text>().text = zodiac_Data.name;
        
        info.obj.transform.position = info.zodiac_Data.world_transform.position;
        info.obj.transform.localScale = Vector3.one;
        info.obj.SetActive(false);
        //  info.rt.anchoredPosition = WorldToCanvasPosition(zodiac_Data.center_pos, canvasRT, Camera.main);

        zodiac_name_infoList.Add(info);

        return temp;

    }
    void Zodiac_nameHandle() {
      
        for (int i = 0; i < zodiac_name_infoList.Count; i++) {
            
           zodiac_name_infoList[i].obj.transform.position = Vector3.Lerp(zodiac_name_infoList[i].obj.transform.position,
               zodiac_name_infoList[i].zodiac_Data.world_transform.position,
               textLerpSpeed * Time.deltaTime);

           zodiac_name_infoList[i].obj.transform.forward = Camera.main.transform.forward;
         
        }
    }

    bool IsSpecialStar(string _name)
    {
        for (int i = 0; i < specialStar.Length; i++)
        {
            if (_name == specialStar[i])
            {
                return true;
            }
        }
        return false;
    }

    static public Vector2 WorldToCanvasPosition(Vector3 position, RectTransform canvas, Camera cam = null)
    {

      
            //Vector position (percentage from 0 to 1) considering camera size.
            //For example (0,0) is lower left, middle is (0.5,0.5)
            Vector2 temp = cam.WorldToViewportPoint(position);

            //Calculate position considering our percentage, using our canvas size
            //So if canvas size is (1100,500), and percentage is (0.5,0.5), current value will be (550,250)
            temp.x *= canvas.sizeDelta.x;
            temp.y *= canvas.sizeDelta.y;

            //The result is ready, but, this result is correct if canvas recttransform pivot is 0,0 - left lower corner.
            //But in reality its middle (0.5,0.5) by default, so we remove the amount considering cavnas rectransform pivot.
            //We could multiply with constant 0.5, but we will actually read the value, so if custom rect transform is passed(with custom pivot) , 
            //returned value will still be correct.

            temp.x -= canvas.sizeDelta.x * canvas.pivot.x;
            temp.y -= canvas.sizeDelta.y * canvas.pivot.y;

            return temp;
        
    }

    class Zodiac_name_info{

        public GameObject obj;
        public CreateHipHierarchy.Zodiac_Data zodiac_Data;
        public RectTransform rt;
    }
}
