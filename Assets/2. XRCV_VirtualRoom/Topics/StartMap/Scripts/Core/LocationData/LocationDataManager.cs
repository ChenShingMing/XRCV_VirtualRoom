using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.Events;

//Unity - Get data from Google Drive https://www.youtube.com/watch?v=Cawemvq92E0


public class LocationDataManager : MonoBehaviour
{
    public LocationData locationData;
    public string path;
    public UnityEvent OnUpdateComplete;

    private void OnEnable()
    {
        Debug.Log("LocationDataManager OnEnable");
        UpdateData();
    }

    private void OnDisable()
    {
        Debug.Log("LocationDataManager OnDisable");
        DestoryTextureAndClearList();
    }

    #region Public

    [Button]
    public void DebugPath()
    {
        Debug.Log(Path.Combine(Application.streamingAssetsPath, path));
    }


    [Button]
    public void UpdateData()
    {
        UpdateData(locationData);
    }

    public void UpdateData(LocationData locationData)
    {
        DestoryTextureAndClearList();

        string[] dirs;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        dirs = Directory.GetDirectories(Path.Combine(Application.streamingAssetsPath, path));
#elif UNITY_ANDROID
        dirs = Directory.GetDirectories(Application.persistentDataPath + "/" + path);
#endif

        StartCoroutine(DoUpdate(dirs));
    }

#endregion

#region Private

    private IEnumerator DoUpdate(string[] dirs)
    {
        for (int i = 0; i < dirs.Length; i++)
        {
            yield return StartCoroutine(CreateLocationData(dirs[i]));
        }

        if (OnUpdateComplete != null)
        {
            OnUpdateComplete.Invoke();
        }

        Debug.Log("LocationData Update Complete.");
    }

    private IEnumerator CreateLocationData(string dirPath)
    {
        string temp;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        temp = Path.Combine(Application.streamingAssetsPath, path);
#elif UNITY_ANDROID
        temp = Path.Combine(Application.persistentDataPath + "/" + path);
#endif
        string locationPath = dirPath.Replace(temp, "");
        locationPath = locationPath.Remove(0, 1);
        //Debug.Log(locationPath);

        string[] data = locationPath.Split("_");
        string[] locationInfo = data[1].Split(",");

        Texture2D pic = null;
        Texture2D day = null;
        Texture2D night = null;

        string picPath = dirPath + "/picture.jpg";
        string dayPath = dirPath + "/day.png";
        string nightPath = dirPath + "/night.png";


#region GetImg

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(picPath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                pic = DownloadHandlerTexture.GetContent(uwr);
            }
        }

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(dayPath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                day = DownloadHandlerTexture.GetContent(uwr);
            }
        }

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(nightPath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                night = DownloadHandlerTexture.GetContent(uwr);
            }
        }

#endregion


        Location location = new Location();
        location.name = data[0];
        location.latitudeAndLongitude = new Vector2(float.Parse(locationInfo[0]), float.Parse(locationInfo[1]));

        location.picture = pic;
        location.day = day;
        location.night = night;

        locationData.locationList.Add(location);

    }

    private void DestoryTextureAndClearList()
    {
        for (int i = 0; i < locationData.locationList.Count; i++)
        {
            if (locationData.locationList[i].picture != null)
            {
                if (Application.isPlaying) Destroy(locationData.locationList[i].picture);
                else DestroyImmediate(locationData.locationList[i].picture);
            }
            if (locationData.locationList[i].day != null)
            {
                if (Application.isPlaying) Destroy(locationData.locationList[i].day);
                else DestroyImmediate(locationData.locationList[i].day);
            }
            if (locationData.locationList[i].night != null)
            {
                if (Application.isPlaying) Destroy(locationData.locationList[i].night);
                else DestroyImmediate(locationData.locationList[i].night);
            }
        }

        locationData.locationList.Clear();
    }

#endregion
}
