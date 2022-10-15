using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventTrigger : MonoBehaviour
{
    public float time;
    public bool triggerWhenStart;
    public UnityEvent unityEvent;

    
    public void Start()
    {
        if (triggerWhenStart)
        {
            InvokeRepeating("TriggerEvent", 0f, time);
        }
        else
        {
            InvokeRepeating("TriggerEvent", time, time);
        }
    }


    public void TriggerEvent()
    {
        if(unityEvent != null)
        {
            unityEvent.Invoke();
        }
    }
}
