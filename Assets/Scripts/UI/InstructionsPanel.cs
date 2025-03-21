using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InstructionsPanel : MonoBehaviour
{
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private Button closeButton;
    
    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseInstructions);
        }
    }
    
    private void Start()
    {
        if (instructionsPanel == null)
        {
            CreateInstructionsPanel();
        }
        
        // Show instructions on first run
        ShowInstructions();
        
        // Make sure we have an EventSystem for button clicks
        EnsureEventSystem();
    }
    
    private void EnsureEventSystem()
    {
        // Check if we have an EventSystem
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            // Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("Created EventSystem for UI interaction");
        }
    }
    
    private void CreateInstructionsPanel()
    {
        // Find or create canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("UICanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create panel
        instructionsPanel = new GameObject("InstructionsPanel");
        instructionsPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = instructionsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = instructionsPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // Add instructions text
        GameObject textObj = new GameObject("InstructionsText");
        textObj.transform.SetParent(instructionsPanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.1f);
        textRect.anchorMax = new Vector2(0.95f, 0.9f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Interactive Shape Demo\n\n" +
                    "• Double-click or double-tap the shape to change its color\n" +
                    "• Drag left/right to rotate the shape\n" +
                    "• Use pinch gesture or mouse wheel to scale the shape\n" +
                    "• Use the buttons at the bottom to switch between different shapes\n" +
                    "• Toggle the menu with the button in the top-right corner\n\n" +
                    "This demo showcases procedurally generated shapes with precision hit detection,\n" +
                    "particle effects, and interactive gestures.";
        text.color = Color.white;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        
        // Add close button
        GameObject buttonObj = new GameObject("CloseButton");
        buttonObj.transform.SetParent(instructionsPanel.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.4f, 0.05f);
        buttonRect.anchorMax = new Vector2(0.6f, 0.15f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        closeButton = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.6f, 0.8f, 1.0f);
        
        // Add custom button handler to ensure the click works
        ButtonClickHelper clickHelper = buttonObj.AddComponent<ButtonClickHelper>();
        clickHelper.Initialize(this);
        
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "GOT IT!";
        buttonText.color = Color.white;
        buttonText.fontSize = 20;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        // Add click listener in two ways to ensure it works
        closeButton.onClick.AddListener(CloseInstructions);
        
        // Add color transitions for better feedback
        ColorBlock colors = closeButton.colors;
        colors.normalColor = new Color(0.3f, 0.6f, 0.8f, 1.0f);
        colors.highlightedColor = new Color(0.4f, 0.7f, 0.9f, 1.0f);
        colors.pressedColor = new Color(0.2f, 0.5f, 0.7f, 1.0f);
        colors.selectedColor = new Color(0.4f, 0.7f, 0.9f, 1.0f);
        closeButton.colors = colors;
    }
    
    public void ShowInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
            
            // Play sound if available
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }
        }
    }
    
    public void CloseInstructions()
    {
        Debug.Log("CloseInstructions called");
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
            
            // Play sound if available
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }
        }
    }
}

// Helper class to ensure button clicks work
public class ButtonClickHelper : MonoBehaviour, IPointerClickHandler
{
    private InstructionsPanel panel;
    
    public void Initialize(InstructionsPanel panel)
    {
        this.panel = panel;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Button clicked via IPointerClickHandler");
        if (panel != null)
        {
            panel.CloseInstructions();
        }
    }
} 