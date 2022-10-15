using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


[System.Serializable]
public class Location 
{
    public string name;
    public Vector2 latitudeAndLongitude;

    public Sprite picture;
    public Texture day;
    public Texture night;
}
