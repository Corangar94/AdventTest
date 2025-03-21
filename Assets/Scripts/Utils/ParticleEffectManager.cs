using UnityEngine;
using System.Collections;

public class ParticleEffectManager : MonoBehaviour
{
    private static ParticleEffectManager _instance;
    public static ParticleEffectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ParticleEffectManager");
                _instance = go.AddComponent<ParticleEffectManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    // Cached particle effect system
    private new ParticleSystem particleSystem;
    private Material particleMorphMaterial;
    private Material particleMorphAdvancedMaterial;
    private Material rainbowMaterial;
    
    // Cached textures
    private Texture2D noiseTexture;
    private Texture2D voronoiTexture;
    private Texture2D circleTexture;

    [SerializeField] private float dissolveSpeed = 2.0f;
    private Coroutine currentDissolveCoroutine;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Generate textures
        noiseTexture = TextureGenerator.CreateNoiseTexture(256, 4f);
        voronoiTexture = TextureGenerator.CreateVoronoiTexture(256, 24);
        circleTexture = TextureGenerator.CreateCircularGradientTexture(256);
        
        // Add these texture validity checks
        if (noiseTexture == null || voronoiTexture == null || circleTexture == null)
        {
            Debug.LogWarning("Some textures failed to generate. Creating fallback textures.");
            
            if (noiseTexture == null)
                noiseTexture = CreateDefaultParticleTexture();
                
            if (voronoiTexture == null)
                voronoiTexture = CreateDefaultParticleTexture();
                
            if (circleTexture == null)
                circleTexture = CreateDefaultParticleTexture();
        }
        
        // Load materials from resources
        particleMorphMaterial = Resources.Load<Material>("Materials/ParticleMorphMaterial");
        particleMorphAdvancedMaterial = Resources.Load<Material>("Materials/ParticleMorphAdvancedMaterial");
        
        // If the material couldn't be loaded, create it
        if (particleMorphAdvancedMaterial == null)
        {
            Debug.LogWarning("ParticleMorphAdvancedMaterial not found in Resources. Creating a new one at runtime.");
            particleMorphAdvancedMaterial = new Material(Shader.Find("Custom/ParticleMorphAdvanced"));
        }
        
        rainbowMaterial = Resources.Load<Material>("Materials/RainbowAnimatedMaterial");
        
        // Apply textures to materials
        if (particleMorphMaterial != null)
        {
            // Apply a texture to the dissolve texture property
            particleMorphMaterial.SetTexture("_DissolveTexture", voronoiTexture);
        }
        
        if (particleMorphAdvancedMaterial != null)
        {
            // Apply textures to the advanced material
            particleMorphAdvancedMaterial.SetTexture("_DissolveTexture", voronoiTexture);
            particleMorphAdvancedMaterial.SetTexture("_MainTex", circleTexture);
            particleMorphAdvancedMaterial.SetColor("_EdgeColor", new Color(0.8f, 0.4f, 0.0f, 1.0f));
            particleMorphAdvancedMaterial.SetFloat("_EdgeWidth", 0.05f);
            particleMorphAdvancedMaterial.SetFloat("_EmissionStrength", 2.0f);
            particleMorphAdvancedMaterial.SetFloat("_NoiseScale", 25.0f);
            particleMorphAdvancedMaterial.SetFloat("_NoiseSpeed", 1.5f);
            particleMorphAdvancedMaterial.SetFloat("_DistortionAmount", 0.2f);
        }
        
