using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class FirebaseLicenseInfoManager : MonoBehaviour
{
    public LicenseInformation licenseInformation;
    public string cllection = "Element";
    public string documentID;

    public SampleData sampleData;

    FirebaseFirestore firestore;

    private void EnsureFirestore()
    {
        if (firestore != null) return;
        // Each process gets a uniquely named FirebaseApp so Firestore uses a separate
        // LevelDB directory per instance, allowing multiple builds to run simultaneously.
        int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
        string appName = "xrcv_" + pid;
        FirebaseApp app;
        try { app = FirebaseApp.Create(FirebaseApp.DefaultInstance.Options, appName); }
        catch { app = FirebaseApp.GetInstance(appName); }
        firestore = FirebaseFirestore.GetInstance(app);
    }

    [Button]
    public void AddData()
    {
        EnsureFirestore();
        // �N sampleData �ഫ���r��
        Dictionary<string, object> data = ConvertSampleDataToDictionary(sampleData);

        // ���o Firestore �� DocumentReference
        DocumentReference docRef = firestore.Collection(cllection).Document(documentID);

        // �N��Ƽg�J Firestore
        docRef.SetAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SetAsync �Q�����C");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SetAsync �J����~�G" + task.Exception);
                return;
            }

            Debug.Log("Data saved to Firestore.");
        });
    }

    private bool IsGooglePlayServicesAvailable()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var ctx = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                                 .GetStatic<AndroidJavaObject>("currentActivity"))
            using (var gps = new AndroidJavaClass("com.google.android.gms.common.GoogleApiAvailability"))
            {
                var instance = gps.CallStatic<AndroidJavaObject>("getInstance");
                int result = instance.Call<int>("isGooglePlayServicesAvailable", ctx);
                Debug.Log($"[Firebase] GPS availability result: {result}");
                return result == 0; // 0 = SUCCESS
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Firebase] GPS check failed (likely Quest): {e.Message}");
            return false;
        }
#else
        return true;
