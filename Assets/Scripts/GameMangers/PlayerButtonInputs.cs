using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerButtonInput : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    public float pullStrength = 10f;
    public float pullRadius = 2f;
    private bool isDragging = false;

    public BoxCollider2D boundaryCollider; // Assign in inspector

    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f; // seconds

    // --- Ship selection state ---
    private ShipController selectedShip;
    private List<ShipController> selectedShips = new List<ShipController>();

    // --- Selection box ---
    private SelectionBox selectionBox;
    [SerializeField] private Camera mainCamera;
    [SerializeField] public List<ShipController> allShips = new List<ShipController>();

    private bool isDraggingBox = false;
    private Vector2 dragStartScreenPos;
    private bool isMouseDown = false;
    private float dragThreshold = 10f; // pixels

    void Awake()
    {
        inputActions = new InputSystem_Actions();

        // Find or create SelectionBox
        selectionBox = FindFirstObjectByType<SelectionBox>();
        if (selectionBox == null)
        {
            GameObject selectionBoxGO = new GameObject("SelectionBox");
            selectionBox = selectionBoxGO.AddComponent<SelectionBox>();
        }
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
        // Mouse down: record start position, but don't start box yet
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragStartScreenPos = Mouse.current.position.ReadValue();
            isMouseDown = true;
            isDraggingBox = false;
        }

        // If mouse is held, check if moved enough to start box selection
        if (isMouseDown && Mouse.current.leftButton.isPressed)
        {
            Vector2 currentScreenPos = Mouse.current.position.ReadValue();
            float dragDist = (currentScreenPos - dragStartScreenPos).magnitude;

            if (!isDraggingBox && dragDist > dragThreshold)
            {
                isDraggingBox = true;
                selectionBox.BeginSelection(dragStartScreenPos);
            }

            if (isDraggingBox)
            {
                selectionBox.UpdateSelection(currentScreenPos);
            }
        }

        // Mouse up: finish box selection or treat as click
        if (isMouseDown && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (isDraggingBox)
            {
                isDraggingBox = false;
                selectionBox.EndSelection();

                Rect selectionRect = selectionBox.GetSelectionRect();
                SelectShipsInBox(selectionRect);
            }
            else
            {
                // Treat as click (single selection or double click)
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
                            AudioManager.Instance.PlayClickSound();
                            CameraMovement.Instance.ZoomAndFollow(hit.collider.transform);
                        }
                        else
                        {
                            AudioManager.Instance.PlayClickSound();
                            SelectObject(hit.collider.gameObject);
                        }
                        lastClickTime = Time.time;
                    }
                    else if (hit.collider.CompareTag("Ship"))
                    {
                        // Select ship on single left click
                        var ship = hit.collider.GetComponent<ShipController>();
                        if (ship != null)
                        {
                            SelectSingleShip(ship);
                        }
                    }
                }
                else
                {
                    // Deselect ship if clicking empty space
                    DeselectAllShips();
                }
            }
            isMouseDown = false;
        }

        // Right-click: return camera to original target or give ship(s) move order
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (selectedShips.Count > 0)
            {
                // Calculate center of selected ships
                Vector2 center = Vector2.zero;
                foreach (var ship in selectedShips)
                    center += (Vector2)ship.transform.position;
                center /= selectedShips.Count;

                Vector2 forward = (mousePosition - center).normalized;
                if (forward.sqrMagnitude < 0.01f)
                    forward = Vector2.up; // Default direction if too close

                var formationPositions = GetArrowFormationPositions(mousePosition, forward, selectedShips.Count, 2.5f);

                for (int i = 0; i < selectedShips.Count; i++)
                {
                    selectedShips[i].SetTargetPosition(formationPositions[i]);
                }
                AudioManager.Instance.PlayClickSound();
            }
            else
            {
                AudioManager.Instance.PlayUnclickSound();
                CameraMovement.Instance.ReturnToOriginalTarget();
                PlanetDataUI.Instance.Hide();
            }
        }
    }

    /// <summary>
    /// Returns a list of positions in an arrow (V) formation centered at target, facing forward.
    /// </summary>
    private List<Vector2> GetArrowFormationPositions(Vector2 target, Vector2 forward, int count, float spacing = 2.5f)
    {
        List<Vector2> positions = new List<Vector2>(count);
        Vector2 right = new Vector2(-forward.y, forward.x); // Perpendicular to forward

        int row = 0;
        int placed = 0;
        while (placed < count)
        {
            int shipsInRow = row == 0 ? 1 : 2 * row;
            for (int i = 0; i < shipsInRow && placed < count; i++)
            {
                float offset = (row == 0) ? 0 : (i - (shipsInRow - 1) / 2f) * spacing;
                Vector2 pos = target - forward * row * spacing + right * offset;
                positions.Add(pos);
                placed++;
            }
            row++;
        }
        return positions;
    }

    void SelectObject(GameObject obj)
    {
        Debug.Log($"Selected: {obj.name}");

        var planetComp = obj.GetComponent<PlanetDataComponent>();
        if (planetComp != null && planetComp.Data != null)
        {
            int empireId = planetComp.Data.ownerEmpireID.HasValue ? planetComp.Data.ownerEmpireID.Value : -1;
            string ownerName = (empireId >= 0 && empireId < GameManager.empireNames.Length) ? GameManager.empireNames[empireId] : "None";
            PlanetDataUI.Instance.ShowPlanetData(planetComp.Data, ownerName);
            return;
        }

        var starComp = obj.GetComponent<StarDataComponent>();
        if (starComp != null && starComp.Data != null)
        {
            PlanetDataUI.Instance.ShowStarData(starComp.Data);
            return;
        }

        PlanetDataUI.Instance.Hide();
    }

    // --- Ship selection helpers ---
    void SelectSingleShip(ShipController ship)
    {
        DeselectAllShips();
        selectedShip = ship;
        selectedShips.Clear();
        selectedShips.Add(ship);
        ship.Select();
        Debug.Log($"[DEBUG] Ship selected by click: {ship.name} (ID: {ship.Data?.id})");
    }

    void DeselectAllShips()
    {
        foreach (var ship in selectedShips)
        {
            ship.Deselect();
        }
        selectedShips.Clear();
        selectedShip = null;
    }

    void SelectShipsInBox(Rect screenRect)
    {
        DeselectAllShips();
        List<string> selectedNames = new List<string>();
        foreach (var ship in allShips)
        {
            // Convert ship world position to screen position
            Vector3 shipWorldPos = ship.transform.position;
            Vector2 shipScreenPos = mainCamera.WorldToScreenPoint(shipWorldPos);

            // Check if ship is within the selection box
            if (screenRect.Contains(shipScreenPos))
            {
                ship.Select();
                selectedShips.Add(ship);
                selectedNames.Add($"{ship.name} (ID: {ship.Data?.id})");
            }
        }
        if (selectedShips.Count > 0)
            selectedShip = selectedShips[0];

        Debug.Log($"[DEBUG] Ships selected by box: {string.Join(", ", selectedNames)}");
    }
}