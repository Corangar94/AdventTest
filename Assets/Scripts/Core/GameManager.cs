using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ShapeManager shapeManager;
    [SerializeField] private MenuController menuController;
    [SerializeField] private InputController inputController;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private InstructionsPanel instructionsPanel;
    
    private void Awake()
    {
        // Create AudioManager if it doesn't exist
        if (audioManager == null && AudioManager.Instance == null)
        {
            GameObject audioManagerObj = new GameObject("AudioManager");
            audioManager = audioManagerObj.AddComponent<AudioManager>();
        }
        else if (AudioManager.Instance != null)
        {
            audioManager = AudioManager.Instance;
        }
        
        // Create InputController if it doesn't exist
        if (inputController == null && InputController.Instance == null)
        {
            GameObject inputControllerObj = new GameObject("InputController");
            inputController = inputControllerObj.AddComponent<InputController>();
        }
        else if (InputController.Instance != null)
        {
            inputController = InputController.Instance;
        }
        
        // Create ShapeManager if it doesn't exist
        if (shapeManager == null)
        {
            GameObject shapeManagerObj = new GameObject("ShapeManager");
            shapeManager = shapeManagerObj.AddComponent<ShapeManager>();
        }
        
        // Create MenuController if it doesn't exist
        if (menuController == null)
        {
            GameObject menuControllerObj = new GameObject("MenuController");
            menuController = menuControllerObj.AddComponent<MenuController>();
        }
        
        // Create Instructions Panel if it doesn't exist
        if (instructionsPanel == null)
        {
            GameObject instructionsPanelObj = new GameObject("InstructionsPanel");
            instructionsPanel = instructionsPanelObj.AddComponent<InstructionsPanel>();
        }
    }
    
    private void Start()
    {
        // Set the camera background to a dark color
        Camera.main.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        
        // Initially show the menu
        if (menuController != null)
        {
            // Give it a frame to initialize first
            Invoke(nameof(ShowMenu), 0.1f);
        }
    }
    
    private void ShowMenu()
    {
        if (menuController != null)
        {
            menuController.ToggleMenu();
        }
        else
        {
            Debug.LogError("MenuController is null when trying to show menu");
        }
    }
} 