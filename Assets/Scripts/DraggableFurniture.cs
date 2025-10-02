using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableFurniture : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string itemId;
    public string itemType;
    
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    
    // Reference to FirebaseController (set this when instantiating)
    public FirebaseController firebaseController;
    
    // Draggable area bounds (in local canvas coordinates)
    public RectTransform draggableArea;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        // Add CanvasGroup if not present (for dragging transparency)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f; // Make slightly transparent while dragging
        canvasGroup.blocksRaycasts = false;
        
        // Move to front by setting as last sibling in hierarchy
        transform.SetAsLastSibling();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Move with mouse/touch
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        
        // Constrain to draggable area if set
        if (draggableArea != null)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            
            // Get bounds of draggable area
            Rect bounds = draggableArea.rect;
            
            // Clamp position
            pos.x = Mathf.Clamp(pos.x, bounds.xMin, bounds.xMax);
            pos.y = Mathf.Clamp(pos.y, bounds.yMin, bounds.yMax);
            
            rectTransform.anchoredPosition = pos;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Save position to Firebase
        SavePosition();
    }
    
    private void SavePosition()
    {
        if (firebaseController != null)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            firebaseController.SaveFurniturePosition(itemId, itemType, pos);
        }
    }
    
    public void SetPosition(Vector2 position)
    {
        rectTransform.anchoredPosition = position;
    }
    
    public Vector2 GetDefaultPositionForType()
    {
        // Default positions based on type (you can adjust these)
        switch (itemType)
        {
            case "bed": return new Vector2(-150, 100);
            case "chair": return new Vector2(150, 100);
            case "desk": return new Vector2(-150, -100);
            case "lamp": return new Vector2(150, -100);
            default: return Vector2.zero;
        }
    }
}