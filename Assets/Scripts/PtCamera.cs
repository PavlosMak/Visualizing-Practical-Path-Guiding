using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class PtCamera : MonoBehaviour {
    
    private Vector2 origin = new Vector2(0f, 0f);
    [SerializeField] private int maxDepth = 3;
    [SerializeField] private float epsilon = 0.001f;
    [SerializeField] private LineRenderer lr;
    [SerializeField] private Rigidbody2D rb2d;

    [SerializeField] private float focalLen;
    [SerializeField] private float size;

    private List<GameObject> instansiatedRays = new List<GameObject>();

    [FormerlySerializedAs("ray")] [SerializeField] private GameObject rayPrefab;

    [SerializeField] private int resolution;

    private List<Ray> rayBuffer = new List<Ray>();

    void Start() {
        lr.positionCount = 3;
        origin = transform.position;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + new Vector2(focalLen, size / 2));
        lr.SetPosition(2, origin + new Vector2(focalLen, -size / 2));
    }

    void OnValidate() {
        origin = transform.position;
        lr.positionCount = 3;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + new Vector2(focalLen, size / 2));
        lr.SetPosition(2, origin + new Vector2(focalLen, -size / 2));

    }
    Color CastRay(Vector2 dir) {
        //Clear the ray buffer
        //Clear the instantiated ray objects
        foreach (var rayObject in instansiatedRays) {
            Destroy(rayObject);
        }
        instansiatedRays = new List<GameObject>();

        // Init the path tracing parameters
        var rayOrigin = origin;
        dir = dir.normalized;
        Color beta = new Color(1.0f, 1.0f, 1.0f); 
        Color finalColor = Color.black;

        for (int i = 0; i < maxDepth; i++) {
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dir);
            if (hit.collider == null) {
                rayBuffer.Add(new Ray(dir, rayOrigin, 15, Color.red));
                DrawRay(dir, rayOrigin, 15, Color.red);
                break;
            }

            GameObject hitObject = hit.transform.gameObject;
            PtMaterial brdf = hitObject.GetComponent<PtMaterial>(); 
            
            if (hitObject.tag == "Light") {
                rayBuffer.Add(new Ray(dir, rayOrigin, hit.distance, Color.yellow));
                DrawRay(dir, rayOrigin, hit.distance, brdf.GetEmission());
                finalColor = PtUtils.addColors(finalColor, PtUtils.multColors(beta,brdf.GetEmission())); 
            }

            DrawRay(dir, rayOrigin, hit.distance, Color.magenta);

            BRDFSample fSample = brdf.SampleF(dir, hit.normal);            
            Color fColor = fSample.COLOR;            
            float pdf = fSample.PDF;
            Debug.Log("PDf " + pdf);
            beta = PtUtils.multScalarColor(Mathf.Abs(Vector2.Dot(dir.normalized,fSample.OUT_DIR.normalized))/pdf, PtUtils.multColors(fColor, beta)); 
            dir = fSample.OUT_DIR;
            rayOrigin = hit.point + hit.normal * epsilon;
        }
        Debug.Log("Final color is: " + finalColor);
        return finalColor;
    }


    private void DrawRay(Vector3 dir, Vector3 rayOrigin, float length, Color color) {
        var r = Instantiate(rayPrefab, rayOrigin, Quaternion.identity);
        r.GetComponent<RayRenderer>().direction = dir;
        r.GetComponent<RayRenderer>().length = length;
        r.GetComponent<LineRenderer>().material.color = color;
        instansiatedRays.Add(r);
    }

    void Update() {
        Vector3 mousePos = Camera.main!.ScreenToWorldPoint(Input.mousePosition);

        Vector2 dir = (new Vector2(mousePos.x, mousePos.y) - origin).normalized;
        Debug.DrawRay(origin, dir * focalLen, Color.blue);

        //Draw the camera mesh
        if (origin != new Vector2(transform.position.x, transform.position.y)) {
            origin = transform.position;
            CastRay(dir);
        }

        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + new Vector2(focalLen, size / 2));
        lr.SetPosition(2, origin + new Vector2(focalLen, -size / 2));

        if (Input.GetButtonDown("Fire1")) {
            CastRay(dir);
        }
    }
}