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

    void Start()
    {
        // 獲取 Firestore 實例
        firestore = FirebaseFirestore.DefaultInstance;
    }

    [Button]
    public void AddData()
    {
        firestore = FirebaseFirestore.DefaultInstance;

        // 將 sampleData 轉換為字典
        Dictionary<string, object> data = ConvertSampleDataToDictionary(sampleData);

        // 取得 Firestore 的 DocumentReference
        DocumentReference docRef = FirebaseFirestore.DefaultInstance.Collection(cllection).Document(documentID);

        // 將資料寫入 Firestore
        docRef.SetAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SetAsync 被取消。");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SetAsync 遇到錯誤：" + task.Exception);
                return;
            }

            Debug.Log("資料成功新增至 Firestore。");
        });
    }

    [Button]
    public void ReadData(UnityAction OnSuccessEvent = null, UnityAction OnFailEvent = null)
    {
        firestore = FirebaseFirestore.DefaultInstance;

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
