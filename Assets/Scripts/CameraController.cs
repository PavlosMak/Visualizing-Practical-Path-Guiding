using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField] private float verticalStepSize = 0.1f; 
    [SerializeField] private float horizontalStepSize = 0.1f;
    [SerializeField] private float zoomStep = 0.1f;

    private Vector3 verticalStep;
    private Vector3 horizontalStep;
    // Start is called before the first frame update
    void Start()
    {
        verticalStep = new Vector3(0,verticalStepSize,0);        
        horizontalStep = new Vector3(horizontalStepSize,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 step = new Vector3();
        if(Input.GetKey(KeyCode.W)) {
            step = verticalStep;
        } else if(Input.GetKey(KeyCode.S)) {
            step = -verticalStep;
        } else if(Input.GetKey(KeyCode.D)) {
            step = horizontalStep;
        } else if(Input.GetKey(KeyCode.A)) {
            step = -horizontalStep;
        } else if(Input.GetKey(KeyCode.Q)) {
            Camera.main.orthographicSize = Mathf.Max(0.1f, Camera.main.orthographicSize - zoomStep);
        } else if(Input.GetKey(KeyCode.E)) {
            Camera.main.orthographicSize += zoomStep;
        }
        transform.position += step;
    }
}
