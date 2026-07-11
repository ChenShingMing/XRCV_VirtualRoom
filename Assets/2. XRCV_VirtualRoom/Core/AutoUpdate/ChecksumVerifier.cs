using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Collections;
using UnityEngine;

public static class ChecksumVerifier
{
    // Coroutine-friendly: waits on main thread while background thread computes hash.
    public static IEnumerator VerifyCoroutine(
        string filePath, string expectedChecksum, Action<bool> onResult)
    {
        // 若 manifest 尚未填入真實 checksum，跳過驗證
        if (string.IsNullOrEmpty(expectedChecksum) ||
            expectedChecksum.StartsWith("sha256:<"))
        {
            Debug.Log("[ChecksumVerifier] No checksum set — skipping verification");
            onResult?.Invoke(true);
            yield break;
        }

        bool done  = false;
        bool match = false;

        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                string actual   = ComputeSHA256(filePath);
                string expected = expectedChecksum;
                if (expected.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
                    expected = expected.Substring(7);

                match = string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
                Debug.Log($"[ChecksumVerifier] actual={actual}");
                Debug.Log($"[ChecksumVerifier] expected={expected}  match={match}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ChecksumVerifier] Error: {e.Message}");
                match = false;
            }
            finally
            {
                done = true;
            }
        });

        while (!done) yield return null;
        onResult?.Invoke(match);
    }

    private static string ComputeSHA256(string filePath)
    {
        using (var sha256 = SHA256.Create())
        using (var stream = File.OpenRead(filePath))
        {
            byte[] hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
