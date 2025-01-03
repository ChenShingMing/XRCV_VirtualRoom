using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TMControllerBase : MonoBehaviour
{
    public static TMControllerBase instance;
    public abstract void TriggerControlCanvas();
}
