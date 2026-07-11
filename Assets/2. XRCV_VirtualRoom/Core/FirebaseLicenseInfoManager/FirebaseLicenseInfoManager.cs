using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Button]
    public void ReadData(UnityAction OnSuccessEvent = null, UnityAction OnFailEvent = null)
    {
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
