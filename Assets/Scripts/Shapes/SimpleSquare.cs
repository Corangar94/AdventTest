using UnityEngine;
using System.Collections.Generic;

public class SimpleSquare : Shape
{
    [SerializeField] private Sprite squareSprite;
    [SerializeField] private Material squareGlowMaterial;
    
    protected override void Awake()
    {
        // Ensure we have a SpriteRenderer BEFORE calling base.Awake()
        if (GetComponent<SpriteRenderer>() == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        else
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Ensure we have a PolygonCollider2D
        if (GetComponent<PolygonCollider2D>() == null)
        {
            polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        }
        else
        {
            polygonCollider = GetComponent<PolygonCollider2D>();
        }
        
        // NOW call base.Awake() since components exist
        base.Awake();
        
        // Set up the sprite - use provided sprite or create one
        if (squareSprite != null)
        {
            spriteRenderer.sprite = squareSprite;
        }
        else if (spriteRenderer.sprite == null)
        {
            // Try to load from resources first
            Sprite resourceSprite = Resources.Load<Sprite>("Sprites/Square");
            if (resourceSprite != null)
            {
                spriteRenderer.sprite = resourceSprite;
            }
            else
            {
                // Use the SpriteGenerator to create the sprite
                spriteRenderer.sprite = SpriteGenerator.CreateSquareSprite();
            }
        }
        
        // Use MaterialImporter to get the material
        if (ShapeMaterialManager.Instance != null)
        {
            // Get material from the ShapeMaterialManager
            Material material = ShapeMaterialManager.Instance.GetMaterialInstance(ShaderHelper.ShapeType.Square, GetInstanceID());
            if (material != null)
            {
                spriteRenderer.material = material;
                originalMaterial = material; // Store reference for dissolve effects
            }
        }
        else if (squareGlowMaterial != null)
        {
            // Fallback to the serialized material if MaterialImporter is not available
            spriteRenderer.material = new Material(squareGlowMaterial);
            originalMaterial = spriteRenderer.material;
        }
        
        // Configure the collider
        UpdateColliderFromSprite();
    }
    
    private void UpdateColliderFromSprite()
    {
        if (spriteRenderer.sprite == null || polygonCollider == null)
            return;
            
        // Get the sprite's physics shape count
        int pathCount = spriteRenderer.sprite.GetPhysicsShapeCount();
        
        if (pathCount == 0)
        {
            // If sprite has no physics shape, create a basic square collider
            Vector2[] points = new Vector2[4];
            float size = 0.4f; // Use 40% of unit size
            
            points[0] = new Vector2(-size, -size); // Bottom left
            points[1] = new Vector2(size, -size);  // Bottom right
            points[2] = new Vector2(size, size);   // Top right
            points[3] = new Vector2(-size, size);  // Top left
            
            polygonCollider.SetPath(0, points);
        }
        else
        {
            // Use sprite's physics shape
            List<Vector2> path = new List<Vector2>();
            for (int i = 0; i < pathCount; i++)
            {
                path.Clear();
                spriteRenderer.sprite.GetPhysicsShape(i, path);
                polygonCollider.SetPath(i, path.ToArray());
            }
        }
    }
    
    public override void ChangeToRandomColor()
    {
        base.ChangeToRandomColor();
        
        // Update shader material color if using MaterialImporter
        if (ShapeMaterialManager.Instance != null)
        {
            ShapeMaterialManager.Instance.UpdateMaterialColor(GetInstanceID(), spriteRenderer.color);
        }
        // Fallback for when MaterialImporter is not available
        else if (spriteRenderer.material != null)
        {
            // Update the glow color to match the shape color, but with higher intensity
            Color glowColor = spriteRenderer.color;
            glowColor.r = Mathf.Min(1.0f, glowColor.r * 1.5f);
            glowColor.g = Mathf.Min(1.0f, glowColor.g * 1.5f);
            glowColor.b = Mathf.Min(1.0f, glowColor.b * 1.5f);
            
            // Apply the glow color to the material
            spriteRenderer.material.SetColor("_GlowColor", glowColor);
        }
    }
} 