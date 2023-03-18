using UnityEngine;

[ExecuteAlways]
public class AAB3Renderer : MonoBehaviour {
    
    [SerializeField] private Bounds bounds;
    
    void Update() {
        transform.position = bounds.center;
        transform.localScale = bounds.extents;
    }
    
}