using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(VersionCheckService))]
[RequireComponent(typeof(DownloadManager))]
public class UpdateManager : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent           OnReadyToLoad;
    public UnityEvent<string>   OnUpdateAvailable;

    [Header("UI")]
    public UpdateUI updateUI;

    private VersionCheckService _versionCheckService;
    private DownloadManager     _downloadManager;

    private void Awake()
    {
        _versionCheckService = GetComponent<VersionCheckService>();
        _downloadManager     = GetComponent<DownloadManager>();
    }

    private void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[UpdateManager] Offline — skipping version check");
            Proceed();
            return;
        }

        Debug.Log($"[UpdateManager] Checking version (local: {Application.version})…");
        _versionCheckService.Check(OnManifestReceived, OnCheckFailed);
    }

    private void OnManifestReceived(VersionManifest manifest)
    {
        string local = Application.version;
        Debug.Log($"[UpdateManager] Cloud version: {manifest.latestVersion}");

        if (!manifest.IsNewerThan(local))
        {
            Debug.Log("[UpdateManager] Already up to date");
            Proceed();
            return;
        }

        Debug.Log($"[UpdateManager] Update available → {manifest.latestVersion}  ({manifest.releaseNote})");

        bool forced = manifest.IsForceUpdateRequired(local);
        OnUpdateAvailable?.Invoke(manifest.releaseNote);

        if (updateUI != null)
            updateUI.ShowUpdatePrompt(manifest.releaseNote, forced, onSkip: Proceed);

        var asset = manifest.GetCurrentPlatformAsset();
        if (asset == null || string.IsNullOrEmpty(asset.downloadUrl))
        {
            Debug.LogWarning("[UpdateManager] No download URL — skipping download");
            Proceed();
            return;
        }

        string destPath = System.IO.Path.Combine(
            Application.temporaryCachePath,
            $"update_{manifest.latestVersion}{GetExtension()}");

        Debug.Log($"[UpdateManager] Downloading → {destPath}");

        _downloadManager.Download(
            asset.downloadUrl,
            destPath,
            onProgress: progress =>
            {
                if (updateUI != null) updateUI.SetProgress(progress);
            },
            onComplete: path =>
            {
                Debug.Log($"[UpdateManager] Download complete: {path}");
                if (updateUI != null) updateUI.ShowComplete();
                Proceed(); // Phase 4/5: replace with installer
            },
            onError: err =>
            {
                Debug.LogWarning($"[UpdateManager] Download failed: {err}");
                if (updateUI != null)
                    updateUI.ShowError(err);  // ShowError re-enables Skip button for user to proceed
                else
                    Proceed(); // no UI — auto-proceed
            });
    }

    private void OnCheckFailed(string error)
    {
        Debug.LogWarning($"[UpdateManager] Version check failed: {error} — proceeding without update");
        Proceed();
    }

    private void Proceed()
    {
        if (updateUI != null) updateUI.Hide();
        OnReadyToLoad?.Invoke();
    }

    private static string GetExtension()
    {
#if UNITY_ANDROID
        return ".apk";
#else
        return ".zip";
#endif
    }
}
