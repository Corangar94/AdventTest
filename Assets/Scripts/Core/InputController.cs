using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Handles cross-platform input for both mobile and desktop platforms
/// </summary>
public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }
    
    // Event triggered when a shape is double-clicked/tapped
    public event Action<GameObject> OnDoubleClick;
    
    // New events for gesture controls
    public event Action<GameObject, float> OnRotate;
    public event Action<GameObject, float> OnScale;
    
    // Timing for double-click/tap detection
    private float lastClickTime;
    private float doubleClickTimeThreshold = 0.3f;
    private GameObject lastClickedObject;
    
    // Drag and rotation variables
    private bool isDragging = false;
    private GameObject draggedObject = null;
    private Vector2 lastMousePosition;
    private float rotationSensitivity = 0.5f;
    
    // Pinch-to-zoom variables
    private float initialPinchDistance;
    private float scaleSensitivity = 0.01f;
    private bool isPinching = false;
    private GameObject pinnedObject = null;
    
    // Mouse wheel scaling
    private float mouseWheelSensitivity = 0.1f;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Update()
    {
        // Check for desktop input (mouse)
        CheckDesktopInput();
        
        // Check for mobile input (touch)
        CheckMobileInput();
    }
    
    private void CheckDesktopInput()
    {
        // Check for mouse clicks
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the camera to the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            
            // If we hit something
            if (hit.collider != null)
            {
                // Get the game object we hit
                GameObject hitObject = hit.collider.gameObject;
                
                // Start drag for rotation
                isDragging = true;
                draggedObject = hitObject;
                lastMousePosition = Input.mousePosition;
                
                // Check if it's the same object as the last click for double-click
                if (hitObject == lastClickedObject && Time.time - lastClickTime < doubleClickTimeThreshold)
                {
                    // Double click detected, trigger the event
                    OnDoubleClick?.Invoke(hitObject);
                    
                    // Reset the last clicked object to prevent triple-click detection
                    lastClickedObject = null;
                }
                else
                {
                    // Store the object and time for double-click detection
                    lastClickedObject = hitObject;
                    lastClickTime = Time.time;
                }
            }
        }
        
        // Handle drag rotation
        if (isDragging && draggedObject != null)
        {
            if (Input.GetMouseButton(0))
            {
                // Calculate drag delta for rotation
                Vector2 currentPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                Vector2 delta = currentPosition - lastMousePosition;
                float rotationAmount = delta.x * rotationSensitivity;
                
                // Trigger rotation event
                if (Mathf.Abs(rotationAmount) > 0.1f)
                {
                    OnRotate?.Invoke(draggedObject, rotationAmount);
                }
                
                lastMousePosition = currentPosition;
            }
            else
            {
                // Stop dragging if mouse button is released
                isDragging = false;
                draggedObject = null;
            }
        }
        
        // Handle mouse wheel for scaling
        if (lastClickedObject != null)
        {
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                float scaleFactor = 1f + (scrollDelta * mouseWheelSensitivity * 10f);
                OnScale?.Invoke(lastClickedObject, scaleFactor);
            }
        }
    }
    
    private void CheckMobileInput()
    {
        // Only check for touch input on mobile platforms
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            
            // Check for touch begin phase
            if (touch.phase == TouchPhase.Began)
            {
                // Cast a ray from the camera to the touch position
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                
                // If we hit something
                if (hit.collider != null)
                {
                    // Get the game object we hit
                    GameObject hitObject = hit.collider.gameObject;
                    
                    // Start drag for rotation
                    isDragging = true;
                    draggedObject = hitObject;
                    lastMousePosition = touch.position;
                    
                    // Check if it's the same object as the last touch for double-tap
                    if (hitObject == lastClickedObject && Time.time - lastClickTime < doubleClickTimeThreshold)
                    {
                        // Double tap detected, trigger the event
                        OnDoubleClick?.Invoke(hitObject);
                        
                        // Reset the last clicked object to prevent triple-tap detection
                        lastClickedObject = null;
                    }
                    else
                    {
                        // Store the object and time for double-tap detection
                        lastClickedObject = hitObject;
                        lastClickTime = Time.time;
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved && isDragging && draggedObject != null)
            {
                // Calculate drag delta for rotation
                Vector2 delta = touch.position - lastMousePosition;
                float rotationAmount = delta.x * rotationSensitivity;
                
                // Trigger rotation event
                if (Mathf.Abs(rotationAmount) > 0.1f)
                {
                    OnRotate?.Invoke(draggedObject, rotationAmount);
                }
                
                lastMousePosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                // Stop dragging
                isDragging = false;
                draggedObject = null;
            }
        }
        // Pinch to zoom (two fingers)
        else if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            
            // Calculate the center point between the two touches
            Vector2 touchCenter = (touch0.position + touch1.position) / 2;
            
            // Cast a ray to find what object is at the center of the pinch
            if (!isPinching)
            {
                Ray ray = Camera.main.ScreenPointToRay(touchCenter);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                
                if (hit.collider != null)
                {
                    isPinching = true;
                    pinnedObject = hit.collider.gameObject;
                    initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                }
            }
            
            // Handle pinch scaling
            if (isPinching && pinnedObject != null)
            {
                // Calculate current distance and scale factor
                float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                float pinchDelta = currentPinchDistance - initialPinchDistance;
                
                if (Mathf.Abs(pinchDelta) > 5f) // Threshold to prevent minor movements
                {
                    float scaleFactor = 1f + (pinchDelta * scaleSensitivity);
                    OnScale?.Invoke(pinnedObject, scaleFactor);
                    initialPinchDistance = currentPinchDistance; // Update for next frame
                }
                
                // End pinch if any finger is removed
                if (touch0.phase == TouchPhase.Ended || touch0.phase == TouchPhase.Canceled ||
                    touch1.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Canceled)
                {
                    isPinching = false;
                    pinnedObject = null;
                }
            }
        }
        else
        {
            // Reset pinch state if no fingers are touching
            isPinching = false;
            pinnedObject = null;
        }
    }
} 