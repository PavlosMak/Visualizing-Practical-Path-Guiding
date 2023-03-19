using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RadianceVolume : MonoBehaviour
{

    [SerializeField] Texture texture;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       Handles.DrawTexture3DVolume(texture, 1, 1.68f, FilterMode.Point, false, null);
    }
}
