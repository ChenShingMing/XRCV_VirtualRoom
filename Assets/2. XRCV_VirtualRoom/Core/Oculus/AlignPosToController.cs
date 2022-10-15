using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignPosToController : MonoBehaviour
{
    public Transform targetTrans;
    public Transform head;
    public GameObject align;
    public Vector3 offset;
    public float lerpSpeed;

    private void OnEnable()
    {
        align.transform.position = targetTrans.position + offset;
    }

    // Update is called once per frame
    void Update()
    {
        AlignHandle();
    }

    void AlignHandle()
    {
        Vector3 pos = targetTrans.position + (targetTrans.rotation.normalized * offset);

        align.transform.position = Vector3.Lerp(align.transform.position, pos, lerpSpeed * Time.deltaTime);

        align.transform.LookAt(head);
        align.transform.forward *= -1;
    }

}
