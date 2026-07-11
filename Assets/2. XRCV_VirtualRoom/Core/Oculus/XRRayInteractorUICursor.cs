using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class XRRayInteractorUICursor : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public GameObject cursorInstance;
    public XRInteractorLineVisual lineVisual;

    private Camera _mainCamera;
    private float _defaultLineLength;

    void Start()
    {
        _mainCamera = Camera.main;
        cursorInstance.SetActive(false);
        if (lineVisual != null)
            _defaultLineLength = lineVisual.lineLength;
    }

    void Update()
    {
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        if (rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult raycastResult)
            && raycastResult.gameObject != null)
        {
            cursorInstance.SetActive(true);

            // Offset 1mm toward camera so cursor renders in front of the canvas surface
            Vector3 towardCamera = (_mainCamera.transform.position - raycastResult.worldPosition).normalized;
            cursorInstance.transform.position = raycastResult.worldPosition + towardCamera * 0.001f;
            FaceCamera(cursorInstance.transform);

            // Cap line at UI hit distance to prevent visual penetration
            if (lineVisual != null)
                lineVisual.lineLength = Mathf.Max(0.05f, raycastResult.distance);
        }
        else
        {
            cursorInstance.SetActive(false);
            if (lineVisual != null)
                lineVisual.lineLength = _defaultLineLength;
        }
    }

    private void FaceCamera(Transform cursorTransform)
    {
        Vector3 directionToCamera = cursorTransform.position - _mainCamera.transform.position;
        cursorTransform.rotation = Quaternion.LookRotation(directionToCamera);
    }
}
