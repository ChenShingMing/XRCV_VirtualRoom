using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PanoramicController : MonoBehaviour
{
    public Renderer panoramicSphere;
    public Texture transparent;

    private Material panoramicPhoto;

    private void Awake()
    {
        panoramicPhoto = panoramicSphere.material;
        panoramicPhoto.SetFloat("_Blend", 0);
        Finish();
    }

    public void Use(Texture targetTexture)
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

    public void Finish()
    {
        Use(transparent);
    }

}
