using UnityEngine;
using System.Collections.Generic;

public class SimpleTriangle : Shape
{
    [SerializeField] private Sprite triangleSprite;
    [SerializeField] private Material triangleGlowMaterial;
    
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
        if (triangleSprite != null)
        {
            spriteRenderer.sprite = triangleSprite;
        }
        else if (spriteRenderer.sprite == null)
        {
            // Try to load from resources first
            Sprite resourceSprite = Resources.Load<Sprite>("Sprites/Triangle");
            if (resourceSprite != null)
            {
                spriteRenderer.sprite = resourceSprite;
            }
            else
            {
                // Use the SpriteGenerator to create the sprite
                spriteRenderer.sprite = SpriteGenerator.CreateTriangleSprite();
            }
        }
        
        // Use MaterialImporter to get the material
        if (ShapeMaterialManager.Instance != null)
        {
            // Get material from the ShapeMaterialManager
            Material material = ShapeMaterialManager.Instance.GetMaterialInstance(ShaderHelper.ShapeType.Triangle, GetInstanceID());
            if (material != null)
            {
                spriteRenderer.material = material;
                originalMaterial = material; // Store reference for dissolve effects
            }
        }
        else if (triangleGlowMaterial != null)
        {
            // Fallback to the serialized material if MaterialImporter is not available
            spriteRenderer.material = new Material(triangleGlowMaterial);
            originalMaterial = spriteRenderer.material;
        }
        
        // Configure the collider
        UpdateColliderFromSprite();
    }
    
    private void CreateTriangleSprite()
    {
        Debug.Log("Creating triangle sprite");
        
        // Create a new texture for the triangle with transparent background
        int textureSize = 256; // Larger texture for better quality
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[textureSize * textureSize];
        
        // Initialize with transparent pixels
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // Define the triangle vertices (in texture space)
        // Points upward with equal sides (equilateral)
        int margin = 8;
        
        // Calculate points for equilateral triangle
        Vector2Int center = new Vector2Int(textureSize / 2, textureSize / 2);
        float sideLength = textureSize - (margin * 2);
        float height = sideLength * 0.866f; // sqrt(3)/2
        
        Vector2Int top = new Vector2Int(center.x, Mathf.RoundToInt(center.y - height/2) + margin);
        Vector2Int bottomLeft = new Vector2Int(Mathf.RoundToInt(center.x - sideLength/2) + margin, 
                                             Mathf.RoundToInt(center.y + height/2) - margin);
        Vector2Int bottomRight = new Vector2Int(Mathf.RoundToInt(center.x + sideLength/2) - margin, 
                                              Mathf.RoundToInt(center.y + height/2) - margin);
        
        // Draw the filled triangle
        DrawTriangle(pixels, textureSize, top, bottomLeft, bottomRight, Color.white);
        
        // Apply anti-aliasing by softening the edges slightly
        SoftenEdges(pixels, textureSize, 1);
        
        // Apply the pixels to the texture
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Create a sprite from the texture
        spriteRenderer.sprite = Sprite.Create(
            texture, 
            new Rect(0, 0, textureSize, textureSize), 
            new Vector2(0.5f, 0.5f), 
            100f // Pixels per unit
        );
        
        Debug.Log("Triangle sprite created successfully");
    }
    
    // Helper method to soften edges for better anti-aliasing
    private void SoftenEdges(Color[] pixels, int textureSize, int passes)
    {
        // Make a copy of the original pixels
        Color[] originalPixels = new Color[pixels.Length];
        System.Array.Copy(pixels, originalPixels, pixels.Length);
        
        // Simple blur filter for edges
        for (int pass = 0; pass < passes; pass++)
        {
            for (int y = 1; y < textureSize - 1; y++)
            {
                for (int x = 1; x < textureSize - 1; x++)
                {
                    int idx = y * textureSize + x;
                    
                    // Check if this is an edge pixel (has transparent neighbors)
                    bool isEdge = false;
                    if (pixels[idx].a > 0)
                    {
                        // Check its 4 neighbors
                        if (pixels[(y-1) * textureSize + x].a == 0 || 
                            pixels[(y+1) * textureSize + x].a == 0 ||
                            pixels[y * textureSize + (x-1)].a == 0 || 
                            pixels[y * textureSize + (x+1)].a == 0)
                        {
                            isEdge = true;
                        }
                    }
                    
                    // Apply smoothing to edge pixels
                    if (isEdge)
                    {
                        // Average with neighbors
                        Color avgColor = Color.clear;
                        avgColor += originalPixels[(y-1) * textureSize + x];
                        avgColor += originalPixels[(y+1) * textureSize + x];
                        avgColor += originalPixels[y * textureSize + (x-1)];
                        avgColor += originalPixels[y * textureSize + (x+1)];
                        avgColor += originalPixels[idx] * 4; // Weight the center pixel more
                        
                        pixels[idx] = avgColor / 8;
                    }
                }
            }
            
            // Update original for next pass if needed
            if (pass < passes - 1)
            {
                System.Array.Copy(pixels, originalPixels, pixels.Length);
            }
        }
    }
    
    // Helper method to draw a filled triangle on a pixel array
    private void DrawTriangle(Color[] pixels, int textureSize, Vector2Int v1, Vector2Int v2, Vector2Int v3, Color color)
    {
        // Find bounding box of the triangle
        int minX = Mathf.Min(Mathf.Min(v1.x, v2.x), v3.x);
        int maxX = Mathf.Max(Mathf.Max(v1.x, v2.x), v3.x);
        int minY = Mathf.Min(Mathf.Min(v1.y, v2.y), v3.y);
        int maxY = Mathf.Max(Mathf.Max(v1.y, v2.y), v3.y);
        
        // Clamp to texture bounds
        minX = Mathf.Max(0, minX);
        maxX = Mathf.Min(textureSize - 1, maxX);
        minY = Mathf.Max(0, minY);
        maxY = Mathf.Min(textureSize - 1, maxY);
        
        // Check each pixel in the bounding box
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                // Check if the pixel is inside the triangle
                if (IsPointInTriangle(new Vector2(x, y), v1, v2, v3))
                {
                    pixels[y * textureSize + x] = color;
                }
            }
        }
    }
    
    // Check if a point is inside a triangle using barycentric coordinates
    private bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);
        
        bool hasNegative = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPositive = (d1 > 0) || (d2 > 0) || (d3 > 0);
        
        // If all signs are the same, the point is inside the triangle
        return !(hasNegative && hasPositive);
    }
    
    // Helper function for IsPointInTriangle
    private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
    
    private void UpdateColliderFromSprite()
    {
        if (spriteRenderer.sprite == null || polygonCollider == null)
            return;
            
        // Get the sprite's physics shape count
        int pathCount = spriteRenderer.sprite.GetPhysicsShapeCount();
        
        if (pathCount == 0)
        {
            // If sprite has no physics shape, create a basic triangle collider
            Vector2[] points = new Vector2[3];
            float size = 0.4f; // Use 40% of unit size
            
            points[0] = new Vector2(0, size);       // Top
            points[1] = new Vector2(-size, -size);  // Bottom left
            points[2] = new Vector2(size, -size);   // Bottom right
            
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