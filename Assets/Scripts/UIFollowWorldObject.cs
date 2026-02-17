using UnityEngine;

public class UIFollowWorldObject : MonoBehaviour
{
    public Transform target; // The GameObject to follow
    public Canvas canvas;    // Reference to the overlay canvas

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (target == null || canvas == null) return;

        // Convert world position to screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);

        // Set UI position (for overlay canvas, this is correct)
        rectTransform.position = screenPos;
    }
}