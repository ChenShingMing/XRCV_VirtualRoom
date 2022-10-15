using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class GazeSphere  {

    public static SphereCollider sphereCollider;
    public static SphereCollider sphereColliderline;
    static float radius = 100;
    static float lineradius = 10f;

    public static Vector3 RayHitOnSphere(Ray ray)
    {
        creatSphereCollider();

        RaycastHit hit;
        ray.origin = ray.GetPoint(radius + 10);
        ray.direction = -ray.direction;
        if (sphereCollider.Raycast(ray, out hit, radius + 10))
        {

            return hit.point;
        }
        else {
            return Vector3.zero;
        }
    }

    static void creatSphereCollider()
    {
        if (sphereCollider != null) return;
        GameObject go = new GameObject("GazeSphere");
        
        //將投射的碰撞球放在相機裡面
        go.transform.SetParent(Camera.main.transform);
        go.transform.localPosition = new Vector3();

        sphereCollider = go.AddComponent<SphereCollider>();
        sphereCollider.radius = radius;
        go.layer = LayerMask.GetMask("IgnoreRaycast");
    }

    public static Vector3 RayHitOnSphereLine(Ray ray)
    {
        creatLineSphereCollider();

        RaycastHit hit;
        ray.origin = ray.GetPoint(lineradius + 10);
        ray.direction = -ray.direction;
        if (sphereColliderline.Raycast(ray, out hit, lineradius + 10))
        {
            return hit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }

    static void creatLineSphereCollider()
    {
        if (sphereColliderline != null) return;
        GameObject go = new GameObject("GazeSphereLine");

        go.transform.SetParent(Camera.main.transform);
        go.transform.localPosition = new Vector3();

        sphereColliderline = go.AddComponent<SphereCollider>();
        sphereColliderline.radius = lineradius;
        go.layer = LayerMask.GetMask("IgnoreRaycast");
    }
}
