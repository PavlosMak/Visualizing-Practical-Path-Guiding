using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arc : MonoBehaviour
{

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int segments = 20; 
    [SerializeField] private float startAngle = 90.0f;
    [SerializeField] private float endAngle = -90.0f;
    [SerializeField] private float radius = 1.0f;
    [SerializeField] private Material segmentMaterial;

    private Vector3 prevPosition;
    private Quaternion prevRotation;
    private List<LineRenderer> segmentRenderers = new List<LineRenderer>();

    // Start is called before the first frame update
    void Start()
    {
        DrawArc(startAngle, endAngle, radius, lineRenderer, segments);
    }

    // Update is called once per frame
    void Update()
    {
        if((prevPosition!=transform.position) || (prevRotation != transform.rotation)) {
            DrawArc(startAngle, endAngle, radius, lineRenderer, segments);
        }
        prevPosition = transform.position;
        prevRotation = transform.rotation;
        //TODO: The following is for testing
        if(Input.GetKeyDown(KeyCode.Tab)) {
            ColorSegment(-45.0f,0,Color.green);
            ColorSegment(45,90,Color.cyan);
            ColorSegment(-90,-50,Color.red);
        } 
        if(Input.GetKeyDown(KeyCode.Backspace)) {
            ClearDrawnSegments();
        }
    }


    public void ClearDrawnSegments() {
        foreach (var lr in segmentRenderers) {
            lr.positionCount = 0;
        }
        for(int c = 0; c < gameObject.transform.childCount; c++) {
            GameObject.Destroy(
                gameObject.transform.GetChild(c).gameObject
            );
        }
        segmentRenderers = new List<LineRenderer>();
    }

    public void DrawArc(float startAngle, float endAngle, float radius, LineRenderer lineRenderer, int segments) {
        // lineRenderer.positionCount = 0; 
        float arcLength = endAngle - startAngle;
        float angleStep = (arcLength / segments);
        float angle = startAngle;
        lineRenderer.positionCount = 0;
        lineRenderer.positionCount = segments;
        for(int i = 0; i < segments; i++) {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y =  Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            Vector3 point = new Vector3(x,y,0);
            lineRenderer.SetPosition(i, point);
            angle += angleStep;
        }
    }

    public void ColorSegment(float start, float end, Color color) {
        
        GameObject child = new GameObject();
        child.name = "Segment: " + start + "-" + end;
        child.transform.parent = gameObject.transform;
        child.transform.position = gameObject.transform.position - new Vector3(0,0,0.1f);
        child.transform.rotation = gameObject.transform.rotation;

        //Add a LineRenderer to it
        LineRenderer lr = child.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.startWidth = 0.06f;
        lr.endWidth = lr.startWidth;
        lr.material = segmentMaterial;
        lr.material.color = color;
        segmentRenderers.Add(lr);
        DrawArc(start, end, radius, lr, 10);
    }
}
