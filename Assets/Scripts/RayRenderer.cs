using UnityEngine;

[ExecuteAlways]
public class RayRenderer : MonoBehaviour {
    public Vector2 direction;
    public float length;

    [SerializeField] private LineRenderer lr;


    private void Start() {
        lr.positionCount = 2;
    }

    private void Update() {
        Vector2 pos = transform.position;

        lr.SetPosition(0, pos);
        lr.SetPosition(1, pos + direction.normalized * length);

        transform.GetChild(0);
    }
}