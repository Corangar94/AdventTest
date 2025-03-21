using UnityEngine;
using System.Collections;

public class ShapeManager : MonoBehaviour
{
    [SerializeField] private Transform shapeContainer;
    
    // References to different shape prefabs
    [SerializeField] private GameObject hexagonPrefab;
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private GameObject trianglePrefab;
    
    [SerializeField] private bool useParticleEffects = true;
    [SerializeField] private bool useRainbowEffect = true;
    [SerializeField] private float morphTransitionTime = 0.5f;
    
    private Shape currentShape;
    private ShapeType currentShapeType = ShapeType.Hexagon;
    private bool isTransitioning = false;
    
    public enum ShapeType
    {
        Hexagon,
        Square,
        Triangle
    }
    
    private void Awake()
    {
        // Create a shape container if not assigned
        if (shapeContainer == null)
        {
            GameObject container = new GameObject("ShapeContainer");
            shapeContainer = container.transform;
            shapeContainer.SetParent(transform);
            shapeContainer.localPosition = Vector3.zero;
        }
    }
    
    private void Start()
    {
        // Create initial shape (hexagon)
        SwitchShape(ShapeType.Hexagon);
        
        // Ensure we have a particle effect manager
        if (useParticleEffects && ParticleEffectManager.Instance == null)
        {
            Debug.LogWarning("ParticleEffectManager not found but useParticleEffects is enabled. Creating one.");
            // Force creation of the ParticleEffectManager instance
            ParticleEffectManager effectManager = ParticleEffectManager.Instance;
        }
    }
    
    public void SwitchShape(ShapeType shapeType)
    {
        Debug.Log("SwitchShape called for shape type: " + shapeType);
        
        // Only switch if it's a different shape and not already transitioning
        if ((currentShapeType == shapeType && currentShape != null) || isTransitioning)
        {
            Debug.Log("Skipping shape switch - same shape or already transitioning");
            return;
        }
            
        if (useParticleEffects && currentShape != null)
        {
            // Start the transition effect
            Debug.Log("Starting particle transition to: " + shapeType);
            StartCoroutine(TransitionWithParticleEffect(shapeType));
        }
        else
        {
            // Directly switch without effects
            Debug.Log("Direct switch to: " + shapeType);
            CreateNewShape(shapeType);
        }
    }
    
    private IEnumerator TransitionWithParticleEffect(ShapeType newShapeType)
    {
        isTransitioning = true;
        Debug.Log("Starting transition from " + currentShapeType + " to " + newShapeType);
        
        // Remember the previous shape's position and color for the particle effect
        Vector3 prevPosition = currentShape.transform.position;
        Color prevColor = currentShape.GetComponent<SpriteRenderer>().color;
        Vector2 shapeSize = currentShape.GetComponent<SpriteRenderer>().bounds.size;
        
        // Play the particle effect at the current shape's position
        if (ParticleEffectManager.Instance != null)
        {
            // Play the morph effect (burst of particles)
            ParticleEffectManager.Instance.PlayMorphEffect(prevPosition, prevColor, 2f, useRainbowEffect);
            
            // Also play outline effect on the shape
            ParticleEffectManager.Instance.PlayShapeOutlineEffect(prevPosition, shapeSize, prevColor, 100);
            
            // Start the dissolve animation on the current shape
            bool dissolveComplete = false;
            currentShape.StartDissolve(morphTransitionTime / 2, () => dissolveComplete = true);
            
            // Wait for the dissolve to complete
            while (!dissolveComplete)
            {
                yield return null;
            }
            
            Debug.Log("Dissolve animation complete, creating new shape: " + newShapeType);
            
            // Store shape info before destroying it
            ShapeType previousType = currentShapeType;
            
            // Create the new shape
            currentShapeType = newShapeType;
            if (currentShape != null)
            {
                Destroy(currentShape.gameObject);
                currentShape = null;
            }
            
            // Create new shape
            Debug.Log("Creating new shape of type: " + newShapeType);
            CreateNewShape(newShapeType);
            Debug.Log("New shape created: " + (currentShape != null ? currentShape.GetType().Name : "null"));
            
            // Make sure the new shape appears at the same position with same color
            if (currentShape != null)
            {
                currentShape.transform.position = prevPosition;
                
                // Set the shape to use same color as previous shape
                SpriteRenderer renderer = currentShape.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = prevColor;
                }
                
                // Start with the shape hidden but ready to rematerialize
                currentShape.GetComponent<SpriteRenderer>().enabled = true;
                
                // Wait a moment for the transition
                yield return new WaitForSeconds(0.1f);
                
                // Play a morph effect for the appearance
                ParticleEffectManager.Instance.PlayMorphEffect(prevPosition, prevColor, 1f, useRainbowEffect);
                
                // Also play outline effect on the new shape
                Vector2 newShapeSize = currentShape.GetComponent<SpriteRenderer>().bounds.size;
                ParticleEffectManager.Instance.PlayShapeOutlineEffect(prevPosition, newShapeSize, prevColor, 100);
                
                // Start the rematerialization animation
                bool rematerializeComplete = false;
                currentShape.StartRematerialize(morphTransitionTime / 2, () => rematerializeComplete = true);
                
                // Wait for the rematerialization to complete
                while (!rematerializeComplete)
                {
                    yield return null;
                }
                
                Debug.Log("Rematerialization complete for shape: " + currentShapeType);
            }
        }
        else
        {
            // Fallback if no particle effect manager
            currentShapeType = newShapeType;
            if (currentShape != null)
            {
                Destroy(currentShape.gameObject);
                currentShape = null;
            }
            
            CreateNewShape(newShapeType);
        }
        
