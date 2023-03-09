using UnityEngine;

[ExecuteAlways]
public class RayRenderer : MonoBehaviour {
    public Vector2 direction;
    public float length;

    [SerializeField] private LineRenderer lr;
    [SerializeField] private float rayWidth = 0.03f;

    private void Start() {
        lr.positionCount = 2;
        Vector2 pos = transform.position;
        lr.SetPosition(0, pos);
        lr.SetPosition(1, pos + direction.normalized * length);
        lr.startWidth = rayWidth;
        lr.endWidth = rayWidth;
    }
}