using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableFurniture : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public string itemId;
    public string itemType;
    
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private bool isFlipped = false;
    
    // Reference to FirebaseController (set this when instantiating)
    public FirebaseController firebaseController;
    
    // Draggable area bounds (in local canvas coordinates)
    public RectTransform draggableArea;
    
    // Double-click detection
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;
    private bool isDragging = false;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        // Add CanvasGroup if not present (for dragging transparency)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Don't flip if we were dragging
        if (isDragging)
        {
            isDragging = false;
            return;
        }
        
        // Check for double-click
        float timeSinceLastClick = Time.time - lastClickTime;
        
        if (timeSinceLastClick <= doubleClickThreshold)
        {
            // Double-click detected - flip the furniture
            FlipFurniture();
        }
        
        lastClickTime = Time.time;
    }
    
    private void FlipFurniture()
    {
        isFlipped = !isFlipped;
        
        // Flip the sprite horizontally by inverting the X scale
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        
        // Save the flip state
        if (firebaseController != null)
        {
            firebaseController.SaveFurnitureFlip(itemId, isFlipped);
        }
    }
    
    public void SetFlipped(bool flipped)
    {
        isFlipped = flipped;
        
        // Apply the flip state
        if (isFlipped)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * -1; // Ensure it's negative (flipped)
            transform.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x); // Ensure it's positive (normal)
            transform.localScale = scale;
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        
        if (firebaseController != null)
            firebaseController.PlayPickUpItemSound();

        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        
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
        
        if (firebaseController != null)
            firebaseController.PlayPlaceItemSound();
        
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
        // Default positions based on type
        switch (itemType)
        {
            case "bed": return new Vector2(-150, 100);
            case "chair": return new Vector2(150, 100);
            case "desk": return new Vector2(-150, -100);
            case "lamp": return new Vector2(150, -100);
            case "bookshelf": return new Vector2(0, -150);
<<<<<<< HEAD
            case "walldeco": return new Vector2(-50, 150);
=======
>>>>>>> 73309d803293356213cb7f2ba93d39dea7a3cd4c
            default: return Vector2.zero;
        }
    }
}