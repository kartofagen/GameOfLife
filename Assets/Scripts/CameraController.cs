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
    [SerializeField] private Vector2 boundsMin = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 boundsMax = new Vector2(10f, 10f);

    private Camera mainCamera;
    private Vector3 targetPosition;
    private float targetZoom;
    private Vector3 lastPanPosition;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        targetPosition = transform.position;
        targetZoom = mainCamera.orthographicSize;
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
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        if (Mathf.Abs(mainCamera.orthographicSize - targetZoom) > 0.01f)
        {
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize, 
                targetZoom, 
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
            targetPosition += mainCamera.orthographicSize * panSpeed * keyboardInput;
            
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, boundsMin.x, boundsMax.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, boundsMin.y, boundsMax.y);
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    private void ApplySmoothMovement()
    {
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(
                transform.position, 
                targetPosition, 
                Time.deltaTime * panLerpSpeed
            );
        }
    }
}
