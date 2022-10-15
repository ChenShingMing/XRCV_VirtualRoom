using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;


public class Pointer : MonoBehaviour
{

    public RectTransform imageFX;
    public float duration;
    public float scale;

    public Vector3 pointerPos;
    
    private RectTransform rectTransform;
    private Vector3 posTemp; //lerp的數值


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        SetPointPosHandle();
    }

    [Button]
    public void Click()
    {
        RectTransform imageFXClone = GameObject.Instantiate(imageFX, imageFX.position, imageFX.rotation, imageFX.parent);
        Image image = imageFXClone.GetComponent<Image>();

        image.DOColor(new Color(image.color.r, image.color.g, image.color.b, 0f), duration);
        imageFXClone.DOScale(new Vector3(scale, scale, scale), duration)
            .OnComplete(() =>
            {
                Finish(imageFXClone.gameObject);
            });
    }


    private void Finish(GameObject obj)
    {
        Destroy(obj);
    }

    private void SetPointPosHandle()
    {
        posTemp = Vector3.Lerp(rectTransform.position, pointerPos, Time.deltaTime * 15);
        rectTransform.position = posTemp;
    }

}
