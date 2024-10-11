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


    void SwitchPlatform()
    {

        classroomManager.mainUICanvas_Current = platformSwitchSettings[platform].mainUIGameObject;
        classroomManager.inputActionManager.SetCurrentInputHandler(platformSwitchSettings[platform].inputHandler);

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
        public GameObject mainUIGameObject;
        [FoldoutGroup("物件設置")]
        public InputHandler inputHandler;


        [FoldoutGroup("物件設置")]
        public List<GameObject> activeGameObjects = new List<GameObject>();

        public void SetEnable(bool value)
        {
            mainUIGameObject.SetActive(value);
            inputHandler.gameObject.SetActive(value);

            for (int i = 0; i < activeGameObjects.Count; i++)
            {
                activeGameObjects[i].SetActive(value);
            }
        }

    }
}
