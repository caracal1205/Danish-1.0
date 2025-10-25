using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private List<Card> deck = new List<Card>();
    private List<Card> pile = new List<Card>();
    private List<Card> burned = new List<Card>();
    private List<Player> players = new List<Player>();

    private int currentPlayerIndex = 0;

    public Transform botHandPanel;
    public Transform playerHandPanel;
    public Transform drawPilePos;
    public Transform discardPilePos;
    public Transform burnPilePos;

    public GameObject cardPrefab;
    public GameObject cardBackPrefab;
    public Text pileText;
    public Text infoText;

    private bool reversedRule = false;
    private bool sameSuitRule = false;
    private Suit restrictedSuit;
    private int minRankRequired;

    void Start()
    {
        StartNewGame();
        StartTurn();
    }

    public void StartNewGame()
    {
        deck = Deck.CreateStandard52();
        Deck.Shuffle(deck);

        players.Add(new Player("Ruben"));
        players.Add(new Player("Timothy"));

        DealInitialCards(players[0]);
        DealInitialCards(players[1]);

        pile.Clear();
        burned.Clear();
        reversedRule = false;
        sameSuitRule = false;

        pile.Add(DrawFromDeck());
        UpdatePileText();
    } // <-- manquait cette accolade !

    private void DealInitialCards(Player p)
    {
        // 3 cachées, 3 visibles, 3 en main
        p.hidden.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        p.visible.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        p.hand.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
    }

    void StartTurn()
    {
        Player current = players[currentPlayerIndex];
        infoText.text = $"Tour de : {current.name}";
    }

    void ShowPlayerHand(Player player)
    {
        foreach (Transform child in playerHandPanel)
            Destroy(child.gameObject);

        float spread = 20f;
        int n = player.hand.Count;
        float startAngle = -spread * (n - 1) / 2;

        for (int i = 0; i < n; i++)
        {
            GameObject cardGO = Instantiate(cardPrefab, playerHandPanel);
            CardUI ui = cardGO.GetComponent<CardUI>();
            ui.Setup(player.hand[i]);

            RectTransform rt = cardGO.GetComponent<RectTransform>();
            rt.localRotation = Quaternion.Euler(0, 0, startAngle + spread * i);
        }
    }

    public void OnCardClicked(CardUI clickedCard)
    {
        Player current = players[currentPlayerIndex];
        if (!IsCardPlayable(clickedCard.card))
        {
            infoText.text = "Carte non jouable !";
            return;
        }

        current.hand.Remove(clickedCard.card);
        pile.Add(clickedCard.card);
        ApplyCardEffect(clickedCard.card, current);
        DrawIfNeeded(current);
        NextTurn();
    }

    void NextTurn()
    {
        UpdatePileText();
        UpdatePileVisuals();
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        StartTurn();
    }

    bool IsCardPlayable(Card card)
    {
        if (pile.Count == 0) return true;
        Card top = pile.Last();

        if (card.value == 3) return true;
        if (card.value == 6) return true;
        if (top.value == 6 && card.suit != top.suit) return false;
        return card.value >= top.value;
    }

    void ApplyCardEffect(Card card, Player player)
    {
        Card top = pile.Count > 1 ? pile[pile.Count - 2] : null;

        if (card.value == 3 && top != null)
        {
            infoText.text = $"{player.name} copie l'effet du {top.value}";
            ApplyCardEffect(top, player);
        }
        else if (card.value == 8)
        {
            infoText.text = "Tour sauté !";
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        }
        else if (card.value == 10)
        {
            infoText.text = $"{player.name} brûle la pile !";
            pile.Clear();
            UpdatePileVisuals();
        }
    }

    void DrawIfNeeded(Player player)
    {
        while (player.hand.Count < 3 && deck.Count > 0)
            DrawCard(player);
    }

    void DrawCard(Player p)
    {
        if (deck.Count > 0)
            p.hand.Add(DrawFromDeck());
    }

    Card DrawFromDeck()
    {
        Card c = deck[0];
        deck.RemoveAt(0);
        UpdatePileVisuals();
        return c;
    }

    void UpdatePileText()
    {
        if (pile.Count > 0)
            pileText.text = "Pile : " + pile.Last();
        else
            pileText.text = "Pile vide";
    }

    void UpdatePileVisuals()
    {
        if (pile.Count > 0)
            discardPilePos.GetComponentInChildren<Text>().text = pile.Last().ToString();
        else
            discardPilePos.GetComponentInChildren<Text>().text = "Vide";

        drawPilePos.GetComponentInChildren<Text>().text = $"{deck.Count} cartes";
        burnPilePos.GetComponentInChildren<Text>().text = "🔥";
    }
}
