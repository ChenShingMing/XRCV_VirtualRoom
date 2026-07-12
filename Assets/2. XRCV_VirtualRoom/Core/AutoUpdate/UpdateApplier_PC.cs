using System.Diagnostics;
using System.IO;
using System.Text;

/// <summary>
/// PC 更新：寫 bat 到 %TEMP%，啟動後 Quit。
/// Bat 等 Unity 關閉 → PowerShell 解壓 ZIP 覆蓋安裝目錄 → 重啟 EXE。
/// 僅在 Windows Standalone / Editor Win 有效。
/// </summary>
public static class UpdateApplier_PC
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    public static void Install(string zipPath)
    {
        // 安裝目錄 = EXE 所在資料夾（Application.dataPath 的父層）
        string installDir = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
        string exePath    = Path.Combine(installDir, UnityEngine.Application.productName + ".exe");
        string batPath    = Path.Combine(Path.GetTempPath(), "xrcv_update.bat");

        // 用單引號包路徑，讓 PowerShell 正確處理含空格路徑
        string batContent =
            "@echo off\r\n" +
            "timeout /t 3 /nobreak > NUL\r\n" +
            "powershell -Command \"Expand-Archive -Path '" + zipPath    + "' -DestinationPath '" + installDir + "' -Force\"\r\n" +
            "del \"" + zipPath + "\"\r\n" +
            "start \"\" \""  + exePath + "\"\r\n" +
            "del \"%~f0\"\r\n";

        File.WriteAllText(batPath, batContent, Encoding.Default);
        UnityEngine.Debug.Log($"[UpdateApplier_PC] Bat written to: {batPath}");

        Process.Start(new ProcessStartInfo
        {
            FileName        = "cmd.exe",
            Arguments       = "/c \"" + batPath + "\"",
            UseShellExecute = false,
            CreateNoWindow  = true,
        });

        UnityEngine.Debug.Log("[UpdateApplier_PC] Updater launched — quitting app");
        UnityEngine.Application.Quit();
    }
#else
    public static void Install(string zipPath)
    {
        UnityEngine.Debug.LogWarning("[UpdateApplier_PC] Install called on non-Windows platform — ignored");
    }
#endif
}
