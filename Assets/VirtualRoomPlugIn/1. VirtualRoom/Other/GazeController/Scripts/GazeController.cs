using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeController : MonoBehaviour
{
    public enum PointViewType
    {
        Teacher,
        Student,
        None
    }
    public static GazePointView Tip_PointView;
    public static GameObject TipArrow;

    public static Canvas GazeCanvas;
    public static Camera adjustCamera;
    static Transform gazeControllerObjectTF;


    public PointViewType pointViewType;
    public string studentname = "";
    public bool isLocalPlayer = true;
    public bool isSmooth;
    public Vector3 point;

    public GazePointView Teacher_PointViewPrefab;
    public GazePointView Student_PointViewPrefab;
    public GazePointView Tip_PointViewPrefab;
    public GameObject TipArraowPrefab;
    

    //float radius = 100;


    [HideInInspector]
    public GazePointView currentPointView;
    Player player;
    
    // Use this for initialization
    void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    private void Start()
    {
        CreatObjectTF();
        CreatGazeCanvas();

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            CheckPoint();
            FollowTipPointView();
        }
        else
        {
            CreatGazePointView();
        }
        PointViewHandle();

    }
    private void OnDisable()
    {
        /*if(currentPointView!=null)
        currentPointView.gameObject.SetActive( false);*/
    }
    private void OnDestroy()
    {
        if (currentPointView != null)
            Destroy(currentPointView.gameObject);
        if (Tip_PointView != null)
            Destroy(Tip_PointView.gameObject);
        if (TipArrow != null)
            Destroy(TipArrow.gameObject);
    }

    #region  初始化生成
    void CreatObjectTF()
    {
        if (gazeControllerObjectTF != null) return;
        gazeControllerObjectTF = new GameObject("GazeController_Object").transform;

    }

    void CreatGazeCanvas()
    {
        if (GazeCanvas != null) return;
        GameObject go = new GameObject("GazeCanvas");
        go.transform.SetParent(gazeControllerObjectTF);
        go.layer = LayerMask.NameToLayer("UI");
        GazeCanvas = go.AddComponent<Canvas>();
        GazeCanvas.renderMode = RenderMode.WorldSpace;

    }
    #endregion

    void CreatGazePointView()
    {

        if (currentPointView != null) return;
        if (pointViewType == PointViewType.None) return;


        if (pointViewType == PointViewType.Teacher)
        {
            currentPointView = Instantiate(Teacher_PointViewPrefab);
        }
        else if (pointViewType == PointViewType.Student)
        {
            currentPointView = Instantiate(Student_PointViewPrefab);
        }

        currentPointView.transform.SetParent(GazeCanvas.transform);
        currentPointView.transform.position = point;
        currentPointView.gameObject.SetActive(true);

    }
    void PointViewHandle()
    {
        if (currentPointView != null)
        {
            if (player.playerName != "" && currentPointView.txtName.text == "學生")
            {
                currentPointView.txtName.text = player.playerName;
            }
            
            if (isSmooth)
            {
                currentPointView.transform.position = currentPointView.transform.position = Vector3.Lerp(currentPointView.transform.position, point, Time.deltaTime * 15);
            }
            else
            {
                currentPointView.transform.position = currentPointView.transform.position = point;
            }

            //currentPointView.transform.forward = Camera.main.transform.forward;
            currentPointView.transform.forward = currentPointView.transform.position - Camera.main.transform.position;
        }
        if (Tip_PointView != null)
        {
            //Tip_PointView.transform.forward = Camera.main.transform.forward;
            Tip_PointView.transform.forward = Tip_PointView.transform.position - Camera.main.transform.position;
        }


    }
    void CheckPoint()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        point = GazeSphere.RayHitOnSphere(ray);
    }


    public void SetPointViewName(string name)
    {
        if (currentPointView == null) return;
        currentPointView.name = name;

    }

    public void SetPointViewActive(bool value)
    {
        gameObject.SetActive(value);

        if (currentPointView != null)
        {
            currentPointView.gameObject.SetActive(value);
        }
    }


    public void SetTipPointView(Vector3 point)
    {
        if (Tip_PointView == null)
        {
            Tip_PointView = Instantiate(Tip_PointViewPrefab);
            Tip_PointView.transform.SetParent(GazeCanvas.transform);
            Tip_PointView.transform.position = point;
            Tip_PointView.gameObject.SetActive(true);
        }
        else
        {
            Tip_PointView.transform.position = point;
            Tip_PointView.gameObject.SetActive(true);
        }
    }
    
    Vector3 TipArrow_movePoint;
    Animator TipArrowAnim;

    void FollowTipPointView()
    {

        if (Tip_PointView == null || !Tip_PointView.gameObject.activeSelf) return;

        Vector3 screenPoint = Camera.main.WorldToViewportPoint(Tip_PointView.transform.position);

        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0.01 && screenPoint.x < 0.99 && screenPoint.y > 0.01 && screenPoint.y < 0.99;

        if (TipArrow == null)
        {

            if (!onScreen)
            {
                CheckTipArrow();
            }
            return;
        }

        if (TipArrowAnim == null)
        {
            TipArrowAnim = TipArrow.GetComponent<Animator>();
        }

        TipArrowAnim.SetBool("Show", onScreen);

        //TipArrow 轉向 TipView 
        Vector3 dir = Tip_PointView.transform.position - point;
        dir = Vector3.ProjectOnPlane(dir, Camera.main.transform.forward);
        TipArrow.transform.rotation = Quaternion.LookRotation(dir, Camera.main.transform.forward);

        
        //adjustCamera_Handle
        CheckOrthoGraphicCamera();
        adjustCamera.transform.position = Camera.main.transform.position;
        adjustCamera.transform.rotation = Camera.main.transform.rotation;

        //TipArrow 移動

        if (onScreen)
        {
            /*  Vector3 adjustCamera_screenPoint = Camera.main.WorldToViewportPoint(Tip_PointView.transform.position);
              Vector3 movePoint = new Vector3( Mathf.Clamp(adjustCamera_screenPoint.x, 0.4f, 0.6f),  Mathf.Clamp(adjustCamera_screenPoint.y, 0.4f, 0.6f));


              Vector3 mdir = movePoint - new Vector3(0.5f, 0.5f);

              movePoint = mdir.normalized * 0.1f + new Vector3(0.5f, 0.5f);

              TipArrow_movePoint = movePoint;
              Ray ray = Camera.main.ViewportPointToRay(TipArrow_movePoint);

              TipArrow.transform.position = GazeSphere.RayHitOnSphere(ray);*/

        }
        else
        {
            Vector3 adjustCamera_screenPoint = adjustCamera.WorldToViewportPoint(Tip_PointView.transform.position);
            Vector3 movePoint = new Vector3(Mathf.Clamp(adjustCamera_screenPoint.x, 0.01f, 0.99f), Mathf.Clamp(adjustCamera_screenPoint.y, 0.01f, 0.99f));

            Vector3 mdir = movePoint - new Vector3(0.5f, 0.5f);

            movePoint = mdir.normalized * 0.1f + new Vector3(0.5f, 0.5f);

            TipArrow_movePoint = Vector3.Lerp(TipArrow_movePoint, movePoint, Time.deltaTime * 3);
            Ray ray = Camera.main.ViewportPointToRay(TipArrow_movePoint);
            TipArrow.transform.position = GazeSphere.RayHitOnSphere(ray);
        }

    }

    Camera CheckOrthoGraphicCamera()
    {
        if (adjustCamera == null)
        {
            GameObject go = new GameObject("GazeOrthoGraphicCamera");
            go.transform.SetParent(gazeControllerObjectTF);
            adjustCamera = go.AddComponent<Camera>();
            adjustCamera.orthographic = true;
            adjustCamera.depth = -10;
            adjustCamera.orthographicSize = 45;
        }
        return adjustCamera;
    }
    GameObject CheckTipArrow()
    {
        if (TipArrow == null)
        {
            GameObject go = Instantiate(TipArraowPrefab);
            go.transform.SetParent(GazeCanvas.transform);
            TipArrow = go;
            return TipArrow;
        }
        else
        {
            return TipArrow;
        }
    }


    void TipArrowHandle()
    {
        if (TipArrow == null) return;
        TipArrow.transform.position = point;
    }

    /*
    //印出player的視線座標
    private void OnGUI()
    {
        GUI.Box(new Rect(10, 130, 150, 150), "GazeController");
        GUI.Label(new Rect(20, 150, 300, 20), "座標 : " + point.ToString());
    }
    */
}
