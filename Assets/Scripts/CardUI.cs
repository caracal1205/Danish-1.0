using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CardUI : MonoBehaviour
{
    public Card card;
    public Button button;
    public Text label; 
    private GameManager gameManager;

    void Awake()
    {
        Canvas myCanvas = GetComponent<Canvas>();
        if (myCanvas != null && Camera.main != null) myCanvas.worldCamera = Camera.main;

        if (button == null) button = GetComponent<Button>();
        // Label is optional now since the 3D model shows the rank/suit
        if (label == null) label = GetComponentInChildren<Text>();
    }

    public void Setup(Card c, GameManager manager)
    {
        card = c;
        gameManager = manager;
        
        // Hide the debug text if it exists
        if (label != null) label.enabled = false;
        
        if (button != null)
            button.interactable = true;

        // Note: We no longer call Enable3DModel because 
        // this prefab IS already the correct 3D model.
    }
    
    public void DisableInteraction()
    {
        if (button != null) button.interactable = false;
    }
}