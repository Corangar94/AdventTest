using UnityEngine;
using System.Collections;

public abstract class Shape : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected bool useRainbowEffect = true;
    
    // Gesture control settings
    [SerializeField] protected float minScale = 0.5f;
    [SerializeField] protected float maxScale = 2.0f;
    [SerializeField] protected float rotationSmoothTime = 0.1f;
    [SerializeField] protected float scaleSmoothTime = 0.1f;
    
    protected PolygonCollider2D polygonCollider;
    private Vector3 originalScale;
    private bool isAnimating = false;
    protected Material dissolveMaterial;
    protected Coroutine dissolveCoroutine;
    protected Material originalMaterial;
    
    // For smooth rotation and scaling
    private float currentRotVelocity;
    private Vector3 currentScaleVelocity;
    private Quaternion targetRotation;
    private Vector3 targetScale;
    
    protected virtual void Awake()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        originalScale = transform.localScale;
        
        // Initialize rotation and scale targets
        targetRotation = transform.rotation;
        targetScale = transform.localScale;
        
        // We no longer need to store the original material here since MaterialImporter will handle it
    }
    
    protected virtual void Start()
    {
        // Initially set a random color
        ChangeToRandomColor();
        
        // Subscribe to the input controller's events
        if (InputController.Instance != null)
        {
            InputController.Instance.OnDoubleClick += HandleDoubleClick;
            InputController.Instance.OnRotate += HandleRotation;
            InputController.Instance.OnScale += HandleScaling;
        }
        else
        {
            Debug.LogWarning("InputController not found. Gesture detection will not work.");
        }
        
        // Ensure we have the particle effect manager
        if (ParticleEffectManager.Instance == null)
        {
            Debug.LogWarning("ParticleEffectManager not found. Particle effects will not work.");
        }
    }
    
    protected virtual void Update()
    {
        // Apply smooth rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-rotationSmoothTime * 30f * Time.deltaTime));
        
        // Apply smooth scaling
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref currentScaleVelocity, scaleSmoothTime);
    }
    
    protected virtual void OnDestroy()
    {
        // Unsubscribe from the input controller's events
        if (InputController.Instance != null)
        {
            InputController.Instance.OnDoubleClick -= HandleDoubleClick;
            InputController.Instance.OnRotate -= HandleRotation;
            InputController.Instance.OnScale -= HandleScaling;
        }
        
        // Release material resources using ShapeMaterialManager
        if (ShapeMaterialManager.Instance != null)
        {
            ShapeMaterialManager.Instance.ReleaseMaterial(GetInstanceID());
        }
        
        // Clean up dissolve material
        if (dissolveMaterial != null)
        {
            Destroy(dissolveMaterial);
            dissolveMaterial = null;
        }
    }
    
    private void HandleDoubleClick(GameObject clickedObject)
    {
        // Check if this is the object that was double-clicked
        if (clickedObject == gameObject)
        {
            ChangeToRandomColor();
            if (!isAnimating)
            {
                StartPulseAnimation();
                
                // Play particle effect
                PlayParticleEffect();
            }
        }
    }
    
    private void HandleRotation(GameObject rotatedObject, float rotationAmount)
    {
        // Check if this is the object that should be rotated
        if (rotatedObject == gameObject)
        {
            // Update target rotation (add to current rotation)
            targetRotation *= Quaternion.Euler(0, 0, rotationAmount);
            
            // Play sound effect if AudioManager exists
            if (AudioManager.Instance != null && Mathf.Abs(rotationAmount) > 1f)
            {
                AudioManager.Instance.PlayRotationSound();
            }
        }
    }
    
    private void HandleScaling(GameObject scaledObject, float scaleFactor)
    {
        // Check if this is the object that should be scaled
        if (scaledObject == gameObject)
        {
            // Calculate new scale (based on original scale)
            Vector3 newScale = targetScale * scaleFactor;
            
            // Clamp scale between min and max values
            float magnitude = newScale.magnitude / originalScale.magnitude;
            if (magnitude < minScale)
            {
                newScale = originalScale * minScale;
            }
            else if (magnitude > maxScale)
            {
                newScale = originalScale * maxScale;
            }
            
            // Update target scale
            targetScale = newScale;
            
            // Play sound effect if AudioManager exists
            if (AudioManager.Instance != null && Mathf.Abs(scaleFactor - 1f) > 0.05f)
            {
                AudioManager.Instance.PlayScalingSound();
            }
        }
    }
    
    // Reset the shape to its original scale and rotation
    public virtual void ResetTransform()
    {
        targetRotation = Quaternion.identity;
        targetScale = originalScale;
    }
    
    // Legacy input method for backward compatibility
    private void OnMouseDown()
    {
        // Only use this if we don't have an InputController
        if (InputController.Instance == null)
        {
            // Double-click logic would go here
            ChangeToRandomColor();
            if (!isAnimating)
            {
                StartPulseAnimation();
                PlayParticleEffect();
            }
        }
    }
    
    public virtual void ChangeToRandomColor()
    {
        spriteRenderer.color = Random.ColorHSV();
        
        // Play sound effect if AudioManager exists
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShapeChangeSound();
        }
    }
    
    private void StartPulseAnimation()
    {
        isAnimating = true;
        LeanTween.scale(gameObject, originalScale * 1.2f, 0.1f).setOnComplete(() => {
            LeanTween.scale(gameObject, originalScale, 0.1f).setOnComplete(() => {
                isAnimating = false;
            });
        });
    }
    
    protected void PlayParticleEffect()
    {
        if (ParticleEffectManager.Instance != null)
        {
            // Play a particle effect at this shape's position with its color
            ParticleEffectManager.Instance.PlayMorphEffect(
                transform.position, 
                spriteRenderer.color, 
                transform.localScale.x,
                useRainbowEffect
            );
        }
    }
    
    // New methods for dissolve effects
    
    // Start dissolve animation (0 = fully visible, 1 = fully dissolved)
    public void StartDissolve(float duration = 0.5f, System.Action onComplete = null)
    {
        if (dissolveCoroutine != null)
        {
            StopCoroutine(dissolveCoroutine);
        }
        
        dissolveCoroutine = StartCoroutine(DissolveRoutine(0f, 1f, duration, onComplete));
    }
    
    // Start rematerialize animation (1 = fully dissolved, 0 = fully visible)
    public void StartRematerialize(float duration = 0.5f, System.Action onComplete = null)
    {
        if (dissolveCoroutine != null)
        {
            StopCoroutine(dissolveCoroutine);
        }
        
        dissolveCoroutine = StartCoroutine(DissolveRoutine(1f, 0f, duration, onComplete));
    }
    
    // Dissolve animation coroutine
    protected IEnumerator DissolveRoutine(float startValue, float endValue, float duration, System.Action onComplete)
    {
        // Store original material if not already saved
        if (originalMaterial == null && spriteRenderer.material != null)
        {
            originalMaterial = spriteRenderer.material;
        }

        // Create the dissolve material if needed
        if (dissolveMaterial == null && ParticleEffectManager.Instance != null)
        {
            // Create a material instance for the dissolve effect
            dissolveMaterial = new Material(Shader.Find("Custom/ParticleMorphAdvanced"));
            dissolveMaterial.SetTexture("_MainTex", spriteRenderer.sprite.texture);
            dissolveMaterial.SetTexture("_DissolveTexture", ParticleEffectManager.Instance.GetDissolveTexture());
            dissolveMaterial.SetColor("_EdgeColor", new Color(0.8f, 0.4f, 0f, 1f));
            dissolveMaterial.SetFloat("_EdgeWidth", 0.05f);
        }
        
        // Apply the material if we have one
        if (dissolveMaterial != null)
        {
            // Set the color to match the shape
            dissolveMaterial.SetColor("_Color", spriteRenderer.color);
            
            // Assign the material to the renderer
            spriteRenderer.material = dissolveMaterial;
            
            // Animate the dissolve amount
            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                float value = Mathf.Lerp(startValue, endValue, t);
                dissolveMaterial.SetFloat("_DissolveAmount", value);
                yield return null;
            }
            
            // Ensure we reach the final value exactly
            dissolveMaterial.SetFloat("_DissolveAmount", endValue);
            
            // If we're dissolving completely, hide the renderer
            if (endValue >= 1f)
            {
                spriteRenderer.enabled = false;
            }
            else if (endValue <= 0f)
            {
                // Reset to the original material when fully rematerialized
                spriteRenderer.material = originalMaterial;
            }
        }
        
        // Call the completion callback if provided
        onComplete?.Invoke();
    }
} 