        isTransitioning = false;
        Debug.Log("Transition complete. Current shape: " + currentShapeType);
    }
    
    private void CreateNewShape(ShapeType shapeType)
    {
        Debug.Log("CreateNewShape called for type: " + shapeType);
        
        // Create new shape based on the type
        GameObject prefab = null;
        GameObject newShapeObj = null;
        
        switch (shapeType)
        {
            case ShapeType.Hexagon:
                if (hexagonPrefab != null)
                {
                    prefab = hexagonPrefab;
                    Debug.Log("Using hexagon prefab");
                }
                else
                {
                    newShapeObj = new GameObject("Hexagon");
                    var hexagon = newShapeObj.AddComponent<SimpleHexagon>();
                    Debug.Log("Created new SimpleHexagon component");
                    
                    // Ensure there's a SpriteRenderer and PolygonCollider2D
                    // (Although SimpleHexagon should handle this in its Awake method now)
                    if (newShapeObj.GetComponent<SpriteRenderer>() == null)
                        newShapeObj.AddComponent<SpriteRenderer>();
                        
                    if (newShapeObj.GetComponent<PolygonCollider2D>() == null)
                        newShapeObj.AddComponent<PolygonCollider2D>();
                }
                break;
                
            case ShapeType.Square:
                if (squarePrefab != null)
                {
                    prefab = squarePrefab;
                    Debug.Log("Using square prefab");
                }
                else
                {
                    newShapeObj = new GameObject("Square");
                    var square = newShapeObj.AddComponent<SimpleSquare>();
                    Debug.Log("Created new SimpleSquare component");
                    
                    // Ensure there's a SpriteRenderer and PolygonCollider2D
                    if (newShapeObj.GetComponent<SpriteRenderer>() == null)
                        newShapeObj.AddComponent<SpriteRenderer>();
                        
                    if (newShapeObj.GetComponent<PolygonCollider2D>() == null)
                        newShapeObj.AddComponent<PolygonCollider2D>();
                }
                break;
                
            case ShapeType.Triangle:
                if (trianglePrefab != null)
                {
                    prefab = trianglePrefab;
                    Debug.Log("Using triangle prefab");
                }
                else
                {
                    newShapeObj = new GameObject("Triangle");
                    var triangle = newShapeObj.AddComponent<SimpleTriangle>();
                    Debug.Log("Created new SimpleTriangle component");
                    
                    // Ensure there's a SpriteRenderer and PolygonCollider2D
                    if (newShapeObj.GetComponent<SpriteRenderer>() == null)
                        newShapeObj.AddComponent<SpriteRenderer>();
                        
                    if (newShapeObj.GetComponent<PolygonCollider2D>() == null)
                        newShapeObj.AddComponent<PolygonCollider2D>();
                }
                break;
                
            default:
                Debug.LogError("Unknown shape type: " + shapeType);
                return;
        }
        
        // Instantiate from prefab if we have one
        if (prefab != null)
        {
            newShapeObj = Instantiate(prefab);
        }
        
        // Set up the new shape
        if (newShapeObj != null)
        {
            newShapeObj.transform.SetParent(shapeContainer);
            newShapeObj.transform.localPosition = Vector3.zero;
            currentShape = newShapeObj.GetComponent<Shape>();
            
            if (currentShape == null)
            {
                Debug.LogError("Failed to get Shape component from new object: " + newShapeObj.name);
            }
            else 
            {
                Debug.Log("Shape created successfully: " + currentShape.GetType().Name);
            }
        }
        else
        {
            Debug.LogError("Failed to create new shape object for type: " + shapeType);
        }
        
        // Update current shape type to ensure it matches
        currentShapeType = shapeType;
    }
} 