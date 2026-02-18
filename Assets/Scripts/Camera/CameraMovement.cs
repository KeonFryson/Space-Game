using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement Instance { get; private set; }

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachineConfiner2D confiner;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float fastMoveSpeed = 40f;
    [SerializeField] private float edgeScrollSpeed = 15f;
    [SerializeField] private float dragSpeed = 1f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;

    [Header("Edge Scrolling")]
    [SerializeField] private bool enableEdgeScrolling = true;
    [SerializeField] private float edgeThreshold = 20f;

    private InputSystem_Actions inputActions;
    private bool isDragging;
    private Vector3 lastMousePosition;
    private Camera cam;
    [SerializeField] private Transform cameraTransform;
    private Collider2D boundingShape;

    public bool isActive = true;

    // Store original target and zoom
    private Transform originalTarget;
    private float originalZoom;

    bool isFollowingTarget = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        inputActions = new InputSystem_Actions();
        cam = Camera.main;

        if (virtualCamera == null)
        {
            virtualCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        if (virtualCamera != null && confiner == null)
        {
            confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
        }

        if (confiner != null)
        {
            boundingShape = confiner.BoundingShape2D;
        }

        // Use the parent transform as the camera rig if available
        cameraTransform = transform.parent != null ? transform.parent : transform;

        // Set the virtual camera to follow the parent (moving object)
        if (virtualCamera != null)
        {
            virtualCamera.Follow = cameraTransform;
        }

        // Store original target and zoom
        originalTarget = cameraTransform;
        if (virtualCamera != null)
        {
            originalZoom = virtualCamera.Lens.OrthographicSize;
        }
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        if (!isActive)
            return;

        if (!isFollowingTarget)
        {
            HandleKeyboardMovement();
            HandleEdgeScrolling();
            HandleMouseDrag();
            
        }

        HandleZoom();

    }

    private void CenterCameraTransform()
    {
        if (cam == null)
            return;

        Vector3 camPos = cam.transform.position;
        cameraTransform.position = new Vector3(camPos.x, camPos.y, cameraTransform.position.z);
    }

    private void HandleKeyboardMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            vertical = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            vertical = -1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            horizontal = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            horizontal = 1f;

        Vector3 moveDirection = new Vector3(horizontal, vertical, 0f).normalized;
        float currentSpeed = Keyboard.current.leftShiftKey.isPressed ? fastMoveSpeed : moveSpeed;

        // Scale speed based on zoom (orthographic size)
        float zoomFactor = virtualCamera != null ? virtualCamera.Lens.OrthographicSize / minZoom : 1f;
        currentSpeed *= zoomFactor;

        Vector3 newPosition = cameraTransform.position + moveDirection * currentSpeed * Time.unscaledDeltaTime;
        cameraTransform.position = ClampPositionToBounds(newPosition);
    }

    private void HandleEdgeScrolling()
    {
        if (!enableEdgeScrolling)
            return;

        Vector3 edgeMove = Vector3.zero;
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (mousePosition.x < edgeThreshold)
            edgeMove.x = -1f;
        else if (mousePosition.x > Screen.width - edgeThreshold)
            edgeMove.x = 1f;

        if (mousePosition.y < edgeThreshold)
            edgeMove.y = -1f;
        else if (mousePosition.y > Screen.height - edgeThreshold)
            edgeMove.y = 1f;

        if (edgeMove != Vector3.zero)
        {
            // Scale edge scroll speed based on zoom (orthographic size)
            float zoomFactor = virtualCamera != null ? virtualCamera.Lens.OrthographicSize / minZoom : 1f;
            float scaledEdgeScrollSpeed = edgeScrollSpeed * zoomFactor;

            Vector3 newPosition = cameraTransform.position + edgeMove * scaledEdgeScrollSpeed * Time.unscaledDeltaTime;
            cameraTransform.position = ClampPositionToBounds(newPosition);
        }
    }

    private void HandleMouseDrag()
    {
        if (cam == null)
            return;

        if (Mouse.current.middleButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMousePosition = Mouse.current.position.ReadValue();
        }

        if ((Mouse.current.middleButton.isPressed || Mouse.current.rightButton.isPressed) && isDragging)
        {
            Vector3 currentMousePos = Mouse.current.position.ReadValue();
            Vector3 mouseDelta = currentMousePos - lastMousePosition;

            float worldDeltaX = -mouseDelta.x * dragSpeed * virtualCamera.Lens.OrthographicSize / Screen.height;
            float worldDeltaY = -mouseDelta.y * dragSpeed * virtualCamera.Lens.OrthographicSize / Screen.height;

            Vector3 newPosition = cameraTransform.position + new Vector3(worldDeltaX, worldDeltaY, 0f);
            cameraTransform.position = ClampPositionToBounds(newPosition);

            lastMousePosition = currentMousePos;
        }

        if (Mouse.current.middleButton.wasReleasedThisFrame || Mouse.current.rightButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private void HandleZoom()
    {
        if (virtualCamera == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newSize = virtualCamera.Lens.OrthographicSize - scroll * zoomSpeed * Time.unscaledDeltaTime;
            virtualCamera.Lens.OrthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
           
        }
    }

    private Vector3 ClampPositionToBounds(Vector3 position)
    {
        if (boundingShape == null)
            return position;

        Vector2 clampedPosition2D = boundingShape.ClosestPoint(position);
        return new Vector3(clampedPosition2D.x, clampedPosition2D.y, position.z);
    }

    public void ZoomAndFollow(Transform target)
    {
        if (virtualCamera == null)
            return;

        isFollowingTarget = true;

        virtualCamera.Follow = target;
        float targetZoom = minZoom;
        StopAllCoroutines();
        StartCoroutine(SmoothZoom(targetZoom));
    }

    public void ReturnToOriginalTarget()
    {
        if (virtualCamera == null || originalTarget == null)
            return;

        isFollowingTarget = false;

        virtualCamera.Follow = originalTarget;
        StopAllCoroutines();
        //StartCoroutine(SmoothZoom(originalZoom));
    }

    private System.Collections.IEnumerator SmoothZoom(float targetZoom)
    {
        float duration = 0.5f;
        float startZoom = virtualCamera.Lens.OrthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startZoom, targetZoom, elapsed / duration);
            yield return null;
        }
        virtualCamera.Lens.OrthographicSize = targetZoom;
    }
}