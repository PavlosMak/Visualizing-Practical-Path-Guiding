using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PtSceneIntegrator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    Color[] EvaluatePoint(Vector2 point, int samples) {
        Color[] result = new Color[180];
        float invSamples = 1.0f / ((float) samples);
        for(int i = -90; i < 90; i++) {
            for(int j = 0; j < samples; j++) {
                result[i] = PtUtils.addColors(result[i], EvaluateDirection(point, i));
            }
            result[i] = PtUtils.multScalarColor(invSamples,  result[i]);
        }
        return result;
    }

    Color EvaluateDirection(Vector2 point, float theta) {
        return Color.black;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
