using UnityEngine;

public class ShipController : MonoBehaviour
{
    public ShipData Data { get; set; }
    private bool isSelected;
    private Vector2 targetPosition;

    [SerializeField] private float maxMoveSpeed = 50f;
    [SerializeField] private float acceleration = 100f; // Units per second squared
    private float currentSpeed = 0f;

    void Start()
    {
        targetPosition = transform.position;
        currentSpeed = 0f;
    }

    void Update()
    {
        Vector2 currentPosition = transform.position;
        if (currentPosition != targetPosition)
        {
            // Accelerate up to maxMoveSpeed
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxMoveSpeed);

            // Move towards target
            float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
            float moveStep = currentSpeed * Time.deltaTime;

            if (moveStep >= distanceToTarget)
            {
                // Arrived at target
                transform.position = targetPosition;
                currentSpeed = 0f; // Reset speed for next move
            }
            else
            {
                Vector2 direction = (targetPosition - currentPosition).normalized;
                transform.position = currentPosition + direction * moveStep;
            }
        }
        else
        {
            currentSpeed = 0f; // Not moving, reset speed
        }
    }

    public void SetTargetPosition(Vector2 pos)
    {
        targetPosition = pos;
        currentSpeed = 0f; // Reset speed when new order is given
    }

    public void Select()
    {
        isSelected = true;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.yellow;
    }

    public void Deselect()
    {
        isSelected = false;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && Data != null)
            sr.color = Data.ownerEmpireId == 0 ? Color.cyan : Color.gray;
    }
}