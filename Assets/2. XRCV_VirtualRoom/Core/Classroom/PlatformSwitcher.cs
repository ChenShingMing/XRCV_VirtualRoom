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
        OpenXR,
        Quest   // Quest 目前共用 OpenXR 的 PlatformSetting，保留 enum 供未來獨立配置
    }

    [FoldoutGroup("物件設置")]
    public ClassroomManager classroomManager;

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

    public void OnSwitchToOculus()
    {
        Debug.Log("PlatformSwitcher Switched to Quest.");
        platform = Platform.Quest;
        SwitchPlatform();
    }


    void SwitchPlatform()
    {
        // Quest 目前沒有獨立設置，共用 OpenXR PlatformSetting
        Platform key = (platform == Platform.Quest && !platformSwitchSettings.ContainsKey(Platform.Quest))
            ? Platform.OpenXR
            : platform;

        classroomManager.mainUICanvas_Current = platformSwitchSettings[key].mainUIGameObject;
        classroomManager.inputActionManager.SetCurrentInputHandler(platformSwitchSettings[key].inputHandler);

        foreach (KeyValuePair<Platform, PlatformSetting> setting in platformSwitchSettings)
        {
            setting.Value.SetEnable(setting.Key == key);
        }

        Debug.Log("Switch To " + platform.ToString());
    }

    [System.Serializable]
    public class PlatformSetting
    {
        [FoldoutGroup("物件設置")]
        public GameObject mainUIGameObject;
        [FoldoutGroup("物件設置")]
        public InputHandler inputHandler;


        [FoldoutGroup("物件設置")]
        public Camera platformCamera;

        [FoldoutGroup("物件設置")]
        public List<GameObject> activeGameObjects = new List<GameObject>();

        public void SetEnable(bool value)
        {
            mainUIGameObject.SetActive(value);
            inputHandler.gameObject.SetActive(value);

            if (platformCamera != null)
                platformCamera.enabled = value;

            for (int i = 0; i < activeGameObjects.Count; i++)
            {
                activeGameObjects[i].SetActive(value);
            }
        }

    }
}
