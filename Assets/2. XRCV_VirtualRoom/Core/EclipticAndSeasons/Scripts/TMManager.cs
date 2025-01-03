using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TMManager : MonoBehaviour
{
    public List<GameObject> topics;

    private void Start()
    {
        SetTM(0);
    }

    [Button]
    public void SetTM(int _index)
    {
        SetTM(topics[_index]);
    }


    public void SetTM(GameObject _topic)
    {
        for (int i = 0; i < topics.Count; i++)
        {
            topics[i].SetActive(false);
        }

        _topic.SetActive(true);
    }
}
