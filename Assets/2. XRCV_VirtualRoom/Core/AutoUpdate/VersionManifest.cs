using System;

[Serializable]
public class VersionManifest
{
    public string latestVersion;
    public string minRequiredVersion;
    public bool   forceUpdate;
    public string releaseNote;
    public PlatformAsset android;
    public PlatformAsset pc;

    public PlatformAsset GetCurrentPlatformAsset()
    {
#if UNITY_ANDROID
        return android;
#else
        return pc;
#endif
    }

    public bool IsNewerThan(string localVersion)
    {
        return CompareVersions(localVersion, latestVersion) < 0;
    }

    public bool IsForceUpdateRequired(string localVersion)
    {
        return forceUpdate || CompareVersions(localVersion, minRequiredVersion) < 0;
    }

    // 逐段以數字比較（"1.9" < "1.10"），取代原本的字串 Ordinal 比較
    // （Ordinal 比較對長度不一的版號如 "1.9" vs "1.10" 會比錯）。
    private static int CompareVersions(string a, string b)
    {
        int[] pa = ParseParts(a);
        int[] pb = ParseParts(b);
        int len = Math.Max(pa.Length, pb.Length);
        for (int i = 0; i < len; i++)
        {
            int va = i < pa.Length ? pa[i] : 0;
            int vb = i < pb.Length ? pb[i] : 0;
            if (va != vb) return va.CompareTo(vb);
        }
        return 0;
    }

    private static int[] ParseParts(string version)
    {
        if (string.IsNullOrEmpty(version)) return new int[0];
        string[] segments = version.TrimStart('v', 'V').Split('.');
        int[] parts = new int[segments.Length];
        for (int i = 0; i < segments.Length; i++)
            int.TryParse(segments[i], out parts[i]);
        return parts;
    }
}

[Serializable]
public class PlatformAsset
{
    public string downloadUrl;
    public string checksum;
    public long   sizeBytes;
}
