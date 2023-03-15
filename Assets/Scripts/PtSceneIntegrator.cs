using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class PtSceneIntegrator : MonoBehaviour
{

    [SerializeField] string path = "Assets/radiance.txt";
    [SerializeField] float stepSize = 0.01f;


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
    void Start()
    {
        Segments.Add(new Segment(new Vector3(0.01f,3f,0f),new Vector3(7.01f,3,0), new Vector3(0,-1,0)));
        Segments.Add(new Segment(new Vector3(7.01f, 3, 0), new Vector3(7.01f,-6,0), new Vector3(-1,0,0)));
        Segments.Add(new Segment(new Vector3(7.01f,-6,0), new Vector3(2.01f,-6,0), new Vector3(0,1,0)));
        Segments.Add(new Segment(new Vector3(2.01f,-6,0), new Vector3(2.01f,-4,0), new Vector3(1,0,0)));
        Segments.Add(new Segment(new Vector3(2.01f,-4,0), new Vector3(5.01f,-4,0), new Vector3(0,-1,0)));
        Segments.Add(new Segment(new Vector3(5.01f,-4,0), new Vector3(5.01f,-3,0), new Vector3(1,0,0)));
        Segments.Add(new Segment(new Vector3(5.01f,-3,0), new Vector3(0.01f,-3,0), new Vector3(0,1,0)));
    }

    //Visualizes the points getting samples in the editor view
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red; 
    //     foreach(Vector3 point in points) {
    //         Gizmos.DrawSphere(point, 0.01f);
    //     }
    // }

    Color[,,] EvaluateScene(int samples) {
        Color[,,] result = new Color[xRange,yRange,180];
        HashSet<Vector2> seenPoints = new HashSet<Vector2>();
        Debug.Log("Integrating Scene...");
        foreach(Segment segment in Segments) {
            float distance = Vector3.Distance(segment.start, segment.end);
            int lineSamples = (int) Mathf.Floor(distance / stepSize);
            for(int i = 0; i < lineSamples; i++) {
                Vector3 sample = Vector3.Lerp(segment.start, segment.end, i*stepSize);
                //We check if the point has been encountered before
                if(seenPoints.Contains(sample)) {
                    continue;
                }
                seenPoints.Add(sample);
                // File.AppendAllText(path, System.String.Format("{0} {1}\n", sample.x, sample.y));
                EvaluatePoint(sample, segment.normal, samples);
                // pointsToNormals.Add(sample, segment.normal);
            }
        }
        Debug.Log("Integration completed");
        return result;
    }

    void EvaluatePoint(Vector2 point, Vector2 normal, int samples) {
        // Color[] result = new Color[180];
        float invSamples = 1.0f / ((float) samples);
        var result = Color.black;
        for(int i = -90; i < 90; i++) {
            for(int j = 0; j < samples; j++) {
                result = PtUtils.addColors(result, EvaluateDirection(point, normal, i));
            }
            result = PtUtils.multScalarColor(invSamples,  result);
            File.AppendAllText(path, System.String.Format("{0} {1} {2} : {3}\n", point.x, point.y, i, result));
        }
        // return result;
    }

    Color EvaluateDirection(Vector2 point, Vector2 normal, float theta) {
        Vector2 dir = Quaternion.Euler(0, 0, theta)*normal;
        return CastRayFull(point, dir);
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
                finalColor = PtUtils.addColors(finalColor, PtUtils.multColors(beta,brdf.GetEmission())); 
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
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I)) {
            EvaluateScene(maxSamples);
        }
        
    }
}
