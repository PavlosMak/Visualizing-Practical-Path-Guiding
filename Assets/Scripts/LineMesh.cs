using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LineMesh : MonoBehaviour
{

    [SerializeField] private LineRenderer lr;
    [SerializeField] private EdgeCollider2D edgeCollider;
    [SerializeField] public List<Vector2> points;

    private Vector3 position;

    void loadPoints()
    {
        float xoffset = transform.position.x;
        float yoffset = transform.position.y;
        
        lr.positionCount = 0;
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
        position = transform.position;
        loadPoints();
    }

    void Start()
    {
        position = transform.position;
        loadPoints();
    }

    void Update()
    {
        if(position != transform.position) {
            loadPoints();
            position = transform.position;
        }
    }
}
