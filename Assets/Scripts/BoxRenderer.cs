using UnityEngine;

[ExecuteAlways]
public class BoxRenderer : MonoBehaviour {
    public Vector2 topRight;
    [SerializeField] private LineRenderer lr;
    [SerializeField] private float width = 0.5f;


    public Vector2 VertBotLeft() {
        return transform.position;
    }

    public Vector2 VertBotRight() {
        return (Vector2)transform.position + new Vector2(topRight.x, 0);
    }

    public Vector2 VertTopLeft() {
        return (Vector2)transform.position + new Vector2(0, topRight.y);
    }

    public Vector2 VertTopRight() {
        return (Vector2)transform.position + topRight;
    }

    private void Start() {
        lr.positionCount = 4;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.loop = true;
    }

    private void Update() {
        
        lr.positionCount = 4;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.loop = true;
    
        Debug.Log("a");
        
        lr.SetPosition(0, VertBotLeft());
        lr.SetPosition(1, VertBotRight());
        lr.SetPosition(2, VertTopRight());
        lr.SetPosition(3, VertTopLeft());

        // set width
        lr.startWidth = width;
        lr.endWidth = width;
    }
}