using UnityEngine;

public static class SpriteGenerator
{
    /// <summary>
    /// Creates a simple hexagon sprite
    /// </summary>
    /// <param name="size">Size of the texture (width and height)</param>
    /// <param name="color">Color of the hexagon</param>
    /// <returns>A sprite with a hexagon shape</returns>
    public static Sprite CreateHexagonSprite(int size = 128, Color? color = null)
    {
        Color shapeColor = color ?? Color.white;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        // Fill with transparent pixels initially
        Color[] colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.clear;
        }
        texture.SetPixels(colors);
        
        // Calculate center and radius
        Vector2 center = new Vector2(size / 2, size / 2);
        float radius = size * 0.4f;
        
        // Calculate hexagon points
        Vector2[] points = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (i * 60); // 60 degrees per point
            points[i] = center + new Vector2(
                radius * Mathf.Cos(angle),
                radius * Mathf.Sin(angle)
            );
        }
        
        // Use more efficient rendering by drawing triangles from center
        for (int i = 0; i < 6; i++)
        {
            int nextI = (i + 1) % 6;
            // Draw a triangle (center, current point, next point)
            DrawTriangle(texture, center, points[i], points[nextI], shapeColor);
        }
        
        texture.Apply();
        
        // Create and return the sprite
        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100
        );
    }
    
    /// <summary>
    /// Creates a simple square sprite
    /// </summary>
    /// <param name="size">Size of the texture (width and height)</param>
    /// <param name="color">Color of the square</param>
    /// <returns>A sprite with a square shape</returns>
    public static Sprite CreateSquareSprite(int size = 128, Color? color = null)
    {
        Color shapeColor = color ?? Color.white;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        // Fill with transparent pixels initially
        Color[] colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.clear;
        }
        texture.SetPixels(colors);
        
        // Calculate the square dimensions (80% of texture size)
        float squareSize = size * 0.8f;
        float offset = (size - squareSize) / 2f;
        
        // Define the square corners
        Vector2[] points = new Vector2[4]
        {
            new Vector2(offset, offset),                      // Bottom-left
            new Vector2(offset + squareSize, offset),         // Bottom-right
            new Vector2(offset + squareSize, offset + squareSize), // Top-right
            new Vector2(offset, offset + squareSize)          // Top-left
        };
        
        // Draw the square (as two triangles)
        Vector2 center = new Vector2(size / 2, size / 2);
        DrawTriangle(texture, points[0], points[1], points[2], shapeColor);
        DrawTriangle(texture, points[0], points[2], points[3], shapeColor);
        
        texture.Apply();
        
        // Create and return the sprite
        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100
        );
    }
    
    /// <summary>
    /// Creates a simple triangle sprite
    /// </summary>
    /// <param name="size">Size of the texture (width and height)</param>
    /// <param name="color">Color of the triangle</param>
    /// <returns>A sprite with a triangle shape</returns>
    public static Sprite CreateTriangleSprite(int size = 128, Color? color = null)
    {
        Color shapeColor = color ?? Color.white;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        // Fill with transparent pixels initially
        Color[] colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.clear;
        }
        texture.SetPixels(colors);
        
        // Calculate the triangle points (equilateral)
        float triangleHeight = size * 0.7f;
        float triangleWidth = triangleHeight * 1.1547f; // (2/√3)
        
        Vector2 center = new Vector2(size / 2, size / 2);
        Vector2 top = new Vector2(center.x, center.y + triangleHeight * 0.4f);
        Vector2 bottomLeft = new Vector2(center.x - triangleWidth / 2, center.y - triangleHeight * 0.6f);
        Vector2 bottomRight = new Vector2(center.x + triangleWidth / 2, center.y - triangleHeight * 0.6f);
        
        // Draw the triangle
        DrawTriangle(texture, top, bottomLeft, bottomRight, shapeColor);
        
        texture.Apply();
        
        // Create and return the sprite
        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100
        );
    }
    
    // More efficient triangle drawing method
    private static void DrawTriangle(Texture2D texture, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
    {
        // Find the bounding box of the triangle
        int minX = Mathf.FloorToInt(Mathf.Min(p1.x, Mathf.Min(p2.x, p3.x)));
        int maxX = Mathf.CeilToInt(Mathf.Max(p1.x, Mathf.Max(p2.x, p3.x)));
        int minY = Mathf.FloorToInt(Mathf.Min(p1.y, Mathf.Min(p2.y, p3.y)));
        int maxY = Mathf.CeilToInt(Mathf.Max(p1.y, Mathf.Max(p2.y, p3.y)));
        
        // Clamp to texture bounds
        minX = Mathf.Max(0, minX);
        maxX = Mathf.Min(texture.width - 1, maxX);
        minY = Mathf.Max(0, minY);
        maxY = Mathf.Min(texture.height - 1, maxY);
        
        // Check each pixel in the bounding box
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2 p = new Vector2(x, y);
                
                // If the point is in the triangle, color it
                if (IsPointInTriangle(p, p1, p2, p3))
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }
    
    // Check if a point is inside a triangle using barycentric coordinates
    private static bool IsPointInTriangle(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float d1, d2, d3;
        bool hasNeg, hasPos;
        
        d1 = Sign(p, p1, p2);
        d2 = Sign(p, p2, p3);
        d3 = Sign(p, p3, p1);
        
        hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        
        return !(hasNeg && hasPos);
    }
    
    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
} 