using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StarMap/LocationData")]
public class LocationData : ScriptableObject
{
    public List<Location> locationList;
}
