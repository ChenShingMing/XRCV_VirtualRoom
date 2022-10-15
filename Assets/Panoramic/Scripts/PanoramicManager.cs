using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PanoramicManager : SerializedMonoBehaviour
{
    public static PanoramicManager ins;

    public PanoramicController controller;
    public Dictionary<string, Texture> panoramicDict;

    private void Awake()
    {
        if(ins == null)
        {
            ins = this;
        }
    }


    [Button]
    public void UsePanoramic(string textureKey)
    {
        if(panoramicDict.ContainsKey(textureKey))
        {
            controller.Use(panoramicDict[textureKey]);
        }
        else
        {
            Debug.Log("There are any panoramic named " + textureKey);
        }
    }

    [Button]
    public void UsePanoramic(Texture texture)
    {
        controller.Use(texture);
    }

    [Button]
    public void FinishPanoramic()
    {
        controller.Finish();
    }

}
