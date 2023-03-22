using UnityEngine;

public class CamController3D : MonoBehaviour {
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private Transform target;

    //Camera fields
    [SerializeField] private float smoothness = 0.5f;
    private Vector3 _cameraOffset;

    [SerializeField] public float rotationSpeedMouse = 5;
    [SerializeField] public float zoomSpeedMouse = 10;

    [SerializeField] private float zoomAmount = 0;
    [SerializeField] private float maxToClamp = 10;

    [SerializeField] Camera cam;

    private bool _showWorld = true;

    public void ToggleShowWorld() {
        _showWorld = !_showWorld;
        cam.cullingMask ^= 1 << LayerMask.NameToLayer("Just2D");
    }

    void Start() {
        _cameraOffset = transform.position - target.position;
        transform.LookAt(target);
    }

    void LateUpdate() {
        // Rotating camera with RMB dragging on PC.
        if (enableRotation && (Input.GetMouseButton(1))) {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeedMouse;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeedMouse;

            Quaternion camAng = Quaternion.Euler(rotY, rotX, 0);

            Vector3 newPos = target.position + _cameraOffset;
            _cameraOffset = camAng * _cameraOffset;

            transform.position = Vector3.Slerp(transform.position, newPos, smoothness);
            transform.LookAt(target);
        }

        else {
            zoomAmount += Input.GetAxis("Mouse ScrollWheel");
            zoomAmount = Mathf.Clamp(zoomAmount, -maxToClamp, maxToClamp);

            var translate = Mathf.Min(Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")),
                maxToClamp - Mathf.Abs(zoomAmount));
            transform.Translate(0, 0, translate * zoomSpeedMouse * Mathf.Sign(Input.GetAxis("Mouse ScrollWheel")));

            _cameraOffset = transform.position - target.position;
        }
    }
}