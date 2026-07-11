using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Loading Scene 的更新流程入口。
/// Start() 時檢查網路 → 查詢版本 → 比對後觸發 OnReadyToLoad。
/// Phase 2 在 OnReadyToLoad 之前插入下載 UI。
/// </summary>
[RequireComponent(typeof(VersionCheckService))]
public class UpdateManager : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnReadyToLoad;
    public UnityEvent<string> OnUpdateAvailable;  // arg: releaseNote

    private VersionCheckService _versionCheckService;

    private void Awake()
    {
        _versionCheckService = GetComponent<VersionCheckService>();
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
        Debug.Log($"[UpdateManager] Force: {manifest.IsForceUpdateRequired(local)}  URL: {manifest.GetCurrentPlatformAsset()?.downloadUrl}");

        // Phase 2: 在此插入下載 UI，目前直接放行
        OnUpdateAvailable?.Invoke(manifest.releaseNote);
        Proceed();
    }

    private void OnCheckFailed(string error)
    {
        Debug.LogWarning($"[UpdateManager] Version check failed: {error} — proceeding without update");
        Proceed();
    }

    private void Proceed()
    {
        OnReadyToLoad?.Invoke();
    }
}
