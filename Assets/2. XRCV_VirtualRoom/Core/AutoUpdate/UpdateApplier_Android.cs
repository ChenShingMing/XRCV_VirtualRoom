using UnityEngine;

/// <summary>
/// 透過 Android FileProvider + ACTION_VIEW Intent 呼叫系統 PackageInstaller。
/// 僅在 Android 平台有效（#if UNITY_ANDROID）。
/// </summary>
public static class UpdateApplier_Android
{
#if UNITY_ANDROID
    public static void Install(string apkPath)
    {
        Debug.Log($"[UpdateApplier_Android] Installing: {apkPath}");
        try
        {
            using var player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var context  = activity.Call<AndroidJavaObject>("getApplicationContext");

            string packageName = context.Call<string>("getPackageName");
            string authority   = packageName + ".provider";

            using var file             = new AndroidJavaObject("java.io.File", apkPath);
            using var fileProviderClass = new AndroidJavaClass("androidx.core.content.FileProvider");
            using var uri              = fileProviderClass.CallStatic<AndroidJavaObject>(
                "getUriForFile", context, authority, file);

            using var intent = new AndroidJavaObject("android.content.Intent",
                "android.intent.action.VIEW");
            intent.Call<AndroidJavaObject>("setDataAndType", uri,
                "application/vnd.android.package-archive");
            // FLAG_GRANT_READ_URI_PERMISSION | FLAG_ACTIVITY_NEW_TASK
            intent.Call<AndroidJavaObject>("addFlags", 0x00000001 | 0x10000000);

            activity.Call("startActivity", intent);
            Debug.Log("[UpdateApplier_Android] PackageInstaller launched — quitting app");

            // 讓系統 installer dialog 不被 Unity app 遮住
            Application.Quit();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UpdateApplier_Android] Install failed: {e.Message}");
        }
    }
#else
    public static void Install(string apkPath)
    {
        Debug.LogWarning("[UpdateApplier_Android] Install called on non-Android platform — ignored");
    }
#endif
}
