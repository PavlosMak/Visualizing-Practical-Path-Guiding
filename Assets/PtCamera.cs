using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PtCamera : MonoBehaviour 
{

    [SerializeField] private Vector2 origin;

    [SerializeField] private LineRenderer lr;
    [SerializeField] private Rigidbody2D rb2d;


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
        
        RaycastHit2D hit = Physics2D.Raycast(origin, dir);
        Debug.Log(hit.point);
        Debug.DrawRay(origin, dir*hit.distance, Color.magenta);
    }

    void Update()
    {
        CastRay(); 
    }
}
