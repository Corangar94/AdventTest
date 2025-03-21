using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles creation and caching of materials to prevent duplication and memory leaks
/// </summary>
public class ShapeMaterialManager : MonoBehaviour
{
    private static ShapeMaterialManager _instance;
    public static ShapeMaterialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ShapeMaterialManager");
                _instance = go.AddComponent<ShapeMaterialManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // Dictionary to cache materials by shape type
    private Dictionary<ShaderHelper.ShapeType, Material> sharedMaterials = new Dictionary<ShaderHelper.ShapeType, Material>();
    
    // Dictionary to track material instances (for cleanup)
    private Dictionary<int, Material> materialInstances = new Dictionary<int, Material>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Get a shared instance of the glow material for a specific shape type
    /// </summary>
    public Material GetSharedMaterial(ShaderHelper.ShapeType shapeType)
    {
        // Create the material if it doesn't exist
        if (!sharedMaterials.ContainsKey(shapeType))
        {
            Material newMaterial = ShaderHelper.CreateShapeGlowMaterial(shapeType, Color.white);
            if (newMaterial != null)
            {
                sharedMaterials[shapeType] = newMaterial;
            }
        }
        
        return sharedMaterials.ContainsKey(shapeType) ? sharedMaterials[shapeType] : null;
    }
    
    /// <summary>
    /// Get a unique instance of the glow material for a specific shape
    /// </summary>
    public Material GetMaterialInstance(ShaderHelper.ShapeType shapeType, int instanceID)
    {
        // If we already have an instance for this ID, return it
        if (materialInstances.ContainsKey(instanceID))
        {
            return materialInstances[instanceID];
        }
        
        // Get the shared material
        Material sharedMaterial = GetSharedMaterial(shapeType);
        if (sharedMaterial == null) return null;
        
        // Create a new instance
        Material instanceMaterial = new Material(sharedMaterial);
        materialInstances[instanceID] = instanceMaterial;
        
        return instanceMaterial;
    }
    
    /// <summary>
    /// Release a material instance when no longer needed
    /// </summary>
    public void ReleaseMaterial(int instanceID)
    {
        if (materialInstances.ContainsKey(instanceID))
        {
            // Destroy the material instance to prevent memory leaks
            if (materialInstances[instanceID] != null)
            {
                Destroy(materialInstances[instanceID]);
            }
            
            materialInstances.Remove(instanceID);
        }
    }
    
    /// <summary>
    /// Updates the color of a material instance
    /// </summary>
    public void UpdateMaterialColor(int instanceID, Color color)
    {
        if (materialInstances.ContainsKey(instanceID) && materialInstances[instanceID] != null)
        {
            // Update the color and glow color
            Color glowColor = color;
            glowColor.r = Mathf.Min(1.0f, color.r * 1.5f);
            glowColor.g = Mathf.Min(1.0f, color.g * 1.5f);
            glowColor.b = Mathf.Min(1.0f, color.b * 1.5f);
            
            materialInstances[instanceID].SetColor("_GlowColor", glowColor);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up all materials
        foreach (var material in sharedMaterials.Values)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
        
        foreach (var material in materialInstances.Values)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
        
        sharedMaterials.Clear();
        materialInstances.Clear();
    }
} 