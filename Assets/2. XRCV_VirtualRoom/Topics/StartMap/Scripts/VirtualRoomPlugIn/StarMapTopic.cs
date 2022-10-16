using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarMapTopic : Topic
{
    StarMapController starMapController;

    public override void OnInit()
    {
        base.OnInit();
        starMapController = controller.GetComponentInChildren<StarMapController>(true);
    }

    public override void TriggerMenu()
    {
        starMapController.TriggerControlCanvas();
    }

    public override void DisableMenu()
    {
        starMapController.DisableCanvas();
    }
}
