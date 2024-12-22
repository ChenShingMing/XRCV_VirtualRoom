using UnityEngine;
using Sirenix.OdinInspector;

public class EarthOrbit : MonoBehaviour
{
    [Header("Orbit Settings")]
    [Tooltip("Reference to the Sun's position.")]
    public Transform sun;

    [Tooltip("Radius of Earth's orbit around the Sun.")]
    public float orbitRadius = 10f;

    [Tooltip("Speed of Earth's orbit (degrees per second).")]
    public float orbitSpeed = 30f;

    [Tooltip("Start from a specific month (1 = January, 12 = December).")]
    [Range(1, 12)]
    public int startingMonth = 1;

    [Tooltip("Enable or disable automatic orbiting.")]
    public bool autoOrbit = true; // 控制是否自動公轉

    private float currentAngle; // Current angle in degrees

    void Start()
    {
        // 初始化位置到指定的月份
        SetMonthPosition(startingMonth);
    }

    void Update()
    {
        if (autoOrbit)
        {
            // 自動更新角度，模擬地球逆時針公轉
            currentAngle += orbitSpeed * Time.deltaTime;
            currentAngle %= 360; // 確保角度在 0~360 度之間

            // 設置地球在軌道上的位置
            UpdateEarthPosition();
        }
    }

    /// <summary>
    /// 更新地球在軌道上的位置。
    /// </summary>
    private void UpdateEarthPosition()
    {
        if (sun == null) return;

        // 計算地球的世界座標位置
        float radians = currentAngle * Mathf.Deg2Rad; // 將角度轉換為弧度
        float x = sun.position.x + Mathf.Cos(radians) * orbitRadius;
        float z = sun.position.z + Mathf.Sin(radians) * orbitRadius;
        transform.position = new Vector3(x, transform.position.y, z);
    }

    [Button]
    /// <summary>
    /// 根據指定的月份設置地球的位置。
    /// </summary>
    /// <param name="month">月份 (1 = 一月, 12 = 十二月)</param>
    public void SetMonthPosition(int month)
    {
        if (month < 1 || month > 12)
        {
            Debug.LogError("Month must be between 1 and 12.");
            return;
        }

        // 計算對應月份的角度
        // 12 月為 0 度，3 月為 90 度，6 月為 180 度，9 月為 270 度
        // 並依此逆時針排列
        float anglePerMonth = 30f; // 每月對應的角度，30 度
        currentAngle = (month * anglePerMonth); // 調整基準，讓 3 月在 90 度

        // 更新地球在軌道上的位置
        UpdateEarthPosition();
    }


    [Button]
    public void SetAutoOrbit(bool _value)
    {
        autoOrbit = _value;
    }

}