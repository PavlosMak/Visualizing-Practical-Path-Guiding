using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LineMesh : MonoBehaviour
{

    [SerializeField] private LineRenderer lr;
    [SerializeField] private EdgeCollider2D edgeCollider;
    [SerializeField] public List<Vector2> points;

    void loadPoints()
    {
        float xoffset = transform.position.x;
        float yoffset = transform.position.z;
        
        Debug.Log(xoffset);
         
        lr.positionCount = points.Count;
        if(edgeCollider != null)
            edgeCollider.SetPoints(points);

        for (int i = 0; i < points.Count; i++)
        {
            lr.SetPosition(i, new Vector2(points[i].x + xoffset, points[i].y + yoffset));
        }
        
    }
    
    void OnValidate()
    {
        loadPoints();
    }

    void Start()
    {
        loadPoints();
    }

    void Update()
    {
    

    }
}
