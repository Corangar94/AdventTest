using UnityEngine;

/// <summary>
/// Helper class for working with shape shaders
/// </summary>
public class ShaderHelper : MonoBehaviour
{
    public enum ShapeType
    {
        Hexagon,
        Square,
        Triangle
    }
    
    /// <summary>
    /// Configures a material using the ShapeGlowShader for a specific shape type
    /// </summary>
    public static void ConfigureShapeGlowMaterial(Material material, ShapeType shapeType, Color glowColor)
    {
        if (material == null) return;
        
        // Set the glow color
        material.SetColor("_GlowColor", glowColor);
        
        // Set the shape type
        material.SetFloat("_ShapeType", (float)shapeType);
        
        // Adjust parameters based on shape type
        switch (shapeType)
        {
            case ShapeType.Hexagon:
                material.SetFloat("_GlowWidth", 0.05f);
                material.SetFloat("_GlowIntensity", 0.6f);
                material.SetFloat("_PulseAmount", 0.2f);
                material.SetFloat("_PulseSpeed", 0.8f);
                break;
                
            case ShapeType.Square:
                material.SetFloat("_GlowWidth", 0.05f);
                material.SetFloat("_GlowIntensity", 0.5f);
                material.SetFloat("_PulseAmount", 0.15f);
                material.SetFloat("_PulseSpeed", 0.9f);
                break;
                
            case ShapeType.Triangle:
                material.SetFloat("_GlowWidth", 0.05f);
                material.SetFloat("_GlowIntensity", 0.7f);
                material.SetFloat("_PulseAmount", 0.25f);
                material.SetFloat("_PulseSpeed", 0.75f);
                break;
        }
    }
    
    /// <summary>
    /// Creates and configures a new material using the ShapeGlowShader
    /// </summary>
    public static Material CreateShapeGlowMaterial(ShapeType shapeType, Color glowColor)
    {
        // Find the glow shader
        Shader shader = Shader.Find("Custom/ShapeGlowShader");
        if (shader == null)
        {
            Debug.LogError("ShapeGlowShader not found! Falling back to default shader.");
            return new Material(Shader.Find("Sprites/Default"));
        }
        
        // Create a new material with the shader
        Material material = new Material(shader);
        
        // Configure the material
        ConfigureShapeGlowMaterial(material, shapeType, glowColor);
        
        return material;
    }
} 