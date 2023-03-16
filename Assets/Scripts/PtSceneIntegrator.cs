using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PtSceneIntegrator : MonoBehaviour {
    [SerializeField] string path = "Assets/radiance_new.txt";
    [SerializeField] private float fineness = 10;
    [SerializeField] private float nAngles = 360;
    
    // visualization 
    [SerializeField] private bool visualizeDirs = false;
    [SerializeField] private float dirRayLen = 0.3f;
    private List<Vector2> sampledPoints = new();
    private List<Ray> sampledDirs = new();

    struct Segment {
        public Vector3 start;
        public Vector3 end;
        public Vector3 normal;

        public Segment(Vector3 start, Vector3 end, Vector3 normal) {
            this.start = start;
            this.end = end;
            this.normal = normal;
        }
    }

    [SerializeField] private int maxDepth = 4;
    [SerializeField] private int maxSamples = 4;

    private List<Segment> Segments = new List<Segment>();

    private float yMin;
    private int xRange;

    private int yRange;

    // [SerializeField] List<GameObject> meshes = new List<GameObject>();
    private Dictionary<Vector3, Vector3> pointsToNormals = new Dictionary<Vector3, Vector3>();
    private float epsilon = 0.001f;

    // Start is called before the first frame update
    void Start() {
        Segments.Add(new Segment(new Vector3(0f, 3f, 0), new Vector3(7f, 3, 0), new Vector3(0, -1, 0)));
        Segments.Add(new Segment(new Vector3(7f, 3f, 0), new Vector3(7f, -6, 0), new Vector3(-1, 0, 0)));
        Segments.Add(new Segment(new Vector3(7f, -6, 0), new Vector3(2f, -6, 0), new Vector3(0, 1, 0)));
        Segments.Add(new Segment(new Vector3(2f, -6, 0), new Vector3(2f, -4, 0), new Vector3(1, 0, 0)));
        Segments.Add(new Segment(new Vector3(2f, -4, 0), new Vector3(5f, -4, 0), new Vector3(0, -1, 0)));
        Segments.Add(new Segment(new Vector3(5f, -4, 0), new Vector3(5f, -3, 0), new Vector3(1, 0, 0)));
        Segments.Add(new Segment(new Vector3(5f, -3, 0), new Vector3(0f, -3, 0), new Vector3(0, 1, 0)));
    }

    // Visualizes the points getting samples in the editor view
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        foreach (Vector3 point in sampledPoints) {
            Gizmos.DrawSphere(point, 0.01f);
        }
    }

    void EvaluateScene(int samples) {
        HashSet<Vector2> seenPoints = new HashSet<Vector2>();
        Debug.Log("Integrating Scene...");
        foreach (var segment in Segments) {
            float segmentLength = Vector3.Distance(segment.start, segment.end);
            int nPoints = (int)Mathf.Floor(fineness * segmentLength);

            for (int i = 0; i < nPoints; i++) {
                Vector3 sample = Vector3.Lerp(segment.start, segment.end, (float)i / nPoints);

                sampledPoints.Add(sample);

                //We check if the point has been encountered before
                if (seenPoints.Contains(sample)) {
                    continue;
                }

                seenPoints.Add(sample);

                EvaluatePoint(sample, segment.normal, samples);
            }
        }

        Debug.Log("Integration completed");
    }

    void EvaluatePoint(Vector2 point, Vector2 normal, int nSamples) {
        float invSamples = 1.0f / nSamples;

        for (int i = 0; i < nAngles; i++) {
            var theta = Mathf.Lerp(0, 360, i / nAngles);
        
            var result = Color.black;
            for (int sample = 0; sample < nSamples; sample++) {
                result = PtUtils.addColors(result, EvaluateDirection(point, normal, theta));
            }
        
            result = PtUtils.multScalarColor(invSamples, result);
            
            File.AppendAllText(path, String.Format("{0} {1} {2} : {3}\n", point.x, point.y, i, result));
            
        }

        // // for each angle, evaluate radiance
        // for (int i = -90; i < 90; i++) {
        //     // evaluate over n samples
        //     var result = Color.black;
        //     for (int sample = 0; sample < nSamples; sample++) {
        //         result = PtUtils.addColors(result, EvaluateDirection(point, normal, i));
        //     }
        //
        //     result = PtUtils.multScalarColor(invSamples, result);
        //
        //     File.AppendAllText(path, String.Format("{0} {1} {2} : {3}\n", point.x, point.y, i, result));
        // }
    }

    Color EvaluateDirection(Vector2 point, Vector2 normal, float theta) {
        Vector2 dir = new Vector2(Mathf.Cos(Mathf.Deg2Rad * theta), Mathf.Sin(Mathf.Deg2Rad * theta)).normalized;
        sampledDirs.Add(new Ray(point, dir));
    
        // INSTEAD OF RETURN BLACK DONT DO IT
        // don't bother casting ray if facing backwards
        if (Vector2.Dot(dir, normal) < 0) {
            return Color.magenta;
        }
         
        return CastRayFull(point + normal * 0.001f, dir);
    }

    Color CastRayFull(Vector2 rayOrigin, Vector2 dir) {
        // Init the path tracing parameters
        dir = dir.normalized;

        var beta = new Color(1.0f, 1.0f, 1.0f);
        Color finalColor = Color.black;


        for (int i = 0; i < maxDepth; i++) {
            // intersect ray with scene
            var hit = Physics2D.Raycast(rayOrigin, dir);

            // if no hit, terminate path
            if (hit.collider == null) {
                // DrawRay(dir, rayOrigin, 15, Color.red);
                break;
            }

            var hitObject = hit.transform.gameObject;
            var brdf = hitObject.GetComponent<PtMaterial>();

            // If we intersect a light
            if (hitObject.CompareTag("Light")) {
                // DrawRay(dir, rayOrigin, hit.distance, brdf.GetEmission());
                finalColor = PtUtils.addColors(finalColor, PtUtils.multColors(beta, brdf.GetEmission()));
                break;
            }

            // DrawRay(dir, rayOrigin, hit.distance, Color.magenta);

            var fSample = brdf.SampleF(dir, hit.normal);
            var fColor = fSample.COLOR;
            var pdf = fSample.PDF;

            beta = PtUtils.multScalarColor(
                Mathf.Abs(Vector2.Dot(dir.normalized, fSample.OUT_DIR.normalized)) / pdf,
                PtUtils.multColors(fColor, beta));

            dir = fSample.OUT_DIR;
            rayOrigin = hit.point + hit.normal * epsilon;
        }

        return finalColor;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.I)) {
            EvaluateScene(maxSamples);
        }

        if (visualizeDirs) {
            foreach (var r in sampledDirs) {
                Debug.DrawRay(r.origin, r.direction * dirRayLen, Color.magenta); 
            }
        }
    }
}