        // Create particle system
        InitializeParticleSystem();
    }
    
    private void InitializeParticleSystem()
    {
        // Create particle system
        GameObject particleObj = new GameObject("MorphParticleSystem");
        particleObj.transform.SetParent(transform);
        
        particleSystem = particleObj.AddComponent<ParticleSystem>();
        var mainModule = particleSystem.main;
        mainModule.startLifetime = 1.5f;
        mainModule.startSpeed = 1f;
        mainModule.startSize = 0.1f;
        mainModule.maxParticles = 1000;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Set up particle renderer
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if (particleMorphAdvancedMaterial != null)
        {
            renderer.material = particleMorphAdvancedMaterial;
            // Check that the material is valid
            Debug.Log("Assigning material to particle system: " + particleMorphAdvancedMaterial.name);
        }
        else
        {
            Debug.LogError("No valid material found for particle system!");
        }
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 5; // Ensure particles render on top
        
        // Set up emission module
        var emission = particleSystem.emission;
        emission.enabled = false; // Only emit during transitions
        
        // Set up shape module
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
        
        // Set up color over lifetime
        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        // Alpha fade out
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = colorGradient;
        
        // Set up size over lifetime
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
            1f, new AnimationCurve(
                new Keyframe(0f, 0.2f, 0f, 0f),
                new Keyframe(0.5f, 1f, 0f, 0f),
                new Keyframe(1f, 0f, 0f, 0f)
            )
        );
        
        // Set up texture sheet animation (for sparkle effect)
        var textureSheetAnimation = particleSystem.textureSheetAnimation;
        textureSheetAnimation.enabled = true;
        textureSheetAnimation.numTilesX = 1;
        textureSheetAnimation.numTilesY = 1;
        
        // Set up rotation over lifetime
        var rotationOverLifetime = particleSystem.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(
            1f, new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(1f, 360f, 0f, 0f)
            )
        );
    }
    
    public void PlayMorphEffect(Vector3 position, Color color, float size = 1f, bool useRainbowEffect = false)
    {
        if (particleSystem == null)
            return;

        
        // Set particle system position
        particleSystem.transform.position = position;
        
        // Configure the particle system
        var mainModule = particleSystem.main;
        mainModule.startColor = color;
        mainModule.startSize = size * 0.1f;
        
        // Set the right material
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        
        if (useRainbowEffect)
        {
            renderer.material = rainbowMaterial;
        }
        else
        {
            renderer.material = particleMorphAdvancedMaterial;
            
            // Start dissolve animation
            if (currentDissolveCoroutine != null)
            {
                StopCoroutine(currentDissolveCoroutine);
            }
            currentDissolveCoroutine = StartCoroutine(AnimateDissolve());
        }
        
        // Configure emission for a burst
        var emission = particleSystem.emission;
        emission.enabled = true;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0f, 50)
        });
        
        // Play the effect
        particleSystem.Play();
        
        // Reset emission after the burst
        StartCoroutine(ResetEmissionAfterBurst());
    }
    
    private IEnumerator ResetEmissionAfterBurst()
    {
        yield return new WaitForSeconds(0.1f);
        var emission = particleSystem.emission;
        emission.enabled = false;
    }
    
    private IEnumerator AnimateDissolve()
    {
        float time = 0;
        
        // Start from fully materialized (dissolve = 0)
        SetParticleMaterialProperties(0);
        
        // Animate to fully dissolved (dissolve = 1)
        while (time < 1)
        {
            time += Time.deltaTime * dissolveSpeed;
            float dissolveAmount = Mathf.Clamp01(time);
            SetParticleMaterialProperties(dissolveAmount);
            yield return null;
        }
        
        // Wait a bit at fully dissolved state
        yield return new WaitForSeconds(0.1f);
        
        // Animate back to fully materialized (dissolve = 0)
        time = 1;
        while (time > 0)
        {
            time -= Time.deltaTime * dissolveSpeed;
            float dissolveAmount = Mathf.Clamp01(time);
            SetParticleMaterialProperties(dissolveAmount);
            yield return null;
        }
    }
    
    public void SetParticleMaterialProperties(float dissolveAmount)
    {
        if (particleMorphMaterial != null)
        {
            particleMorphMaterial.SetFloat("_DissolveAmount", dissolveAmount);
        }
        
        if (particleMorphAdvancedMaterial != null)
        {
            particleMorphAdvancedMaterial.SetFloat("_DissolveAmount", dissolveAmount);
        }
    }
    
    // Add a method to get the dissolve texture for external use
    public Texture2D GetDissolveTexture()
    {
        return voronoiTexture;
    }
    
    // Call this when the object is destroyed to clean up the textures
    private void OnDestroy()
    {
        if (noiseTexture != null)
            Destroy(noiseTexture);
            
        if (voronoiTexture != null)
            Destroy(voronoiTexture);
            
        if (circleTexture != null)
            Destroy(circleTexture);
            
        // Only destroy the material if we created it at runtime
        if (particleMorphAdvancedMaterial != null && Resources.Load<Material>("Materials/ParticleMorphAdvancedMaterial") == null)
            Destroy(particleMorphAdvancedMaterial);
    }
    
    // Play particles that follow the outline of a shape
    public void PlayShapeOutlineEffect(Vector3 position, Vector2 size, Color color, int particleCount = 50)
    {
        if (particleSystem == null)
            return;
        
        // Save the original shape module configuration
        var shape = particleSystem.shape;
        var originalShapeType = shape.shapeType;
        var originalRadius = shape.radius;
        var originalScale = shape.scale;
        
        // Configure the particle system for outline
        var mainModule = particleSystem.main;
        mainModule.startColor = color;
        mainModule.startSize = 0.05f;
        mainModule.startLifetime = 1.0f;
        mainModule.startSpeed = 0.5f;
        
        // Configure shape module for emitting from the edge of an ellipse
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = Mathf.Max(size.x, size.y) * 0.5f;
        shape.radiusThickness = 0f; // Emit from the edge only
        
        // Set the particle system position
        particleSystem.transform.position = position;
        
        // Configure emission for a burst
        var emission = particleSystem.emission;
        emission.enabled = true;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0f, particleCount)
        });
        
        // Configure emission rate over time
        emission.rateOverTime = 0; // No continuous emission
        
        // Play the effect
        particleSystem.Play();
        
        // Reset emission after the burst
        StartCoroutine(RestoreShapeAndResetEmission(originalShapeType, originalRadius, originalScale));
    }
    
    private IEnumerator RestoreShapeAndResetEmission(ParticleSystemShapeType originalType, float originalRadius, Vector3 originalScale)
    {
        yield return new WaitForSeconds(0.1f);
        
        // Reset emission
        var emission = particleSystem.emission;
        emission.enabled = false;
        
        // Restore original shape settings
        var shape = particleSystem.shape;
        shape.shapeType = originalType;
        shape.radius = originalRadius;
        shape.scale = originalScale;
    }
    
   
    
    // Optional: Add this helper method for creating a default texture if needed
    private Texture2D CreateDefaultParticleTexture()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        float centerX = size / 2f;
        float centerY = size / 2f;
        float maxDist = size / 2f;
        
        // Create a soft circular gradient
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distX = x - centerX;
                float distY = y - centerY;
                float dist = Mathf.Sqrt(distX * distX + distY * distY);
                float normalizedDist = Mathf.Clamp01(dist / maxDist);
                
                // Create soft edge
                float alpha = 1f - Mathf.SmoothStep(0.7f, 1f, normalizedDist);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
} 