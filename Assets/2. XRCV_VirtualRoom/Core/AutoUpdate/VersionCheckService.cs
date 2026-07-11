using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class VersionCheckService : MonoBehaviour
{
    private const string ProjectId = "creativexrworld";
    private const string ApiKey    = "AIzaSyCioQs9R6V1MT28lttoHF2HXDD80JoiNL4";
    private const int    TimeoutSeconds = 10;

    public void Check(UnityAction<VersionManifest> onSuccess, UnityAction<string> onFail)
    {
        StartCoroutine(CheckCoroutine(onSuccess, onFail));
    }

    private IEnumerator CheckCoroutine(UnityAction<VersionManifest> onSuccess, UnityAction<string> onFail)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents/AppConfig/version?key={ApiKey}";

        using (var req = UnityWebRequest.Get(url))
        {
            req.timeout = TimeoutSeconds;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onFail?.Invoke($"HTTP {req.responseCode}: {req.error}");
                yield break;
            }

            var manifest = ParseManifest(req.downloadHandler.text);
            if (manifest == null)
            {
                onFail?.Invoke("Failed to parse version manifest");
                yield break;
            }

            onSuccess?.Invoke(manifest);
        }
    }

    private VersionManifest ParseManifest(string json)
    {
        try
        {
            var m = new VersionManifest
            {
                latestVersion      = GetStringField(json, "latestVersion"),
                minRequiredVersion = GetStringField(json, "minRequiredVersion"),
                forceUpdate        = GetBoolField(json, "forceUpdate"),
                releaseNote        = GetStringField(json, "releaseNote"),
                android = new PlatformAsset
                {
                    downloadUrl = GetNestedStringField(json, "android", "downloadUrl"),
                    checksum    = GetNestedStringField(json, "android", "checksum"),
                    sizeBytes   = GetNestedLongField(json, "android", "sizeBytes"),
                },
                pc = new PlatformAsset
                {
                    downloadUrl = GetNestedStringField(json, "pc", "downloadUrl"),
                    checksum    = GetNestedStringField(json, "pc", "checksum"),
                    sizeBytes   = GetNestedLongField(json, "pc", "sizeBytes"),
                },
            };
            return m;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[VersionCheckService] Parse error: {e.Message}");
            return null;
        }
    }

    // ── Firestore REST response parsers ────────────────────────────────────

    private string GetStringField(string json, string field)
    {
        int fIdx = json.IndexOf($"\"{field}\"");
        if (fIdx < 0) return "";
        int svIdx = json.IndexOf("\"stringValue\"", fIdx);
        if (svIdx < 0) return "";
        int colon = json.IndexOf(':', svIdx);
        int q1 = json.IndexOf('"', colon + 1);
        int q2 = json.IndexOf('"', q1 + 1);
        return (q1 >= 0 && q2 > q1) ? json.Substring(q1 + 1, q2 - q1 - 1) : "";
    }

    private bool GetBoolField(string json, string field)
    {
        int fIdx = json.IndexOf($"\"{field}\"");
        if (fIdx < 0) return false;
        int bvIdx = json.IndexOf("\"booleanValue\"", fIdx);
        if (bvIdx < 0) return false;
        int colon = json.IndexOf(':', bvIdx);
        int valStart = colon + 1;
        while (valStart < json.Length && json[valStart] == ' ') valStart++;
        return json.Length >= valStart + 4 && json.Substring(valStart, 4) == "true";
    }

    // Searches within a 600-char window starting from the parent field to handle
    // duplicate child field names (e.g. "downloadUrl" in both android and pc).
    private string GetNestedStringField(string json, string parent, string child)
    {
        int pIdx = json.IndexOf($"\"{parent}\"");
        if (pIdx < 0) return "";
        string section = json.Substring(pIdx, System.Math.Min(600, json.Length - pIdx));
        int cIdx = section.IndexOf($"\"{child}\"");
        if (cIdx < 0) return "";
        int svIdx = section.IndexOf("\"stringValue\"", cIdx);
        if (svIdx < 0) return "";
        int colon = section.IndexOf(':', svIdx);
        int q1 = section.IndexOf('"', colon + 1);
        int q2 = section.IndexOf('"', q1 + 1);
        return (q1 >= 0 && q2 > q1) ? section.Substring(q1 + 1, q2 - q1 - 1) : "";
    }

    private long GetNestedLongField(string json, string parent, string child)
    {
        int pIdx = json.IndexOf($"\"{parent}\"");
        if (pIdx < 0) return 0;
        string section = json.Substring(pIdx, System.Math.Min(600, json.Length - pIdx));
        int cIdx = section.IndexOf($"\"{child}\"");
        if (cIdx < 0) return 0;
        int ivIdx = section.IndexOf("\"integerValue\"", cIdx);
        if (ivIdx < 0) return 0;
        int colon = section.IndexOf(':', ivIdx);
        int q1 = section.IndexOf('"', colon + 1);
        int q2 = section.IndexOf('"', q1 + 1);
        if (q1 < 0 || q2 <= q1) return 0;
        long.TryParse(section.Substring(q1 + 1, q2 - q1 - 1), out long result);
        return result;
    }
}
