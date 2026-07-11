using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class DownloadManager : MonoBehaviour
{
    private const int ConnectionTimeoutSeconds = 60;

    public void Download(string url, string destPath,
        UnityAction<float>  onProgress,
        UnityAction<string> onComplete,
        UnityAction<string> onError)
    {
        StartCoroutine(DownloadCoroutine(url, destPath, onProgress, onComplete, onError));
    }

    private IEnumerator DownloadCoroutine(string url, string destPath,
        UnityAction<float>  onProgress,
        UnityAction<string> onComplete,
        UnityAction<string> onError)
    {
        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
        {
            req.downloadHandler = new DownloadHandlerFile(destPath);
            req.timeout = ConnectionTimeoutSeconds;

            var op = req.SendWebRequest();
            while (!op.isDone)
            {
                onProgress?.Invoke(req.downloadProgress);
                yield return null;
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"HTTP {req.responseCode}: {req.error}");
                yield break;
            }

            onProgress?.Invoke(1f);
            onComplete?.Invoke(destPath);
        }
    }
}
