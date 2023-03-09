using UnityEngine;

[ExecuteAlways]
public class LineSegmentRenderer : MonoBehaviour {
    // ReadOnly! not sure if this is the best way to handle this btw
    [HideInInspector] public Vector2 p0;
    [HideInInspector] public Vector2 p1;

    [SerializeField] private Vector2 offset;
    [SerializeField] private LineRenderer lr;
    [SerializeField] private float width = 0.5f;
    
    public Vector2 P0() {
        return transform.position;
    }

    public Vector2 P1() {
        return (Vector2)transform.position + offset;
    }

    private void Start() {
        lr.positionCount = 2;
    }

    private void Update() {
        // set vertices
        Vector2 pos = transform.position;
        p0 = pos;
        p1 = pos + offset;

        lr.SetPosition(0, p0);
        lr.SetPosition(1, p1);

        // set width
        lr.startWidth = width;
        lr.endWidth = width;
    }
}