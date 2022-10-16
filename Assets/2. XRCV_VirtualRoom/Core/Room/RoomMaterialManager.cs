using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class RoomMaterialManager : MonoBehaviour
{
    public List<Material> roomMaterials;

    private void Start()
    {
        Use();

        ClassroomManager.ins.OnSetTopic.AddListener(Finish);
        ClassroomManager.ins.OnExitTopic.AddListener(Use);
    }


    [Button]
    public void Use()
    {
        for(int i = 0; i < roomMaterials.Count; i++)
        {
            DOTween.Kill(roomMaterials[i]);
            roomMaterials[i].DOFade(1, 0.5f);
        }
    }

    [Button]
    public void Finish()
    {
        for (int i = 0; i < roomMaterials.Count; i++)
        {
            DOTween.Kill(roomMaterials[i]);
            roomMaterials[i].DOFade(0, 0.5f);
        }
    }
}
