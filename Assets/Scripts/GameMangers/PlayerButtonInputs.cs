using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerButtonInput : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    public float pullStrength = 10f;
    public float pullRadius = 2f;
    private bool isDragging = false;

    public BoxCollider2D boundaryCollider; // Assign in inspector

    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f; // seconds

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        // Double-click detection for left mouse button
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Planet") || hit.collider.CompareTag("Star"))
                {
                    float timeSinceLastClick = Time.time - lastClickTime;
                    if (timeSinceLastClick < doubleClickThreshold)
                    {
                        // Double-click detected
                        CameraMovement.Instance.ZoomAndFollow(hit.collider.transform);
                    }
                    else
                    {
                        SelectObject(hit.collider.gameObject);
                    }
                    lastClickTime = Time.time;
                }
            }
        }

        // Right-click: return camera to original target
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CameraMovement.Instance.ReturnToOriginalTarget();
        }
    }

    void SelectObject(GameObject obj)
    {
        Debug.Log($"Selected: {obj.name}");
        // Example: highlight, show UI, etc.
    }

    void OnDrawGizmos()
    {

    }
}