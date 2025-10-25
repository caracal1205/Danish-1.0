// CardUI.cs
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
        // si tu n'as pas assigné le button/label dans l'inspector, essaie de les chercher
        if (button == null) button = GetComponent<Button>();
        if (label == null) label = GetComponentInChildren<Text>();
    }

    public void Setup(Card c)
    {
        card = c;
        if (label != null)
            label.text = c.ToString();
        // active le bouton par défaut (le script GameManager décidera si clickable)
        if (button != null)
            button.interactable = true;
    }

    public void DisableInteraction()
    {
        if (button != null) button.interactable = false;
    }
}
