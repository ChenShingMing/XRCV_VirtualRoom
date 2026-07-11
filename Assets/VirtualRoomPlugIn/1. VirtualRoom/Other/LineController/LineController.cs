using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    private GameObject lineRendererObj;
    private LineRenderer lineRenderer;
    public List<Vector3> worldPos;
    public GameObject lineRendererPre;
    public GameObject linepoint;

    private int drawpoint_count = 0;


    public float penWidth;
    public Color penColor;

    public Vector3 pos;
    public bool moving;

    private void Awake()
    {
        penWidth = 0.05f;
        penColor = Color.green;
    }


    private void FixedUpdate()
    {
        if (moving)
        {
            if(pos == new Vector3())
            {
                return;
            }


            if (worldPos.Count > 1)
            {
                if (Vector3.Distance(pos, worldPos[worldPos.Count - 1]) < 0.15f)
                {
                    return;
                }
            }


            worldPos.Add(pos);
            Draw();

        }
    }

    public void InitLine()
    {
        lineRendererObj = Instantiate(lineRendererPre, this.transform);
        lineRenderer = lineRendererObj.GetComponent<LineRenderer>();
        lineRenderer.numCapVertices = 5;//控制在結尾處添加的點數越多越圓滑
        lineRenderer.numCornerVertices = 5;//控制在折角處添加的點數越多越圓滑
        lineRenderer.startColor = penColor;//開始的顏色
        lineRenderer.endColor = penColor;//結束的顏色 
        lineRenderer.startWidth = penWidth;//開始的寬度
        lineRenderer.endWidth = penWidth;//結束的寬度
    }
    public void Draw()
    {
        for (int i = drawpoint_count; i < worldPos.Count; i ++)
        {
            GameObject _pos = Instantiate(linepoint) as GameObject;
            _pos.transform.SetParent(lineRenderer.gameObject.transform);
            _pos.transform.position = worldPos[i];
        }

        drawpoint_count = worldPos.Count;
        //lineRenderer.positionCount = worldPos.Count;
        //lineRenderer.SetPositions(worldPos.ToArray());
    }
    public void PenClear()
    {
        //清除
        Debug.Log("canvas clear");
        LineRenderer[] lines = GetComponentsInChildren<LineRenderer>();
        int count = lines.Length;

        for (int i = 0; i < count; i++)
        {
            Destroy(lines[i].gameObject);
        }

        drawpoint_count = 0;
    }

}
