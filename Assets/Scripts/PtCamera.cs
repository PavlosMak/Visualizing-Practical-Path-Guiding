using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class PtCamera : MonoBehaviour {
    
    // Parameters
    [SerializeField] private int maxDepth = 3;
    [SerializeField] private float epsilon = 0.001f;
    [SerializeField] private LineRenderer lr;
    [SerializeField] private float focalLen;
    [SerializeField] private float size;
    [SerializeField] private GameObject rayPrefab;
    [SerializeField] private int resolution;
    [SerializeField] private Camera mainCam;
    
    // get the tree
    private AdaptiveSDTree _sdTree = AdaptiveSDTree.Instance;

    // keep track of drawn rays
    private List<GameObject> _instantiatedRays = new();
    
    // path tracing state
    private int _curBounce;
    private RaycastHit2D _hit;
    private Vector2 _prevDir;

    private void DrawCameraFrame() {
        lr.positionCount = 3;
        Vector2 pos = transform.position;
        lr.SetPosition(0, pos);
        lr.SetPosition(1, pos + new Vector2(focalLen, size / 2));
        lr.SetPosition(2, pos + new Vector2(focalLen, -size / 2));
    }

    void ClearDrawnRays() {
        // Destroy all instantiated rays
        foreach (var rayObject in _instantiatedRays) {
            Destroy(rayObject);
        }
        _instantiatedRays = new List<GameObject>();
    }
    
    Color CastRayFull(Vector2 dir) {
        
        // clear rays from screen
        ClearDrawnRays();

        // Init the path tracing parameters
        Vector2 rayOrigin = transform.position;
        dir = dir.normalized;

        var beta = new Color(1.0f, 1.0f, 1.0f);
        var finalColor = Color.black;
        
        // store information about each hit, for updating the tree
        var betas = new List<Color>();
        var hits = new List<RaycastHit2D>();
        
        
        for (_curBounce = 0; _curBounce < maxDepth; _curBounce++) {
            
            // intersect ray with scene
            var hit = Physics2D.Raycast(rayOrigin, dir);

            // if no hit, terminate path
            if (hit.collider == null) {
                // draw the traced ray, in red
                DrawRay(dir, rayOrigin, 15, Color.red);
                break;
            }

            var hitObject = hit.transform.gameObject;
            var brdf = hitObject.GetComponent<PtMaterial>();

            // If we intersect a light
            if (hitObject.CompareTag("Light")) {
                // draw the traced ray, in yellow
                DrawRay(dir, rayOrigin, hit.distance, brdf.GetEmission());
                finalColor = PtUtils.addColors(finalColor, PtUtils.multColors(beta,brdf.GetEmission())); 
                break;
            }
        
            // draw the traced ray
            DrawRay(dir, rayOrigin, hit.distance, Color.magenta);
    
            // sample the BRDF
            var fSample = brdf.SampleF(dir, hit.normal);
            var fColor = fSample.COLOR;
            var pdf = fSample.PDF;

            beta = PtUtils.multScalarColor(
                Mathf.Abs(Vector2.Dot(dir.normalized, fSample.OUT_DIR.normalized)) / pdf,
                PtUtils.multColors(fColor, beta));
            
            // save the beta
            betas.Add(new Color(beta.r, beta.g, beta.b));
            hits.Add(hit);
    
            // obtain new direction
            dir = fSample.OUT_DIR;
            rayOrigin = hit.point + hit.normal * epsilon;
        }

        for (int i = 0; i < hits.Count; i++) {

            var hitI = hits[i];
            var betaI = betas[i];
            
            // store it in the tree
            // _sdTree.
            
        }
        
        
        
        return finalColor;
    }
    
    private void DrawRay(Vector3 dir, Vector3 rayOrigin, float length, Color color) {
        var r = Instantiate(rayPrefab, rayOrigin, Quaternion.identity);
        r.GetComponent<RayRenderer>().direction = dir;
        r.GetComponent<RayRenderer>().length = length;
        r.GetComponent<LineRenderer>().material.color = color;
        _instantiatedRays.Add(r);
    }

    void Update() {
        // TODO handle input in a separate CameraController component
        var position = transform.position;
        var mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 dir = (mousePos - position).normalized;
        Debug.DrawRay(position, dir * focalLen, Color.blue);

        DrawCameraFrame();

        // Handle input (Extract to controller?)
        if (Input.GetButtonDown("Fire1")) {
            CastRayFull(dir);
        }
    }
}