using UnityEngine;
using UnityEngine.UI;

public class ShapeSelectionUI : MonoBehaviour
{
    [SerializeField] private Button hexagonButton;
    [SerializeField] private Button squareButton;
    [SerializeField] private Button triangleButton;
    
    // Reference to the shape manager
    private ShapeManager shapeManager;
    
    private void Awake()
    {
        // Find the shape manager
        shapeManager = FindFirstObjectByType<ShapeManager>();
        
        if (shapeManager == null)
        {
            Debug.LogWarning("ShapeManager not found. Shape selection will not work.");
        }
    }
    
    private void Start()
    {
        // Add click listeners to the buttons
        if (hexagonButton != null)
        {
            hexagonButton.onClick.AddListener(() => {
                if (shapeManager != null)
                {
                    shapeManager.SwitchShape(ShapeManager.ShapeType.Hexagon);
                    
                    // Play sound if available
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayShapeChangeSound();
                    }
                }
            });
        }
        
        if (squareButton != null)
        {
            squareButton.onClick.AddListener(() => {
                if (shapeManager != null)
                {
                    shapeManager.SwitchShape(ShapeManager.ShapeType.Square);
                    
                    // Play sound if available
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayShapeChangeSound();
                    }
                }
            });
        }
        
        if (triangleButton != null)
        {
            triangleButton.onClick.AddListener(() => {
                if (shapeManager != null)
                {
                    shapeManager.SwitchShape(ShapeManager.ShapeType.Triangle);
                    
                    // Play sound if available
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayShapeChangeSound();
                    }
                }
            });
        }
    }
    
    private void OnDestroy()
    {
        // Remove listeners when the object is destroyed
        if (hexagonButton != null)
            hexagonButton.onClick.RemoveAllListeners();
            
        if (squareButton != null)
            squareButton.onClick.RemoveAllListeners();
            
        if (triangleButton != null)
            triangleButton.onClick.RemoveAllListeners();
    }
} 