using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private float verticalStepSize = 0.1f;
    [SerializeField] private float horizontalStepSize = 0.1f;
    [SerializeField] private float zoomStep = 0.1f;
    [SerializeField] private float mouseZoomStep = 8f;
    [SerializeField] private float mousePanSensitivity = 0.0065f;

    // the camera to operate on
    private Camera _cam;

    // keyboard step
    private Vector3 _verticalStep;
    private Vector3 _horizontalStep;

    private Vector3 _lastPosition;

    // Start is called before the first frame update
    void Start() {
        _verticalStep = new Vector3(0, verticalStepSize, 0);
        _horizontalStep = new Vector3(horizontalStepSize, 0, 0);
        _cam = Camera.main;
    }

    void ZoomPanWasd() {
        Vector3 step = new Vector3();
        if (Input.GetKey(KeyCode.W)) {
            step = _verticalStep;
        }
        else if (Input.GetKey(KeyCode.S)) {
            step = -_verticalStep;
        }
        else if (Input.GetKey(KeyCode.D)) {
            step = _horizontalStep;
        }
        else if (Input.GetKey(KeyCode.A)) {
            step = -_horizontalStep;
        }
        else if (Input.GetKey(KeyCode.Q)) {
            _cam.orthographicSize = Mathf.Max(0.1f, _cam.orthographicSize - zoomStep);
        }
        else if (Input.GetKey(KeyCode.E)) {
            _cam.orthographicSize += zoomStep;
        }

        transform.position += step;
    }

    void ZoomPanMouse(int panButton) {
        // pan
        if (Input.GetMouseButtonDown(panButton)) {
            _lastPosition = Input.mousePosition;
        }


        if (Input.GetMouseButton(panButton)) {
            var delta = Input.mousePosition - _lastPosition;
            delta = -delta;

            var step = mousePanSensitivity * _cam.orthographicSize;
            transform.Translate(delta.x * step, delta.y * step, 0);
            _lastPosition = Input.mousePosition;
        }
        
        // zoom
        var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        _cam.orthographicSize -= mouseZoomStep * mouseScroll;
        
    }

    // Update is called once per frame
    void Update() {
        ZoomPanMouse(2);
        ZoomPanWasd();
    }
}