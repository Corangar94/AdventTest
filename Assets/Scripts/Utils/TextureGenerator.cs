using UnityEngine;

public static class TextureGenerator
{
    /// <summary>
    /// Creates a noise texture for dissolve effects
    /// </summary>
    /// <param name="size">Size of the texture (width and height)</param>
    /// <param name="scale">Scale of the noise (higher = more detailed)</param>
    /// <returns>A noise texture suitable for dissolve effects</returns>
    public static Texture2D CreateNoiseTexture(int size = 256, float scale = 1.0f)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Repeat;
        
        float[] perlinValues = new float[size * size];
        float min = float.MaxValue;
        float max = float.MinValue;
        
        // Generate perlin noise
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float xCoord = (float)x / size * scale;
                float yCoord = (float)y / size * scale;
                
                // Use multiple octaves for more natural looking noise
                float noise = 0;
                float amplitude = 1.0f;
                float frequency = 1.0f;
                float persistence = 0.5f;
                
                for (int i = 0; i < 4; i++) // 4 octaves
                {
                    float n = Mathf.PerlinNoise(xCoord * frequency, yCoord * frequency);
                    noise += n * amplitude;
                    
                    amplitude *= persistence;
                    frequency *= 2;
                }
                
                int idx = y * size + x;
                perlinValues[idx] = noise;
                
                // Track min and max for normalization
                if (noise < min) min = noise;
                if (noise > max) max = noise;
            }
        }
        
        // Normalize and set texture pixels
        Color[] colors = new Color[size * size];
        for (int i = 0; i < perlinValues.Length; i++)
        {
            float normalizedValue = Mathf.InverseLerp(min, max, perlinValues[i]);
            colors[i] = new Color(normalizedValue, normalizedValue, normalizedValue, 1);
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return texture;
    }
    
    /// <summary>
    /// Creates a circular gradient texture (white at center, black at edges)
    /// </summary>
    /// <param name="size">Size of the texture</param>
    /// <returns>A circular gradient texture</returns>
    public static Texture2D CreateCircularGradientTexture(int size = 256)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2, size / 2);
        float maxDistance = size / 2;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
                float value = 1 - normalizedDistance;
                
                int idx = y * size + x;
                colors[idx] = new Color(value, value, value, 1);
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return texture;
    }
    
    /// <summary>
    /// Creates a voronoi noise texture
    /// </summary>
    /// <param name="size">Size of the texture</param>
    /// <param name="cellCount">Number of cells in the pattern</param>
    /// <returns>A voronoi noise texture</returns>
    public static Texture2D CreateVoronoiTexture(int size = 256, int cellCount = 16)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        // Generate random cell centers
        Vector2[] points = new Vector2[cellCount];
        for (int i = 0; i < cellCount; i++)
        {
            points[i] = new Vector2(
                Random.Range(0, size),
                Random.Range(0, size)
            );
        }
        
        // Generate Voronoi texture
        Color[] colors = new Color[size * size];
        float maxDistance = size * 0.5f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pixelPos = new Vector2(x, y);
                
                // Find distance to closest point
                float minDist = float.MaxValue;
                for (int i = 0; i < cellCount; i++)
                {
                    float dist = Vector2.Distance(pixelPos, points[i]);
                    minDist = Mathf.Min(minDist, dist);
                }
                
                // Normalize distance
                float value = Mathf.Clamp01(minDist / maxDistance);
                
                int idx = y * size + x;
                colors[idx] = new Color(value, value, value, 1);
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return texture;
    }
} 