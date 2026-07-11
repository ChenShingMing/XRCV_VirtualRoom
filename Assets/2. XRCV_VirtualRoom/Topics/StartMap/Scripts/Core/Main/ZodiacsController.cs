using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZodiacsController : MonoBehaviour
{
    public CreateHipHierarchy createHipHierarchy;

    [Tooltip("星座可見性更新間隔（秒）")]
    public float checkInterval = 0.1f;

    private float _timer = 0f;
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < checkInterval) return;
        _timer = 0f;
        CheckAllZodiacsStatus();
    }

    public void CheckAllZodiacsStatus()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        for (int i = 0; i < createHipHierarchy.zodiac_Datas.Count; i++)
        {
            Vector3 pos = _mainCamera.WorldToViewportPoint(createHipHierarchy.zodiac_Datas[i].world_transform.position);

            if (pos.x < 0.8f && pos.x > 0.2f
                && pos.y < 0.9f && pos.y > 0.1f)
            {
                createHipHierarchy.OpenZodiac(createHipHierarchy.zodiac_Datas[i]);
            }
            else
            {
                createHipHierarchy.CloseZodiac(createHipHierarchy.zodiac_Datas[i]);
            }
        }
    }
}
