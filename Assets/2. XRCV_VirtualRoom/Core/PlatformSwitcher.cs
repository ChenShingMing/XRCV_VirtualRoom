using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class PlatformSwitcher : SerializedMonoBehaviour
{
    public enum Platform
    {
        PC,
        OpenXR
    }


    [FoldoutGroup("參數設置")]
    [SerializeField]
    public Dictionary<Platform, PlatformSetting> platformSwitchSettings;

    [EnumToggleButtons]
    [OnValueChanged("SwitchPlatform")]
    public Platform platform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnSwitchToPC()
    {
        // PC 切換邏輯
        Debug.Log("PlatformSwitcher Switched to PC.");
        platform = Platform.PC;
        SwitchPlatform();
    }

    public void OnSwitchToOpenXR()
    {
        // OpenXR 切換邏輯
        Debug.Log("PlatformSwitcher Switched to OpenXR.");
        platform = Platform.OpenXR;
        SwitchPlatform();
    }



    void SwitchPlatform()
    {

        foreach (KeyValuePair<Platform, PlatformSetting> settins in platformSwitchSettings)
        {
            if (settins.Key == platform)
            {
                settins.Value.SetEnable(true);
            }
            else
            {
                settins.Value.SetEnable(false);
            }
        }

        Debug.Log("Switch To " + platform.ToString());
    }

    [System.Serializable]
    public class PlatformSetting
    {
        [FoldoutGroup("物件設置")]
        public InputHandler inputHandler;


        [FoldoutGroup("物件設置")]
        public List<GameObject> activeGameObjects = new List<GameObject>();

        public void SetEnable(bool value)
        {
            inputHandler.gameObject.SetActive(value);

            for (int i = 0; i < activeGameObjects.Count; i++)
            {
                activeGameObjects[i].SetActive(value);
            }
        }

    }
}
