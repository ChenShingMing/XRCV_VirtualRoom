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
        return string.Compare(localVersion, latestVersion, StringComparison.Ordinal) < 0;
    }

    public bool IsForceUpdateRequired(string localVersion)
    {
        return forceUpdate || string.Compare(localVersion, minRequiredVersion, StringComparison.Ordinal) < 0;
    }
}

[Serializable]
public class PlatformAsset
{
    public string downloadUrl;
    public string checksum;
    public long   sizeBytes;
}
