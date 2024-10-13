using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using System.Linq;
using UnityEngine.XR.OpenXR;
using Unity.XR.Oculus;
using System.IO;
using UnityEditor.SceneManagement;

public class PlatformBuilder : EditorWindow
{
    private string buildFileName;
    private string dateFolder;

    [MenuItem("PlatformBuilder/Builder")]
    public static void ShowWindow()
    {
        GetWindow<PlatformBuilder>("Builder");
    }

    private void OnEnable()
    {
        buildFileName = "creativeXRworld";
        dateFolder = System.DateTime.Now.ToString("yyyyMMdd");
    }

    void OnGUI()
    {
        GUILayout.Label("PlatformBuilder", EditorStyles.boldLabel);

        buildFileName = EditorGUILayout.TextField("Export File Name:", buildFileName);

        if (GUILayout.Button("Switch to PC"))
        {
            SwitchToPC();
        }

        if (GUILayout.Button("Switch to Oculus"))
        {
            SwitchToOculusForAndroid();
        }

        if (GUILayout.Button("Switch to OpenXR"))
        {
            SwitchToOpenXRForAndroid();
        }

        /*

        if (GUILayout.Button("Build PC"))
        {
            if (string.IsNullOrEmpty(buildFileName))
            {
                Debug.LogError("Build file name cannot be empty!");
                return;
            }

            BuildPC(buildFileName);
        }

        if (GUILayout.Button("Build Oculus"))
        {
            if (string.IsNullOrEmpty(buildFileName))
            {
                Debug.LogError("Build file name cannot be empty!");
                return;
            }

            BuildOculusForAndroid(buildFileName);
        }

        if (GUILayout.Button("Build OpenXR"))
        {
            if (string.IsNullOrEmpty(buildFileName))
            {
                Debug.LogError("Build file name cannot be empty!");
                return;
            }

            BuildOpenXRForAndroid(buildFileName);
        }

        if (GUILayout.Button("Build All"))
        {
            BuildAllPlatforms();
        }
*/
    }

    // 添加指定平台（例如Android）的XRLoader
    public void AddLoaderForAndroid<T>() where T : XRLoader
    {
        var xrManager = GetXRManagerForBuildTarget(BuildTargetGroup.Android);
        T loader = xrManager.activeLoaders.FirstOrDefault(l => l is T) as T;

        if (loader == null)
        {
            loader = (T)System.Activator.CreateInstance(typeof(T));
            xrManager.TryAddLoader(loader);
            Debug.Log($"{typeof(T)} added to XR manager for Android.");
        }
    }

    // 移除指定平台（例如Android）的XRLoader
    public void RemoveLoaderForAndroid<T>() where T : XRLoader
    {
        var xrManager = GetXRManagerForBuildTarget(BuildTargetGroup.Android);
        T loader = xrManager.activeLoaders.FirstOrDefault(l => l is T) as T;

        if (loader != null)
        {
            xrManager.TryRemoveLoader(loader);
            Debug.Log($"{typeof(T)} removed from XR manager for Android.");
        }
        else
        {
            Debug.LogWarning($"{typeof(T)} not found in XR manager for Android.");
        }
    }

    // 通過平台獲取XR管理器
    private XRManagerSettings GetXRManagerForBuildTarget(BuildTargetGroup buildTargetGroup)
    {
        var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
        return generalSettings.Manager;
    }

    // 保存當前場景
    private void SaveCurrentScene()
    {
        if (!EditorApplication.isPlaying)
        {
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("Current scene saved.");
            }
        }
    }

    // 切換到PC（PC平台）
    public void SwitchToPC()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        Debug.Log("Switched to PC platform (Windows).");
        CallPlatformSwitcherMethod("OnSwitchToPC");
        SaveCurrentScene(); // 保存場景
    }

    // 切換到OpenXR（Android平台）
    public void SwitchToOpenXRForAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        RemoveLoaderForAndroid<OculusLoader>();
        AddLoaderForAndroid<OpenXRLoader>();
        Debug.Log("Switched to OpenXR for Android platform.");
        CallPlatformSwitcherMethod("OnSwitchToOpenXR");
        SaveCurrentScene(); // 保存場景
    }

    // 切換到Oculus（Android平台）
    public void SwitchToOculusForAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        RemoveLoaderForAndroid<OpenXRLoader>();
        AddLoaderForAndroid<OculusLoader>();
        Debug.Log("Switched to Oculus for Android platform.");
        CallPlatformSwitcherMethod("OnSwitchToOculus");
        SaveCurrentScene(); // 保存場景
    }

    // 建立 Windows (PC) 版本
    public void BuildPC(string fileName)
    {
        SwitchToPC();
        string path = $"Build/{dateFolder}/{fileName}_PC_v" + System.DateTime.Now.ToString("yyyyMMdd");
        Directory.CreateDirectory(path); // 確保資料夾存在
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{path}/{fileName}_PC_v{System.DateTime.Now.ToString("yyyyMMdd")}.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log($"PC build complete: {path}");
    }

    // 建立 Android (OpenXR) 版本
    public void BuildOpenXRForAndroid(string fileName)
    {
        SwitchToOpenXRForAndroid();
        string path = $"Build/{dateFolder}";
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{path}/{fileName}_OpenXR_v{System.DateTime.Now.ToString("yyyyMMdd")}.apk", BuildTarget.Android, BuildOptions.None);
        Debug.Log($"OpenXR build complete: {path}");
    }

    // 建立 Android (Oculus) 版本
    public void BuildOculusForAndroid(string fileName)
    {
        SwitchToOculusForAndroid();
        string path = $"Build/{dateFolder}";
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{path}/{fileName}_Oculus_v{System.DateTime.Now.ToString("yyyyMMdd")}.apk", BuildTarget.Android, BuildOptions.None);
        Debug.Log($"Oculus build complete: {path}");
    }

    // 同時為三個平台進行構建
    public void BuildAllPlatforms()
    {
        if (string.IsNullOrEmpty(buildFileName))
        {
            Debug.LogError("Build file name cannot be empty!");
            return;
        }

        BuildPC(buildFileName);
        BuildOculusForAndroid(buildFileName);
        BuildOpenXRForAndroid(buildFileName);
    }

    // 呼叫 PlatformSwitcher 中的對應方法
    private void CallPlatformSwitcherMethod(string methodName)
    {
        GameObject platformSwitcherObject = GameObject.Find("PlatformSwitcher");
        if (platformSwitcherObject != null)
        {
            var platformSwitcher = platformSwitcherObject.GetComponent<PlatformSwitcher>();
            if (platformSwitcher != null)
            {
                var methodInfo = platformSwitcher.GetType().GetMethod(methodName);
                if (methodInfo != null)
                {
                    methodInfo.Invoke(platformSwitcher, null);
                    Debug.Log($"Called method {methodName} on PlatformSwitcher.");
                }
                else
                {
                    Debug.LogWarning($"Method {methodName} not found on PlatformSwitcher.");
                }
            }
            else
            {
                Debug.LogWarning("PlatformSwitcher component not found on GameObject.");
            }
        }
        else
        {
            Debug.LogWarning("GameObject named 'PlatformSwitcher' not found in the scene.");
        }
    }

}