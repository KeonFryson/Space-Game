using UnityEngine;
using UnityEngine.UI;

public class SelectionBox : MonoBehaviour
{
    private RectTransform boxRect;
    private Vector2 startScreenPos;
    private Vector2 endScreenPos;
    private bool isSelecting = false;

    void Awake()
    {
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("SelectionBoxCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            // Optional: DontDestroyOnLoad(canvasGO);
        }

        // Create selection box image
        GameObject boxGO = new GameObject("SelectionBoxImage");
        boxGO.transform.SetParent(canvas.transform, false);
        boxGO.transform.SetAsLastSibling(); // Ensure it's on top
        boxGO.layer = LayerMask.NameToLayer("Ignore Raycast");
        Image boxImage = boxGO.AddComponent<Image>();
        boxImage.color = new Color(0f, 0.5f, 1f, 0.2f); // Semi-transparent blue

        boxRect = boxGO.GetComponent<RectTransform>();
        boxRect.anchorMin = Vector2.zero;
        boxRect.anchorMax = Vector2.zero;
        boxRect.pivot = Vector2.zero;
        boxRect.sizeDelta = Vector2.zero;
        boxRect.gameObject.SetActive(false);
    }

    public void BeginSelection(Vector2 screenPos)
    {
        isSelecting = true;
        startScreenPos = screenPos;
        boxRect.gameObject.SetActive(true);
        boxRect.anchoredPosition = screenPos;
        boxRect.sizeDelta = Vector2.zero;
    }

    public void UpdateSelection(Vector2 screenPos)
    {
        if (!isSelecting) return;
        endScreenPos = screenPos;
        Vector2 min = Vector2.Min(startScreenPos, endScreenPos);
        Vector2 max = Vector2.Max(startScreenPos, endScreenPos);

        boxRect.anchoredPosition = min;
        boxRect.sizeDelta = max - min;
    }

    public void EndSelection()
    {
        isSelecting = false;
        boxRect.gameObject.SetActive(false);
    }

    public Rect GetSelectionRect()
    {
        Vector2 min = Vector2.Min(startScreenPos, endScreenPos);
        Vector2 max = Vector2.Max(startScreenPos, endScreenPos);
        return new Rect(min, max - min);
    }
}