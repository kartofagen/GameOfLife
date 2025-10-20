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

    private InputAction toggleSimulationAction;
    private InputAction rotateStructureAction;
    private InputAction mouseClickAction;
    private InputAction mouseRightClickAction;
    private InputAction mousePositionAction;
    private InputAction mouseScrollAction;

    private Camera mainCamera;
    private Vector2 lastMousePosition;

    private void Awake()
    {
        mainCamera = Camera.main;
        var gameplayMap = inputActions.FindActionMap("Gameplay");
        
        toggleSimulationAction = gameplayMap.FindAction("ToggleSimulation");
        rotateStructureAction = gameplayMap.FindAction("RotateStructure");
        mouseClickAction = gameplayMap.FindAction("MouseClick");
        mouseRightClickAction = gameplayMap.FindAction("MouseRightClick");
        mousePositionAction = gameplayMap.FindAction("MousePosition");
        mouseScrollAction = gameplayMap.FindAction("MouseScroll");
    }

    private void OnEnable()
    {
        toggleSimulationAction.Enable();
        rotateStructureAction.Enable();
        
        mouseClickAction.Enable();
        mouseRightClickAction.Enable();
        mousePositionAction.Enable();
        mouseScrollAction.Enable();

        toggleSimulationAction.performed += OnToggleSimulation;
        rotateStructureAction.performed += OnRotateStructure;
        
        mouseClickAction.performed += OnMouseClick;
        mouseRightClickAction.performed += OnMouseRightClick;
    }

    private void OnDisable()
    {
        toggleSimulationAction.performed -= OnToggleSimulation;
        rotateStructureAction.performed -= OnRotateStructure;
        mouseClickAction.performed -= OnMouseClick;
        mouseRightClickAction.performed -= OnMouseRightClick;

        toggleSimulationAction.Disable();
        rotateStructureAction.Disable();
        
        mouseClickAction.Disable();
        mouseRightClickAction.Disable();
        mousePositionAction.Disable();
        mouseScrollAction.Disable();
    }

    private void Update()
    {
        Vector2 mousePosition = mousePositionAction.ReadValue<Vector2>();
        if (mousePosition != lastMousePosition)
        {
            onMouseMove?.Invoke(mousePosition);
            lastMousePosition = mousePosition;
        }

        if (mouseClickAction.ReadValue<float>() > 0.5f)
        {
            onMouseClick?.Invoke(mousePosition);
        }
        else if (mouseRightClickAction.ReadValue<float>() > 0.5f)
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
        Vector2 mouseScreenPos = mousePositionAction.ReadValue<Vector2>();
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
}