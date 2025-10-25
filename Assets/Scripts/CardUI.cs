using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Card card;
    public Button button;
    public Text label;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        button.onClick.AddListener(OnClick);
    }

    public void Setup(Card c)
    {
        card = c;
        label.text = c.value + " " + c.suit;
    }

    void OnClick()
    {
        gameManager.OnCardClicked(this);
    }
}
