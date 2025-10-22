using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class InputActionsHandler : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Events")]
    public UnityEvent onToggleSimulation;
    public UnityEvent onRotateStructure;
    public UnityEvent<Vector2> onMouseMove;
    public UnityEvent<Vector2> onMouseClick;
    public UnityEvent<Vector2> onMouseRightClick;

    private InputAction _toggleSimulationAction;
    private InputAction _rotateStructureAction;
    private InputAction _mouseClickAction;
    private InputAction _mouseRightClickAction;
    private InputAction _mousePositionAction;
    private InputAction _mouseScrollAction;

    private Camera _mainCamera;
    private Vector2 _lastMousePosition;

    private void Awake()
    {
        _mainCamera = Camera.main;
        var gameplayMap = inputActions.FindActionMap("Gameplay");
        
        _toggleSimulationAction = gameplayMap.FindAction("ToggleSimulation");
        _rotateStructureAction = gameplayMap.FindAction("RotateStructure");
        _mouseClickAction = gameplayMap.FindAction("MouseClick");
        _mouseRightClickAction = gameplayMap.FindAction("MouseRightClick");
        _mousePositionAction = gameplayMap.FindAction("MousePosition");
        _mouseScrollAction = gameplayMap.FindAction("MouseScroll");
    }

    private void OnEnable()
    {
        _toggleSimulationAction.Enable();
        _rotateStructureAction.Enable();
        
        _mouseClickAction.Enable();
        _mouseRightClickAction.Enable();
        _mousePositionAction.Enable();
        _mouseScrollAction.Enable();

        _toggleSimulationAction.performed += OnToggleSimulation;
        _rotateStructureAction.performed += OnRotateStructure;
        
        _mouseClickAction.performed += OnMouseClick;
        _mouseRightClickAction.performed += OnMouseRightClick;
    }

    private void OnDisable()
    {
        _toggleSimulationAction.performed -= OnToggleSimulation;
        _rotateStructureAction.performed -= OnRotateStructure;
        _mouseClickAction.performed -= OnMouseClick;
        _mouseRightClickAction.performed -= OnMouseRightClick;

        _toggleSimulationAction.Disable();
        _rotateStructureAction.Disable();
        
        _mouseClickAction.Disable();
        _mouseRightClickAction.Disable();
        _mousePositionAction.Disable();
        _mouseScrollAction.Disable();
    }

    private void Update()
    {
        Vector2 mousePosition = _mousePositionAction.ReadValue<Vector2>();
        if (mousePosition != _lastMousePosition)
        {
            onMouseMove?.Invoke(mousePosition);
            _lastMousePosition = mousePosition;
        }

        if (_mouseClickAction.ReadValue<float>() > 0.5f)
        {
            onMouseClick?.Invoke(mousePosition);
        }
        else if (_mouseRightClickAction.ReadValue<float>() > 0.5f)
        {
            onMouseRightClick?.Invoke(mousePosition);
        }
    }

    private void OnToggleSimulation(InputAction.CallbackContext context)
    {
        onToggleSimulation?.Invoke();
    }

    private void OnRotateStructure(InputAction.CallbackContext context)
    {
        onRotateStructure?.Invoke();
    }

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        
    }

    private void OnMouseRightClick(InputAction.CallbackContext context)
    {
        
    }

    public Vector2 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPos = _mousePositionAction.ReadValue<Vector2>();
        return _mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
}