using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//public class LaserScalePointer : OVRCursor
public class LaserScalePointer :MonoBehaviour
{
    /*
    public enum LaserBeamBehavior
    {
        On,        // laser beam always on
        Off,        // laser beam always off
        OnWhenHitTarget,  // laser beam only activates when hit valid target
    }

    public static bool isHitTarget;

    public GameObject cursorVisual;
    public float maxLength = 10.0f;
    public float scale;

    private LaserBeamBehavior _laserBeamBehavior;
    bool m_restoreOnInputAcquired = false;

    public LaserBeamBehavior laserBeamBehavior
    {
        set
        {
            _laserBeamBehavior = value;
            if (laserBeamBehavior == LaserBeamBehavior.Off || laserBeamBehavior == LaserBeamBehavior.OnWhenHitTarget)
            {
                lineRenderer.enabled = false;
            }
            else
            {
                lineRenderer.enabled = true;
            }
        }
        get
        {
            return _laserBeamBehavior;
        }
    }
    private Vector3 _startPoint;
    private Vector3 _forward;
    private Vector3 _endPoint;
    private bool _hitTarget;
    private LineRenderer lineRenderer;

    private Vector3 _lineRenderStartPoint;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        if (cursorVisual) cursorVisual.gameObject.SetActive(false);
        OVRManager.InputFocusAcquired += OnInputFocusAcquired;
        OVRManager.InputFocusLost += OnInputFocusLost;
    }

    public override void SetCursorStartDest(Vector3 start, Vector3 dest, Vector3 normal)
    {
        _startPoint = start;
        _endPoint = dest;
        _hitTarget = true;

        _lineRenderStartPoint = _startPoint + ((_endPoint - _startPoint).normalized * 0.04f);

        LaserScalePointer.isHitTarget = true;
    }

    public override void SetCursorRay(Transform t)
    {
        _startPoint = t.position;
        _forward = t.forward;
        _hitTarget = false;

        _lineRenderStartPoint = t.position + (t.forward * 0.04f);

        LaserScalePointer.isHitTarget = false;
    }

    private void LateUpdate()
    {
        lineRenderer.SetPosition(0, _lineRenderStartPoint);
        if (_hitTarget)
        {
            lineRenderer.SetPosition(1, _endPoint);
            UpdateLaserBeam(_lineRenderStartPoint, _endPoint);

            if (cursorVisual)
            {
                cursorVisual.transform.position = _endPoint;

                float dis = Vector3.Distance(cursorVisual.transform.position, Camera.main.transform.position);
                cursorVisual.transform.localScale = new Vector3(scale * dis, scale * dis, scale * dis);

                cursorVisual.SetActive(true);
            }
        }
        else
        {
            UpdateLaserBeam(_lineRenderStartPoint, _lineRenderStartPoint + maxLength * _forward);
            lineRenderer.SetPosition(1, _lineRenderStartPoint + maxLength * _forward);
            if (cursorVisual) cursorVisual.SetActive(false);
        }
    }

    // make laser beam a behavior with a prop that enables or disables
    private void UpdateLaserBeam(Vector3 start, Vector3 end)
    {
        if (laserBeamBehavior == LaserBeamBehavior.Off)
        {
            return;
        }
        else if (laserBeamBehavior == LaserBeamBehavior.On)
        {
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
        else if (laserBeamBehavior == LaserBeamBehavior.OnWhenHitTarget)
        {
            if (_hitTarget)
            {
                if (!lineRenderer.enabled)
                {
                    lineRenderer.enabled = true;
                    lineRenderer.SetPosition(0, start);
                    lineRenderer.SetPosition(1, end);
                }
            }
            else
            {
                if (lineRenderer.enabled)
                {
                    lineRenderer.enabled = false;
                }
            }
        }
    }

    void OnDisable()
    {
        if (cursorVisual) cursorVisual.SetActive(false);
    }
    public void OnInputFocusLost()
    {
        if (gameObject && gameObject.activeInHierarchy)
        {
            m_restoreOnInputAcquired = true;
            gameObject.SetActive(false);
        }
    }

    public void OnInputFocusAcquired()
    {
        if (m_restoreOnInputAcquired && gameObject)
        {
            m_restoreOnInputAcquired = false;
            gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        OVRManager.InputFocusAcquired -= OnInputFocusAcquired;
        OVRManager.InputFocusLost -= OnInputFocusLost;
    }

    */
}
