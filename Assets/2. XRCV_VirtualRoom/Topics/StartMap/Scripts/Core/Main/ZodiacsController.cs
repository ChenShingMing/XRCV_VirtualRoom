using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZodiacsController : MonoBehaviour 
{
    public CreateHipHierarchy createHipHierarchy;

    // Update is called once per frame
    void FixedUpdate () 
    {
        CheckAllZodiacsStatus();
    }

    public void CheckAllZodiacsStatus()
    {
        for (int i = 0; i < createHipHierarchy.zodiac_Datas.Count; i++)
        {
            Vector3 pos = Camera.main.WorldToViewportPoint(createHipHierarchy.zodiac_Datas[i].world_transform.position);

            if (pos.x < 0.8 && pos.x > 0.2
                && pos.y < 0.9 && pos.y > 0.1)
            {
                createHipHierarchy.OpenZodiac(createHipHierarchy.zodiac_Datas[i]);
            }
            else
            {
                createHipHierarchy.CloseZodiac(createHipHierarchy.zodiac_Datas[i]);
            }
        }
    }
    
}
