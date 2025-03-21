using UnityEngine;

/// <summary>
/// This script should be added to the main scene to initialize the application.
/// </summary>
public class SceneInitializer : MonoBehaviour
{
    [SerializeField] private bool initializeComponents = true;
    
    private void Awake()
    {
        if (initializeComponents)
        {
            // Ensure singleton components are initialized in the correct order
            Debug.Log("Initializing core components...");
            
            // Initialize ShapeMaterialManager first (new)
            if (ShapeMaterialManager.Instance == null)
            {
                Debug.LogWarning("ShapeMaterialManager not initialized - creating instance");
                var materialManager = ShapeMaterialManager.Instance;
            }
            
            // Then AudioManager
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("AudioManager not initialized - creating instance");
                var audioManager = AudioManager.Instance;
            }
            
            // Then InputController
            if (InputController.Instance == null)
            {
                Debug.LogWarning("InputController not initialized - creating instance");
                var inputController = InputController.Instance;
            }
            
            // Then ParticleEffectManager
            if (ParticleEffectManager.Instance == null)
            {
                Debug.LogWarning("ParticleEffectManager not initialized - creating instance");
                var particleManager = ParticleEffectManager.Instance;
            }
            
            // Finally, create GameManager if it doesn't exist
            if (FindObjectOfType<GameManager>() == null)
            {
                Debug.LogWarning("GameManager not initialized - creating instance");
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
            }
            
            Debug.Log("Core components initialized");
        }
    }
} 