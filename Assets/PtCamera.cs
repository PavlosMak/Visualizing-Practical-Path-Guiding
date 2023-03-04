using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PtCamera : MonoBehaviour 
{
    [SerializeField] private Vector2 origin;

    [SerializeField] private LineRenderer lr;

    [SerializeField] private float focalLen;
    [SerializeField] private float size;

    [SerializeField] private int resolution;

    // void DrawTicks()
    // {
    //     var initialTickPos = new Vector3(focalLen, 0, size / 2);
    //     var tickStep = size / resolution;
    //     
    //     for (int i = 0; i < resolution+1; i++)
    //     {
    //         var tickPos = initialTickPos + i * new Vector3(0, 0, tickStep);
    //         var tickPosL = tickPos - new Vector3(-1, 0, 0);
    //         var tickPosR = tickPos - new Vector3(1, 0, 0);
    //         
    //         var lr = gameObject.AddComponent<LineRenderer>();
    //         lr.positionCount = 2;
    //         lr.SetPosition(0, tickPosL);
    //         lr.SetPosition(1, tickPosR);
    //
    //     }
    // }
    
    void OnValidate()
    {
    
        lr.positionCount = 3;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + new Vector2(focalLen, size/2));
        lr.SetPosition(2, origin + new Vector2(focalLen,-size/2));
    }

    void CastRay()
    {
	
        var dir = new Vector2(1, 0.2f).normalized;
        Debug.DrawRay(origin, dir, Color.magenta);

        var l0 = new Vector2(0, 3);
        var l1 = new Vector2(7, 3);
        Debug.DrawLine(l0, l1);


        var a1 = (l1.y - l0.y) / (l1.x - l0.x);
        var a2 = (dir.y - origin.y) / (dir.x - origin.x);

        var b1 = l0.y;
        var b2 = origin.y;

        // lines are parallel
        if (a1 - a2 != 0)
        {
            return;
        }

        var t = (b2 - b1) / (a1 - a2);
        
        // behind
        if (t < 0)
        {
            return;
        }

        var isect = origin + dir * t;
        







    }

    void Update()
    {
        CastRay();
        
    }
}
