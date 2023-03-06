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

    [FormerlySerializedAs("ray")] [SerializeField] private GameObject rayPrefab;

    [SerializeField] private int resolution;

    private List<Ray> rayBuffer = new List<Ray>();

    // void DrawTicks()
    // {
    //     var initialTickPos = new Vector3(focalLen, 0, size / 2);
    //     var tickStep = size / resolution;
    //     
    //     for (int i = 0; i < resolution+1; i++)
    //     {
    //         var tickPos = initialTickPos + i * new Vector3(0, 0, tickStep);
    //         var tickPosL = tickPos - new Vector3(-1, 0, 0);
    //         var tickPosR = tickPos - new Vector3(1, 0, 0);
    //         
    //         var lr = gameObject.AddComponent<LineRenderer>();
    //         lr.positionCount = 2;
    //         lr.SetPosition(0, tickPosL);
    //         lr.SetPosition(1, tickPosR);
    //
    //     }
    // }


    void Start() {
        lr.positionCount = 3;
        origin = transform.position;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + new Vector2(focalLen, size / 2));
        lr.SetPosition(2, origin + new Vector2(focalLen, -size / 2));
        // CastRay();
    }

    void OnValidate() {
        origin = transform.position;
        lr.positionCount = 3;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + new Vector2(focalLen, size / 2));
        lr.SetPosition(2, origin + new Vector2(focalLen, -size / 2));

    }
    void CastRay(Vector2 dir) {
        rayBuffer = new List<Ray>();
        var rayOrigin = origin;
        dir = dir.normalized;

        
        for (int i = 0; i < maxDepth; i++) {
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dir);
            if (hit.collider == null) {
                rayBuffer.Add(new Ray(dir, rayOrigin, 15, Color.red));
                break;
            }

            if (hit.collider.gameObject.tag == "Light") {
                rayBuffer.Add(new Ray(dir, rayOrigin, hit.distance, Color.yellow));
                break;
            }

            Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.green);
           
            
            rayBuffer.Add(new Ray(dir, rayOrigin, hit.distance, Color.magenta));

            dir = SampleHemisphere(dir, hit.point, hit.normal);
            rayOrigin = hit.point + hit.normal * epsilon;
        }
    }

    Vector2 SampleHemisphere(Vector2 wo, Vector2 point, Vector2 normal) {
        return Quaternion.Euler(0, 0, Random.Range(-90.0f, 90.0f)) * normal;
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
            

        foreach (var ray in rayBuffer) {
            var r = Instantiate(rayPrefab, ray.og, Quaternion.identity);
            r.GetComponent<RayRenderer>().direction = ray.dir;
            r.GetComponent<RayRenderer>().length = ray.t;
            r.GetComponent<LineRenderer>().material.color = ray.color;
            
            Debug.DrawRay(ray.og, ray.dir * ray.t, ray.color);
            
        }
    }
}