using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class PanoramicPhotoManager : MonoBehaviour
{
    public GameObject panoramicObj;
    public Texture initTexture;

    private Material panoramicPhoto;

    private void Awake()
    {
        GameObject obj =  GameObject.Instantiate(panoramicObj, Camera.main.transform.parent);
        panoramicPhoto = obj.GetComponent<Renderer>().material;
    }

    private void OnEnable()
    {
        Fade(initTexture, 0.5f);
    }

    private void OnDisable()
    { 
        Fade(initTexture, 0.5f);
    }

    #region PublicMethod

    [Button]
    public void Fade(Texture targetTexture)
    {
        DOTween.Kill(panoramicPhoto);

        if(panoramicPhoto.GetFloat("_Blend") == 0f)
        {
            panoramicPhoto.SetTexture("_MainTex2", targetTexture);
            panoramicPhoto.DOFloat(1f, "_Blend", 0.5f);
        }
        else
        {
            panoramicPhoto.SetTexture("_MainTex", targetTexture);
            panoramicPhoto.DOFloat(0f, "_Blend", 0.5f);
        }
    }

    public void Fade(Texture targetTexture, float duration)
    {
        DOTween.Kill(panoramicPhoto);

        if (panoramicPhoto.GetFloat("_Blend") == 0f)
        {
            panoramicPhoto.SetTexture("_MainTex2", targetTexture);
            panoramicPhoto.DOFloat(1f, "_Blend", duration);
        }
        else
        {
            panoramicPhoto.SetTexture("_MainTex", targetTexture);
            panoramicPhoto.DOFloat(0f, "_Blend", duration);
        }
    }

    public void FadeInitTexture()
    {
        Fade(initTexture, 0.5f);
    }


    #endregion
}
