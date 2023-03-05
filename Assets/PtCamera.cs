using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PtCamera : MonoBehaviour 
{

    [SerializeField] private Vector2 origin;
    [SerializeField] private int maxDepth = 3;
    [SerializeField] private float epsilon = 0.001f;
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
        CastRay();
    }

    void CastRay()
    {
        var rayOrigin = origin;
        var dir = new Vector2(1, -0.4f).normalized;
        for(int i = 0; i < maxDepth; i++) {
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dir);
            if(hit.collider == null) {
                Debug.Log("Ray missed");
                break;
            }
            Debug.Log(hit.point);
            Debug.DrawRay(rayOrigin, dir*hit.distance, Color.magenta);
            Debug.DrawRay(hit.point,hit.normal*0.5f,Color.green);
            //TODO: This reflects as if the wall is a perfect mirror, we should change it
            dir = Vector2.Reflect(dir,hit.normal);
            rayOrigin = hit.point + hit.normal*epsilon; 
        }
    }

    void Update()
    {
        CastRay(); 
    }
}
