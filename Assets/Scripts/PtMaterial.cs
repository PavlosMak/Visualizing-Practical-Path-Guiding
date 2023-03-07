using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BRDFSample {

    public BRDFSample(Color color, Vector2 outDir, float pdf) {
        COLOR = color;
        OUT_DIR = outDir;
        PDF = pdf;
    }


    public Color COLOR {get;}
    public Vector2 OUT_DIR {get;}
    public float PDF {get;}
}

public class PtMaterial : MonoBehaviour
{
    [SerializeField] private Color albedo = Color.white;
    [SerializeField] private Color emision = Color.black;
    private const float InvPi = 1.0f / Mathf.PI;
    private const float Inv2Pi = 1.0f / (2.0f*Mathf.PI);
    // Start is called before the first frame update
    void Start()
    {
        Material mat = this.GetComponent<Material>();
        if(mat != null) {
            albedo = mat.color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Color GetEmission() {
        return emision;
    }

    private Vector2 SampleHemisphere(Vector2 wo, Vector2 normal) {
        return Quaternion.Euler(0, 0, Random.Range(-90.0f, 90.0f)) * normal;
    }

    public BRDFSample SampleF(Vector2 wo, Vector2 normal) {
        Color color = PtUtils.multScalarColor(InvPi,albedo);
        Vector2 outDir = SampleHemisphere(wo, normal);
        float pdf = 1.0f / (2.0f*Mathf.PI);
        return new BRDFSample(color,outDir,pdf);
    }

}
