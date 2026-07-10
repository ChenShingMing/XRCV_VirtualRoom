using UnityEngine;
using UnityEditor;
using UnityEngine.XR.OpenXR;
using UnityEditor.XR.OpenXR.Features;
using System.IO;
using UnityEditor.SceneManagement;

public class PlatformBuilder : EditorWindow
{
    // OpenXR Feature IDs
    private const string FEATURE_OCULUS_TOUCH   = "com.unity.openxr.feature.input.oculustouch";
    private const string FEATURE_VIVE_FOCUS3     = "vive.openxr.feature.focus3controller";

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

        if (GUILayout.Button("Switch to Quest (OpenXR)"))
        {
            SwitchToQuestForAndroid();
        }

        if (GUILayout.Button("Switch to VIVE (OpenXR)"))
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

        if (GUILayout.Button("Build Quest"))
        {
            if (string.IsNullOrEmpty(buildFileName))
            {
                Debug.LogError("Build file name cannot be empty!");
                return;
            }

            BuildQuestForAndroid(buildFileName);
        }

        if (GUILayout.Button("Build VIVE"))
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

    // ──────────────────────────────────────────
    // Platform Switch
    // ──────────────────────────────────────────

    // 切換到 PC（Windows Standalone）
    public void SwitchToPC()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        Debug.Log("Switched to PC platform (Windows).");
        CallPlatformSwitcherMethod("OnSwitchToPC");
        CallClassroomManagerMethod();
        SaveCurrentScene();
    }

    // 切換到 Quest（Android，OpenXR）
    // 兩個平台都走 OpenXR Loader，只切換 Feature
    public void SwitchToQuestForAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        SetAndroidOpenXRFeature(FEATURE_OCULUS_TOUCH, true);   // Quest Touch 控制器
        SetAndroidOpenXRFeature(FEATURE_VIVE_FOCUS3,  false);  // 關閉 VIVE Focus 3

        AssetDatabase.SaveAssets();
        Debug.Log("Switched to Quest (Android OpenXR) platform.");
        CallPlatformSwitcherMethod("OnSwitchToOculus");
        CallClassroomManagerMethod();
        SaveCurrentScene();
    }

    // 切換到 VIVE（Android，OpenXR）
    public void SwitchToOpenXRForAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        SetAndroidOpenXRFeature(FEATURE_VIVE_FOCUS3,  true);   // VIVE Focus 3 控制器
        SetAndroidOpenXRFeature(FEATURE_OCULUS_TOUCH, false);  // 關閉 Quest Touch

        AssetDatabase.SaveAssets();
        Debug.Log("Switched to VIVE (Android OpenXR) platform.");
        CallPlatformSwitcherMethod("OnSwitchToOpenXR");
        CallClassroomManagerMethod();
        SaveCurrentScene();
    }

    // ──────────────────────────────────────────
    // Build（暫時停用，Switch 後手動 Build）
    // ──────────────────────────────────────────

    public void BuildPC(string fileName)
    {
        SwitchToPC();
        string path = $"Build/{dateFolder}/{fileName}_PC_v{System.DateTime.Now:yyyyMMdd}";
        Directory.CreateDirectory(path);
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{path}/{fileName}_PC_v{System.DateTime.Now:yyyyMMdd}.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log($"PC build complete: {path}");
    }

    public void BuildQuestForAndroid(string fileName)
    {
        SwitchToQuestForAndroid();
        string path = $"Build/{dateFolder}";
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{path}/{fileName}_Quest_v{System.DateTime.Now:yyyyMMdd}.apk", BuildTarget.Android, BuildOptions.None);
        Debug.Log($"Quest build complete: {path}");
    }

    public void BuildOpenXRForAndroid(string fileName)
    {
        SwitchToOpenXRForAndroid();
        string path = $"Build/{dateFolder}";
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{path}/{fileName}_VIVE_v{System.DateTime.Now:yyyyMMdd}.apk", BuildTarget.Android, BuildOptions.None);
        Debug.Log($"VIVE build complete: {path}");
    }

    public void BuildAllPlatforms()
    {
        if (string.IsNullOrEmpty(buildFileName))
        {
            Debug.LogError("Build file name cannot be empty!");
            return;
        }

        BuildPC(buildFileName);
        BuildQuestForAndroid(buildFileName);
        BuildOpenXRForAndroid(buildFileName);
    }

    // ──────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────

    // 設定 Android 平台指定 OpenXR Feature 的啟用狀態
    private static void SetAndroidOpenXRFeature(string featureId, bool enabled)
    {
        var feature = FeatureHelpers.GetFeatureWithIdForBuildTarget(BuildTargetGroup.Android, featureId);
        if (feature == null)
        {
            Debug.LogWarning($"找不到 OpenXR Feature: {featureId}");
            return;
        }

        feature.enabled = enabled;
        EditorUtility.SetDirty(feature);
        Debug.Log($"OpenXR Feature '{featureId}' → {enabled}");
    }

    // 儲存當前場景
    private void SaveCurrentScene()
    {
        if (!EditorApplication.isPlaying && EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("Current scene saved.");
        }
    }

    // 呼叫場景中 PlatformSwitcher 的對應方法
    private void CallPlatformSwitcherMethod(string methodName)
    {
        var go = GameObject.Find("PlatformSwitcher");
        if (go == null) { Debug.LogWarning("PlatformSwitcher GameObject not found in scene."); return; }

        var ps = go.GetComponent<PlatformSwitcher>();
        if (ps == null) { Debug.LogWarning("PlatformSwitcher component not found."); return; }

        var method = ps.GetType().GetMethod(methodName);
        if (method == null) { Debug.LogWarning($"Method '{methodName}' not found on PlatformSwitcher."); return; }

        method.Invoke(ps, null);
        Debug.Log($"Called PlatformSwitcher.{methodName}()");
    }

    private void CallClassroomManagerMethod()
    {
        var manager = GameObject.Find("ClassroomManager")?.GetComponent<ClassroomManager>();
        if (manager == null) { Debug.LogWarning("ClassroomManager not found."); return; }
        manager.UpdateVersion();
    }
}
