using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PtCamera : MonoBehaviour {
    // Parameters
    [SerializeField] private int maxDepth = 3;
    [SerializeField] private float epsilon = 0.001f;
    [SerializeField] private LineRenderer lr;
    [SerializeField] private float focalLen;
    [SerializeField] private int spamRate;
    [SerializeField] private float size;

    [SerializeField] private GameObject rayPrefab;
    [SerializeField] private Camera cam2D;

    private bool _liveRefresh = true;
    
    // samplePoint
    private Vector2 _samplePoint;

    // get the tree
    private AdaptiveSDTree _sdTree = AdaptiveSDTree.Instance;

    // keep track of drawn rays
    private List<GameObject> _instantiatedRays = new();

    // path tracing state
    private int _curBounce;
    private int _iteration = 0;
    private RaycastHit2D _hit;
    private Vector2 _prevDir;

    private void DrawCameraFrame() {
        lr.positionCount = 3;
        Vector2 pos = transform.position;
        lr.SetPosition(0, pos);
        lr.SetPosition(1, pos + new Vector2(focalLen, size / 2));
        lr.SetPosition(2, pos + new Vector2(focalLen, -size / 2));
    }

    public void ToggleLiveRefresh() {
        _liveRefresh = !_liveRefresh;
    }

    private void LiveRefresh() {
        if (_liveRefresh) {
            _sdTree.DrawLeaves();
        }
    }

    public void AdaptTree() {
        Debug.Log("Adapting Tree");
        _sdTree.Adapt(_iteration);
        _iteration += 1;
    }

    public void ClearDrawnRays() {
        // Destroy all instantiated rays
        foreach (var rayObject in _instantiatedRays) {
            Destroy(rayObject);
        }

        _instantiatedRays = new List<GameObject>();
    }

    Color CastRayFull(Vector2 dir) {
        // Init the path tracing parameters
        Vector2 rayOrigin = _samplePoint;
        dir = dir.normalized;

        var beta = new Color(1.0f, 1.0f, 1.0f);
        var finalColor = Color.black;

        bool lightFound = false;

        // store information about each hit, for updating the tree
        var betas = new List<Color>();
        var hits = new List<RaycastHit2D>();
        var reflectionAngles = new List<float>();

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
                finalColor = PtUtils.addColors(finalColor, PtUtils.multColors(beta, brdf.GetEmission()));
                lightFound = true;
                Debug.Log(finalColor);
                break;
            }

            // draw the traced ray
            DrawRay(dir, rayOrigin, hit.distance, Color.magenta);

            // sample the BRDF
            var fSample = brdf.SampleF(dir, hit.normal);
            var fColor = fSample.COLOR;
            var pdf = fSample.PDF;

            beta = PtUtils.multScalarColor(
                Mathf.Abs(Vector2.Dot(hit.normal, fSample.OUT_DIR.normalized)) / pdf,
                PtUtils.multColors(fColor, beta));

            // save the beta
            betas.Add(new Color(beta.r, beta.g, beta.b));
            hits.Add(hit);

            var angleGlobal = Mathf.Rad2Deg * Mathf.Atan2(fSample.OUT_DIR.normalized.y, fSample.OUT_DIR.normalized.x);
            angleGlobal = (angleGlobal + 360) % 360;
            reflectionAngles.Add(angleGlobal); //we need to transform the range from -90-90 to 0-180 

            // obtain new direction
            dir = fSample.OUT_DIR;
            rayOrigin = hit.point + hit.normal * epsilon;
        }

        //If the path ended up in a light source we update the tree
        if (lightFound) {
            for (int i = 0; i < hits.Count; i++) {
                var hitI = hits[i];
                var betaI = betas[i];
                var angleI = reflectionAngles[i];

                // store it in the tree
                // _sdTree.

                var colorI = new Color(finalColor.r / betaI.r, finalColor.g / betaI.g, finalColor.b / betaI.b);
                _sdTree.RecordRadiance(hitI.point, angleI, colorI);
            }
        }

        return finalColor;
    }

    private void DrawRay(Vector3 dir, Vector3 rayOrigin, float length, Color color) {
        var r = Instantiate(rayPrefab, rayOrigin, Quaternion.identity);
        r.layer = LayerMask.NameToLayer("Just2D");
        r.GetComponent<RayRenderer>().direction = dir;
        r.GetComponent<RayRenderer>().length = length;
        r.GetComponent<LineRenderer>().material.color = color;
        _instantiatedRays.Add(r);
    }

    void Awake() {
        _sdTree = AdaptiveSDTree.Instance;
        _samplePoint = transform.GetChild(0).transform.position;
    }

    void Update() {

        var position = _samplePoint;
        Vector2 mousePos = cam2D.ScreenToWorldPoint(Input.mousePosition);

        var viewportPos = cam2D.ScreenToViewportPoint(Input.mousePosition);

        Vector2 dir = (mousePos - position).normalized;
        Debug.DrawRay(position, dir * focalLen, Color.blue);

        // DrawCameraFrame();

        // Cast Single Ray
        if (Input.GetKeyDown(KeyCode.R)) {
            ClearDrawnRays();
            CastRayFull(dir);
            LiveRefresh();
        }

        // Clear rays
        if (Input.GetKeyDown(KeyCode.C)) {
            ClearDrawnRays();
        }

        // SpamCast Ray
        if (Input.GetKeyDown(KeyCode.T)) {
            ClearDrawnRays();
            for (int i = 0; i < spamRate; i++) {
                CastRayFull(dir);
            }

            LiveRefresh();
        }
    }
}