#endif
    }

    [Button]
    public void ReadData(UnityAction OnSuccessEvent = null, UnityAction OnFailEvent = null)
    {
        if (!IsGooglePlayServicesAvailable())
        {
            Debug.LogWarning("[Firebase] GPS not available, using REST API fallback.");
            StartCoroutine(ReadDataViaRest(OnSuccessEvent, OnFailEvent));
            return;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(depTask =>
        {
            if (depTask.Result != DependencyStatus.Available)
            {
                Debug.LogError("Firebase dependencies not available: " + depTask.Result);
                OnFailEvent?.Invoke();
                return;
            }

            EnsureFirestore();
            firestore.Collection(cllection).Document(documentID).GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                if (task.IsCompleted)
                {
                    DocumentSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        Dictionary<string, object> data = snapshot.ToDictionary();
                        sampleData.logo = GetValueOrDefault<string>(data, "logo");
                        sampleData.schoolName = GetValueOrDefault<string>(data, "schoolName");
                        sampleData.photonAppID = GetValueOrDefault<string>(data, "photonAppID");
                        sampleData.seatInfo_Stu = (int)GetValueOrDefault<long>(data, "seatInfo_Stu");
                        sampleData.seatInfo_Teacher = (int)GetValueOrDefault<long>(data, "seatInfo_Teacher");

                        SetLicenseInformation(sampleData);

                        if(OnSuccessEvent != null)
                        {
                            OnSuccessEvent.Invoke();
                        }

                    }
                    else
                    {
                        Debug.Log("Document ID not found: " + documentID);

                        if (OnFailEvent != null)
                        {
                            OnFailEvent.Invoke();
                        }
                    }
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("Failed to read data: " + task.Exception);

                    if (OnFailEvent != null)
                    {
                        OnFailEvent.Invoke();
                    }
                }
                });
        });
    }


    public void ReadData(string _documentID, UnityAction OnSuccessEvent = null, UnityAction OnFailEvent = null)
    {
        SetDocumentID(_documentID);
        ReadData(OnSuccessEvent, OnFailEvent);
    }    

    public void SetDocumentID(string _documentID)
    {
        documentID = _documentID;
    }


    private Dictionary<string, object> ConvertSampleDataToDictionary(SampleData sampleData)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        data["logo"] = sampleData.logo;
        data["schoolName"] = sampleData.schoolName;
        data["photonAppID"] = sampleData.photonAppID;
        data["seatInfo_Stu"] = sampleData.seatInfo_Stu;
        data["seatInfo_Teacher"] = sampleData.seatInfo_Teacher;

        return data;
    }

    private T GetValueOrDefault<T>(Dictionary<string, object> data, string key)
    {
        if (data.TryGetValue(key, out object value) && value is T)
        {
            return (T)value;
        }

        return default;
    }

    private const string FirestoreProjectId = "creativexrworld";
    private const string FirestoreApiKey = "AIzaSyCioQs9R6V1MT28lttoHF2HXDD80JoiNL4";

    private IEnumerator ReadDataViaRest(UnityAction OnSuccessEvent, UnityAction OnFailEvent)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{FirestoreProjectId}/databases/(default)/documents/{cllection}/{documentID}?key={FirestoreApiKey}";
        Debug.Log($"[Firebase REST] GET {url}");

        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Firebase REST] Failed: {req.responseCode} {req.error}");
                OnFailEvent?.Invoke();
                yield break;
            }

            string json = req.downloadHandler.text;
            Debug.Log($"[Firebase REST] Response: {json}");

            if (!json.Contains("\"fields\""))
            {
                Debug.Log($"[Firebase REST] Document not found: {documentID}");
                OnFailEvent?.Invoke();
                yield break;
            }

            sampleData.logo            = GetRestStringField(json, "logo");
            sampleData.schoolName      = GetRestStringField(json, "schoolName");
            sampleData.photonAppID     = GetRestStringField(json, "photonAppID");
            sampleData.seatInfo_Stu     = GetRestIntField(json, "seatInfo_Stu");
            sampleData.seatInfo_Teacher = GetRestIntField(json, "seatInfo_Teacher");

            SetLicenseInformation(sampleData);
            OnSuccessEvent?.Invoke();
        }
    }

    private string GetRestStringField(string json, string fieldName)
    {
        int fIdx = json.IndexOf($"\"{fieldName}\"");
        if (fIdx < 0) return "";
        int svIdx = json.IndexOf("\"stringValue\"", fIdx);
        if (svIdx < 0) return "";
        int colon = json.IndexOf(':', svIdx);
        int q1 = json.IndexOf('"', colon + 1);
        int q2 = json.IndexOf('"', q1 + 1);
        return (q1 >= 0 && q2 > q1) ? json.Substring(q1 + 1, q2 - q1 - 1) : "";
    }

    private int GetRestIntField(string json, string fieldName)
    {
        int fIdx = json.IndexOf($"\"{fieldName}\"");
        if (fIdx < 0) return 0;
        int ivIdx = json.IndexOf("\"integerValue\"", fIdx);
        if (ivIdx < 0) return 0;
        int colon = json.IndexOf(':', ivIdx);
        int q1 = json.IndexOf('"', colon + 1);
        int q2 = json.IndexOf('"', q1 + 1);
        if (q1 < 0 || q2 <= q1) return 0;
        int.TryParse(json.Substring(q1 + 1, q2 - q1 - 1), out int result);
        return result;
    }

    private void SetLicenseInformation(SampleData sampleData)
    {
        if (licenseInformation == null) return;

        licenseInformation.key = documentID;

        licenseInformation.logo = sampleData.logo;
        licenseInformation.schoolName = sampleData.schoolName;
        licenseInformation.photonAppID = sampleData.photonAppID;
        licenseInformation.seatInfo_Stu = sampleData.seatInfo_Stu;
        licenseInformation.seatInfo_Teacher = sampleData.seatInfo_Teacher;
    }

}

[System.Serializable]
public class SampleData
{
    public string logo;
    public string schoolName;
    public string photonAppID;
    public int seatInfo_Stu;
    public int seatInfo_Teacher;
}
