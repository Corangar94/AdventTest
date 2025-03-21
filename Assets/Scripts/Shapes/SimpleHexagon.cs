using UnityEngine;
using System.Collections.Generic;

public class SimpleHexagon : Shape
{
    [SerializeField] private Sprite hexagonSprite;
    [SerializeField] private Material hexagonGlowMaterial;
    
    protected override void Awake()
    {
        // Add a SpriteRenderer if it doesn't exist
        if (GetComponent<SpriteRenderer>() == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        else
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Add a PolygonCollider2D if it doesn't exist
        if (GetComponent<PolygonCollider2D>() == null)
        {
            polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        }
        else
        {
            polygonCollider = GetComponent<PolygonCollider2D>();
        }
        
        // Now call the base class Awake
        base.Awake();
        
        // Set up the sprite - use provided sprite or create one
        if (hexagonSprite != null)
        {
            spriteRenderer.sprite = hexagonSprite;
        }
        else if (spriteRenderer.sprite == null)
        {
            // Try to load from resources first
            Sprite resourceSprite = Resources.Load<Sprite>("Sprites/Hexagon");
            if (resourceSprite != null)
            {
                spriteRenderer.sprite = resourceSprite;
            }
            else
            {
                // Use SpriteGenerator instead of creating sprite manually
                spriteRenderer.sprite = SpriteGenerator.CreateHexagonSprite();
            }
        }
        
        // Use ShapeMaterialManager to get the material
        if (ShapeMaterialManager.Instance != null)
        {
            // Get material from the ShapeMaterialManager
            Material material = ShapeMaterialManager.Instance.GetMaterialInstance(ShaderHelper.ShapeType.Hexagon, GetInstanceID());
            if (material != null)
            {
                spriteRenderer.material = material;
                originalMaterial = material; // Store reference for dissolve effects
            }
        }
        else if (hexagonGlowMaterial != null)
        {
            // Fallback to the serialized material if ShapeMaterialManager is not available
            spriteRenderer.material = new Material(hexagonGlowMaterial);
            originalMaterial = spriteRenderer.material;
        }
        
        // Configure the collider to match the sprite
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
            // If sprite has no physics shape, create a basic hexagon collider
            Vector2[] points = new Vector2[6];
            float radius = 0.4f; // Use 40% of unit size
            
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (i * 60); // 60 degrees per point
                points[i] = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
            }
            
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
        
        // Update shader material color if using ShapeMaterialManager
        if (ShapeMaterialManager.Instance != null)
        {
            ShapeMaterialManager.Instance.UpdateMaterialColor(GetInstanceID(), spriteRenderer.color);
        }
        // Fallback for when ShapeMaterialManager is not available
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