using UnityEngine;

public struct BRDFSample {
    public BRDFSample(Color color, Vector2 outDir, float pdf, float angle) {
        COLOR = color;
        OUT_DIR = outDir;
        PDF = pdf;
        ANGLE = angle;
    }


    public Color COLOR { get; }
    public Vector2 OUT_DIR { get; }
    public float PDF { get; }

    public float ANGLE { get; }
}

public class PtMaterial : MonoBehaviour {
    [SerializeField] private Color albedo = Color.white;
    [SerializeField] private Color emision = Color.black;
    private const float InvPi = 1.0f / Mathf.PI;

    private const float Inv2Pi = 1.0f / (2.0f * Mathf.PI);

    // Start is called before the first frame update
    void Start() {
        Material mat = this.GetComponent<Material>();
        if (mat != null) {
            albedo = mat.color;
        }
    }

    public Color GetEmission() {
        return emision;
    }

    public BRDFSample SampleF(Vector2 wo, Vector2 normal) {
        float angle = Random.Range(-90.0f, 90.0f);
        var color = PtUtils.multScalarColor(InvPi, albedo);
        Vector2 outDir = Quaternion.Euler(0, 0, angle) * normal;
        float pdf = 1.0f / Mathf.PI;
        return new BRDFSample(color, outDir, pdf, angle);
    }
}