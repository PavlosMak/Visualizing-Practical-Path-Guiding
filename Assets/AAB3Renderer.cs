using UnityEngine;

[ExecuteAlways]
public class AAB3Renderer : MonoBehaviour {
    
    [SerializeField] private Bounds bounds;

    void SetTransformFromBounds() {
        var scaleX = Mathf.Abs(bounds.max.x - bounds.min.x);
        var scaleY = Mathf.Abs(bounds.max.y - bounds.min.y);
        var scaleZ = 5.0f * Mathf.Abs(bounds.max.z - bounds.min.z) / 360.0f; //We scale to have max be at depth 5
        this.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }
    
    void Update() {
        transform.position = bounds.center;
        transform.localScale = bounds.extents;
    }
    
}