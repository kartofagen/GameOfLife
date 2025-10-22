using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float zoomLerpSpeed = 5f;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 0.5f;
    [SerializeField] private float panLerpSpeed = 8f;

    [Header("Bounds Settings")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 boundsMin;
    [SerializeField] private Vector2 boundsMax;

    private Camera _mainCamera;
    private Vector3 _targetPosition;
    private float _targetZoom;
    private Vector3 _lastPanPosition;

    private void Start()
    {
        _mainCamera = Camera.main;
        _targetPosition = transform.position;
        _targetZoom = _mainCamera.orthographicSize;
    }

    private void Update()
    {
        HandleZoom();
        HandleKeyboardPan();
        ApplySmoothMovement();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (scroll != 0)
        {
            _targetZoom -= scroll * zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
        }

        if (Mathf.Abs(_mainCamera.orthographicSize - _targetZoom) > 0.01f)
        {
            _mainCamera.orthographicSize = Mathf.Lerp(
                _mainCamera.orthographicSize, 
                _targetZoom, 
                Time.deltaTime * zoomLerpSpeed
            );
        }
    }

    private void HandleKeyboardPan()
    {
        Vector3 keyboardInput = new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"),
            0
        );

        if (keyboardInput.magnitude > 0)
        {
            _targetPosition += _mainCamera.orthographicSize * panSpeed * keyboardInput;
            
            if (useBounds)
            {
                _targetPosition.x = Mathf.Clamp(_targetPosition.x, boundsMin.x, boundsMax.x);
                _targetPosition.y = Mathf.Clamp(_targetPosition.y, boundsMin.y, boundsMax.y);
            }
        }
    }

    private void ApplySmoothMovement()
    {
        if (Vector3.Distance(transform.position, _targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(
                transform.position, 
                _targetPosition, 
                Time.deltaTime * panLerpSpeed
            );
        }
    }
}
