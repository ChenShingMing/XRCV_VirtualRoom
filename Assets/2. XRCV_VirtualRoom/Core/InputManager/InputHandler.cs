using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputHandler : MonoBehaviour
{
    public virtual void Update()
    {
        InputHandle();
    }

    public abstract void InputHandle();
}
