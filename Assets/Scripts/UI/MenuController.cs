using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    [SerializeField] private ShapeManager shapeManager;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button hexagonButton;
    [SerializeField] private Button squareButton;
    [SerializeField] private Button triangleButton;
    [SerializeField] private Button toggleMenuButton;
    
    private void Awake()
    {
        // Find ShapeManager if not assigned
        if (shapeManager == null)
        {
            shapeManager = FindFirstObjectByType<ShapeManager>();
        }
        
        // Set up buttons if they exist
        if (hexagonButton != null)
        {
            hexagonButton.onClick.AddListener(() => { 
                SelectShape(ShapeManager.ShapeType.Hexagon);
                PlayButtonClickSound();
            });
        }
        
        if (squareButton != null)
        {
            squareButton.onClick.AddListener(() => { 
                SelectShape(ShapeManager.ShapeType.Square);
                PlayButtonClickSound();
            });
        }
        
        if (triangleButton != null)
        {
            triangleButton.onClick.AddListener(() => { 
                SelectShape(ShapeManager.ShapeType.Triangle);
                PlayButtonClickSound();
            });
        }
        
        if (toggleMenuButton != null)
        {
            toggleMenuButton.onClick.AddListener(() => {
                ToggleMenu();
                PlayButtonClickSound();
            });
        }
    }
    
    private void Start()
    {
        // Create menu system if it doesn't exist
        if (menuPanel == null)
        {
            CreateUISystem();
        }
    }
    
    private void CreateUISystem()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create menu panel
        menuPanel = new GameObject("MenuPanel");
        menuPanel.transform.SetParent(canvasObj.transform, false);
        
        RectTransform menuRect = menuPanel.AddComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0, 0);
        menuRect.anchorMax = new Vector2(1, 0.2f);
        menuRect.offsetMin = Vector2.zero;
        menuRect.offsetMax = Vector2.zero;
        
        Image menuImage = menuPanel.AddComponent<Image>();
        menuImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Create horizontal layout
        HorizontalLayoutGroup layout = menuPanel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        
        // Create shape buttons
        hexagonButton = CreateButton("Hexagon", menuPanel.transform);
        squareButton = CreateButton("Square", menuPanel.transform);
        triangleButton = CreateButton("Triangle", menuPanel.transform);
        
        // Add listeners to buttons
        hexagonButton.onClick.AddListener(() => { 
            SelectShape(ShapeManager.ShapeType.Hexagon);
            PlayButtonClickSound();
        });
        squareButton.onClick.AddListener(() => { 
            SelectShape(ShapeManager.ShapeType.Square);
            PlayButtonClickSound();
        });
        triangleButton.onClick.AddListener(() => { 
            SelectShape(ShapeManager.ShapeType.Triangle);
            PlayButtonClickSound();
        });
        
        // Create toggle menu button
        GameObject toggleButtonObj = new GameObject("ToggleMenuButton");
        toggleButtonObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform toggleRect = toggleButtonObj.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0.9f, 0.9f);
        toggleRect.anchorMax = new Vector2(1.0f, 1.0f);
        toggleRect.offsetMin = new Vector2(0, 0);
        toggleRect.offsetMax = new Vector2(-10, -10);
        
        toggleMenuButton = toggleButtonObj.AddComponent<Button>();
        Image toggleImage = toggleButtonObj.AddComponent<Image>();
        toggleImage.color = new Color(0.3f, 0.3f, 0.8f, 1.0f);
        
        GameObject toggleTextObj = new GameObject("Text");
        toggleTextObj.transform.SetParent(toggleButtonObj.transform, false);
        
        RectTransform toggleTextRect = toggleTextObj.AddComponent<RectTransform>();
        toggleTextRect.anchorMin = Vector2.zero;
        toggleTextRect.anchorMax = Vector2.one;
        toggleTextRect.offsetMin = Vector2.zero;
        toggleTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI toggleText = toggleTextObj.AddComponent<TextMeshProUGUI>();
        toggleText.text = "Menu";
        toggleText.color = Color.white;
        toggleText.alignment = TextAlignmentOptions.Center;
        toggleText.fontSize = 18;
        
        toggleMenuButton.onClick.AddListener(() => {
            ToggleMenu();
            PlayButtonClickSound();
        });
    }
    
    private Button CreateButton(string text, Transform parent)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(120, 50);
        
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.6f, 0.8f, 1.0f);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontSize = 18;
        
        // Setup ColorBlock to show proper button transitions
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.6f, 0.8f, 1.0f);
        colors.highlightedColor = new Color(0.4f, 0.7f, 0.9f, 1.0f);
        colors.pressedColor = new Color(0.2f, 0.5f, 0.7f, 1.0f);
        colors.selectedColor = new Color(0.4f, 0.7f, 0.9f, 1.0f);
        button.colors = colors;
        
        return button;
    }
    
    public void ToggleMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(!menuPanel.activeSelf);
        }
    }
    
    private void SelectShape(ShapeManager.ShapeType shapeType)
    {
        if (shapeManager != null)
        {
            shapeManager.SwitchShape(shapeType);
        }
    }
    
    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }
    }
} 