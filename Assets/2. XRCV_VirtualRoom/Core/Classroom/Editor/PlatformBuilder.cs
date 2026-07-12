using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.OpenXR.Features;
using System.IO;
using UnityEditor.SceneManagement;

public class PlatformBuilder : EditorWindow
{
    // OpenXR Feature IDs
    private const string FEATURE_OCULUS_TOUCH = "com.unity.openxr.feature.input.oculustouch";
    private const string FEATURE_VIVE_FOCUS3  = "vive.openxr.feature.focus3controller";

    // LoadingScene 必須是 build 中的第一個場景
    private const string LOADING_SCENE_PATH = "Assets/2. XRCV_VirtualRoom/Core/LoadingScene/LoadingScene.unity";

    // EditorPrefs keys
    private const string PREF_FILE_NAME   = "PlatformBuilder_FileName";
    private const string PREF_PC_PATH     = "PlatformBuilder_PCPath";
    private const string PREF_QUEST_PATH  = "PlatformBuilder_QuestPath";

    private string _buildFileName;
    private string _pcBuildPath;
    private string _questBuildPath;

    [MenuItem("PlatformBuilder/Builder")]
    public static void ShowWindow() => GetWindow<PlatformBuilder>("Build Window");

    private void OnEnable()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        _buildFileName  = EditorPrefs.GetString(PREF_FILE_NAME,  "creativeXRworld");
        _pcBuildPath    = EditorPrefs.GetString(PREF_PC_PATH,    Path.Combine(projectRoot, "Build", "PC"));
        _questBuildPath = EditorPrefs.GetString(PREF_QUEST_PATH, Path.Combine(projectRoot, "Build", "Quest"));
    }

    void OnGUI()
    {
        GUILayout.Label("Build Window", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // ── 檔名 ──
        EditorGUI.BeginChangeCheck();
        _buildFileName = EditorGUILayout.TextField("執行檔名稱", _buildFileName);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetString(PREF_FILE_NAME, _buildFileName);

        EditorGUILayout.Space(8);

        // ── PC 路徑 ──
        EditorGUILayout.LabelField("PC 輸出目錄", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        _pcBuildPath = EditorGUILayout.TextField(_pcBuildPath);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetString(PREF_PC_PATH, _pcBuildPath);
        if (GUILayout.Button("…", GUILayout.Width(28)))
        {
            string p = EditorUtility.OpenFolderPanel("選擇 PC 輸出目錄", _pcBuildPath, "");
            if (!string.IsNullOrEmpty(p)) { _pcBuildPath = p; EditorPrefs.SetString(PREF_PC_PATH, p); }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // ── Quest 路徑 ──
        EditorGUILayout.LabelField("Quest APK 輸出目錄", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        _questBuildPath = EditorGUILayout.TextField(_questBuildPath);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetString(PREF_QUEST_PATH, _questBuildPath);
        if (GUILayout.Button("…", GUILayout.Width(28)))
        {
            string p = EditorUtility.OpenFolderPanel("選擇 Quest 輸出目錄", _questBuildPath, "");
            if (!string.IsNullOrEmpty(p)) { _questBuildPath = p; EditorPrefs.SetString(PREF_QUEST_PATH, p); }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(12);
        DrawDivider();
        EditorGUILayout.Space(8);

        // ── Build 按鈕 ──
        GUI.backgroundColor = new Color(0.35f, 0.75f, 0.35f);
        if (GUILayout.Button("▶  Build Both  ( PC + Quest )", GUILayout.Height(42)))
            BuildBoth();

        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Build PC", GUILayout.Height(30)))
            BuildPC();
        if (GUILayout.Button("Build Quest", GUILayout.Height(30)))
            BuildQuest();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(12);
        DrawDivider();
        EditorGUILayout.Space(8);

        // ── 平台切換（不 Build） ──
        GUILayout.Label("切換平台（不 Build）", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("切換到 PC"))    SwitchToPC();
        if (GUILayout.Button("切換到 Quest")) SwitchToQuestForAndroid();
        if (GUILayout.Button("切換到 VIVE"))  SwitchToOpenXRForAndroid();
        EditorGUILayout.EndHorizontal();
    }

    // ──────────────────────────────────────────
    // Build
    // ──────────────────────────────────────────

    private void BuildBoth()
    {
        bool pcOk    = BuildPC(showDialog: false);
        bool questOk = BuildQuest(showDialog: false);
        string msg = "Build Both 完成：\n"
            + "PC:    " + (pcOk    ? "✓ 成功" : "✗ 失敗") + "\n"
            + "Quest: " + (questOk ? "✓ 成功" : "✗ 失敗");
        Debug.Log("[PlatformBuilder] " + msg);
        EditorUtility.DisplayDialog("Build Window", msg, "OK");
    }

    private void BuildPC()    => BuildPC(showDialog: true);
    private void BuildQuest() => BuildQuest(showDialog: true);

    private bool BuildPC(bool showDialog)
    {
        if (!ValidateName() || !ValidatePath(_pcBuildPath, "PC")) return false;
        EnsureLoadingScene();
        SwitchToPC();

        Directory.CreateDirectory(_pcBuildPath);
        string exePath = Path.Combine(_pcBuildPath, _buildFileName + ".exe");

        var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, exePath,
            BuildTarget.StandaloneWindows64, BuildOptions.None);

        bool ok = report.summary.result == BuildResult.Succeeded;
        Debug.Log("[PlatformBuilder] PC Build " + (ok ? "成功" : "失敗") + " → " + exePath);
        if (showDialog)
            EditorUtility.DisplayDialog("Build Window",
                "PC Build " + (ok ? "成功！\n" + exePath : "失敗，請查看 Console"), "OK");
        return ok;
    }

    private bool BuildQuest(bool showDialog)
    {
        if (!ValidateName() || !ValidatePath(_questBuildPath, "Quest")) return false;
        EnsureLoadingScene();
        SwitchToQuestForAndroid();

        Directory.CreateDirectory(_questBuildPath);
        string date    = System.DateTime.Now.ToString("yyyyMMdd");
        string apkPath = Path.Combine(_questBuildPath, _buildFileName + "_v" + PlayerSettings.bundleVersion + "_" + date + ".apk");

        var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, apkPath,
            BuildTarget.Android, BuildOptions.None);

        bool ok = report.summary.result == BuildResult.Succeeded;
        Debug.Log("[PlatformBuilder] Quest Build " + (ok ? "成功" : "失敗") + " → " + apkPath);
        if (showDialog)
            EditorUtility.DisplayDialog("Build Window",
                "Quest Build " + (ok ? "成功！\n" + apkPath : "失敗，請查看 Console"), "OK");
        return ok;
    }

    // ──────────────────────────────────────────
    // Platform Switch
    // ──────────────────────────────────────────

    public void SwitchToPC()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        CallPlatformSwitcher("OnSwitchToPC");
        SaveScene();
        Debug.Log("[PlatformBuilder] Switched to PC.");
    }

    public void SwitchToQuestForAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        SetOpenXRFeature(FEATURE_OCULUS_TOUCH, true);
        SetOpenXRFeature(FEATURE_VIVE_FOCUS3,  false);
        AssetDatabase.SaveAssets();
        CallPlatformSwitcher("OnSwitchToOculus");
        SaveScene();
        Debug.Log("[PlatformBuilder] Switched to Quest.");
    }

    public void SwitchToOpenXRForAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        SetOpenXRFeature(FEATURE_VIVE_FOCUS3,  true);
        SetOpenXRFeature(FEATURE_OCULUS_TOUCH, false);
        AssetDatabase.SaveAssets();
        CallPlatformSwitcher("OnSwitchToOpenXR");
        SaveScene();
        Debug.Log("[PlatformBuilder] Switched to VIVE.");
    }

    // ──────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────

    private static void EnsureLoadingScene()
    {
        foreach (var s in EditorBuildSettings.scenes)
            if (s.path == LOADING_SCENE_PATH) return;

        var list = EditorBuildSettings.scenes;
        var newList = new EditorBuildSettingsScene[list.Length + 1];
        newList[0] = new EditorBuildSettingsScene(LOADING_SCENE_PATH, true);
        for (int i = 0; i < list.Length; i++) newList[i + 1] = list[i];
        EditorBuildSettings.scenes = newList;
        Debug.Log("[PlatformBuilder] LoadingScene 已加入 Build Settings [0]。");
    }

    private static void SetOpenXRFeature(string featureId, bool enabled)
    {
        var feature = FeatureHelpers.GetFeatureWithIdForBuildTarget(BuildTargetGroup.Android, featureId);
        if (feature == null) { Debug.LogWarning("[PlatformBuilder] 找不到 OpenXR Feature: " + featureId); return; }
        feature.enabled = enabled;
        EditorUtility.SetDirty(feature);
    }

    private static void SaveScene()
    {
        if (!EditorApplication.isPlaying && EditorSceneManager.GetActiveScene().isDirty)
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static void CallPlatformSwitcher(string methodName)
    {
        var go = GameObject.Find("PlatformSwitcher");
        if (go == null) { Debug.LogWarning("[PlatformBuilder] PlatformSwitcher 找不到。"); return; }
        var ps = go.GetComponent<PlatformSwitcher>();
        if (ps == null) { Debug.LogWarning("[PlatformBuilder] PlatformSwitcher 元件找不到。"); return; }
        var m = ps.GetType().GetMethod(methodName);
        if (m == null) { Debug.LogWarning("[PlatformBuilder] 方法不存在: " + methodName); return; }
        m.Invoke(ps, null);
    }

    private bool ValidateName()
    {
        if (!string.IsNullOrEmpty(_buildFileName)) return true;
        EditorUtility.DisplayDialog("Build Window", "請填寫執行檔名稱", "OK");
        return false;
    }

    private bool ValidatePath(string path, string label)
    {
        if (!string.IsNullOrEmpty(path)) return true;
        EditorUtility.DisplayDialog("Build Window", "請設定 " + label + " 輸出路徑", "OK");
        return false;
    }

    private static void DrawDivider()
    {
        Rect r = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(r, new Color(0.5f, 0.5f, 0.5f, 0.3f));
    }
}
