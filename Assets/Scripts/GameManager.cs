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

    [Header("Configuration des Panels")]
    public Transform playerHandPanel; // Panel actif pour le joueur du tour
    public Transform otherPlayerHandPanel; // Panel pour le joueur passif (facultatif)
    public Transform playerVisibleSlots;
    public Transform playerHiddenSlots;
    public Transform drawPilePos;
    public Transform discardPos;
    public Transform burnPilePos;

    [Header("UI & Prefabs")]
    public GameObject[] allCardPrefabs;
    public Text pileText;
    public Text infoText;
    public Text drawText;
    public Button pickupButton;

    private bool reversedRule = false;

    void Awake()
    {
        Debug.Assert(playerHandPanel != null, "playerHandPanel manquant");
        Debug.Assert(drawPilePos != null, "drawPilePos manquant");
        Debug.Assert(discardPos != null, "discardPos manquant");
    }

    void Start()
    {
        StartNewGame(); 
        StartTurn();
    }

    public void StartNewGame()
    {
        deck = Deck.CreateStandard52();
        Deck.Shuffle(deck);

        players.Clear();
        players.Add(new Player("Joueur 1"));
        players.Add(new Player("Joueur 2"));

        foreach (Player p in players)
        {
            DealInitialCards(p);
        }

        pile.Clear();
        burned.Clear();
        reversedRule = false;

        if (deck.Count > 0)
        {
            Card firstCard = DrawFromDeck();
            pile.Add(firstCard);
        }

        UpdatePileVisuals();
    }

    private void DealInitialCards(Player p)
    {
        p.hidden.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        p.visible.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        p.hand.AddRange(deck.Take(7));
        deck.RemoveRange(0, 7);
    }

    private GameObject GetPrefabForCard(Card card)
    {
        string targetName = $"{card.suit}_{card.rank}";
        foreach (GameObject prefab in allCardPrefabs)
        {
            if(prefab != null && prefab.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase))
                return prefab;
        }
        return null;
    }

    public void OnCardClicked(CardUI clickedCard)
    {
        Player current = players[currentPlayerIndex];
        
        if (!IsCardPlayable(clickedCard.card))
        {
            infoText.text = "Action impossible !";
            return;
        }

        current.hand.Remove(clickedCard.card);
        pile.Add(clickedCard.card);

        ApplyCardEffect(clickedCard.card, current);
        
        if (current.HasNoCards)
        {
            infoText.text = $"{current.Name} a gagné !";
            return;
        }

        DrawIfNeeded(current);
        NextTurn();
    }

    bool IsCardPlayable(Card card)
    {
        if (pile.Count == 0) return true;
        Card top = pile.Last();
        int cardValue = (int)card.rank;
        int topValue = (int)top.rank;

        if (cardValue == 2 || cardValue == 3) return true; 
        if (topValue == 7) return cardValue <= 7;
        return cardValue >= topValue;
    }

    void ApplyCardEffect(Card card, Player player)
    {
        int cardValue = (int)card.rank;
        switch (cardValue)
        {
            case 10:
                burned.AddRange(pile); 
                pile.Clear();
                // On reste sur le même joueur pour qu'il rejoue
                currentPlayerIndex = (currentPlayerIndex - (reversedRule ? -1 : 1) + players.Count) % players.Count; 
                break;
            case 8:
                // Saute le prochain tour
                int direction = reversedRule ? -1 : 1;
                currentPlayerIndex = (currentPlayerIndex + direction) % players.Count;
                if (currentPlayerIndex < 0) currentPlayerIndex += players.Count;
                break;
        }
    }

    public void PickUpPile(Player player)
    {
        if (pile.Count == 0) return;
        player.hand.AddRange(pile);
        pile.Clear();
        NextTurn(); 
    }

    void StartTurn()
    {
        Player current = players[currentPlayerIndex];
        Player waiting = players[(currentPlayerIndex + 1) % players.Count];
        
        infoText.text = $"C'est au tour de : {current.Name}";
        
        // Mise à jour de tous les visuels pour le joueur actuel
        ShowPlayerHand(current);
        ShowCardUpSide(current);
        ShowCardDownSide(current);
        ShowDrawPile();
        ShowDiscard();
        
        // Optionnel : afficher le dos des cartes de l'adversaire
        ShowOpponentBacks(waiting);

        pickupButton.onClick.RemoveAllListeners();
        pickupButton.onClick.AddListener(() => PickUpPile(current));
    }

    void NextTurn()
    {
        int direction = reversedRule ? -1 : 1;
        currentPlayerIndex = (currentPlayerIndex + direction) % players.Count;
        if (currentPlayerIndex < 0) currentPlayerIndex += players.Count;

        UpdatePileVisuals();
        StartTurn();
    }

    void UpdatePileVisuals()
    {
        foreach (Transform child in discardPos) { Destroy(child.gameObject); }
        if (pile.Count > 0)
        {
            Card topCard = pile.Last();
            GameObject prefab = GetPrefabForCard(topCard);
            if (prefab != null) 
            {
                GameObject cardGO = Instantiate(prefab, discardPos);
                cardGO.transform.localPosition = Vector3.zero;
                cardGO.transform.localRotation = Quaternion.Euler(0, 180, 0);
                cardGO.transform.localScale = new Vector3(30f, 30f, 30f);
                CardUI ui = cardGO.GetComponentInChildren<CardUI>();
                if(ui != null) { ui.Setup(topCard, this); ui.DisableInteraction(); }
            }
        }
        if (pileText != null) pileText.text = pile.Count > 0 ? $"Pile : {pile.Last()}" : "Pile vide";
    }

    void ShowPlayerHand(Player player)
    {
        foreach (Transform child in playerHandPanel) { Destroy(child.gameObject); }
        int n = player.hand.Count;
        float spacing = 50f;
        float startX = -spacing * (n - 1) / 2;

        for (int i = 0; i < n; i++)
        {
            GameObject prefab = GetPrefabForCard(player.hand[i]);
            if (prefab == null) continue;
            
            GameObject cardGO = Instantiate(prefab, playerHandPanel);
            cardGO.transform.localScale = new Vector3(30f, 30f, 30f); 
            cardGO.transform.localPosition = new Vector3(startX + (i * spacing), 0, i * -0.1f);
            cardGO.transform.localRotation = Quaternion.Euler(0, 180, 0);
            
            CardUI ui = cardGO.GetComponentInChildren<CardUI>();
            if(ui != null) 
            {
                ui.Setup(player.hand[i], this); 
                ui.button.onClick.AddListener(() => OnCardClicked(ui));
            }
        }
    }

    void ShowCardUpSide(Player player)
    {
        foreach (Transform child in playerVisibleSlots) { Destroy(child.gameObject); }
        for (int i = 0; i < player.visible.Count; i++)
        {
            GameObject prefab = GetPrefabForCard(player.visible[i]);
            GameObject cardGO = Instantiate(prefab, playerVisibleSlots);
            cardGO.transform.localPosition = new Vector3(i * 45f, 0, 0);
            cardGO.transform.localRotation = Quaternion.Euler(0, 180, 0);
            cardGO.transform.localScale = new Vector3(30f, 30f, 30f);
        }
    }

    void ShowCardDownSide(Player player)
    {
        foreach (Transform child in playerHiddenSlots) { Destroy(child.gameObject); }
        for (int i = 0; i < player.hidden.Count; i++)
        {
            GameObject prefab = GetPrefabForCard(player.hidden[i]);
            GameObject cardGO = Instantiate(prefab, playerHiddenSlots);
            cardGO.transform.localPosition = new Vector3(i * 45f, 0, 0);
            cardGO.transform.localRotation = Quaternion.Euler(0, 0, 0); // Face cachée
            cardGO.transform.localScale = new Vector3(30f, 30f, 30f);
        }
    }

    void ShowDrawPile()
    {
        foreach (Transform child in drawPilePos) { Destroy(child.gameObject); }
        if (drawText != null) drawText.text = $"Pioche : {deck.Count}";
        
        int visibleCount = Mathf.Min(deck.Count, 5); // On n'affiche que les 5 dernières pour les perfs
        for (int i = 0; i < visibleCount; i++)
        {
            GameObject prefab = GetPrefabForCard(deck[i]);
            GameObject cardGO = Instantiate(prefab, drawPilePos);
            cardGO.transform.localScale = new Vector3(30f, 30f, 30f); 
            cardGO.transform.localPosition = new Vector3(0, i * 0.5f, 0);
            cardGO.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    void ShowDiscard()
    {
        foreach (Transform child in burnPilePos) { Destroy(child.gameObject); }
        if (burned.Count == 0) return;

        Card lastBurned = burned.Last();
        GameObject prefab = GetPrefabForCard(lastBurned);
        if (prefab != null)
        {
            GameObject cardGO = Instantiate(prefab, burnPilePos);
            cardGO.transform.localScale = new Vector3(30f, 30f, 30f);
            cardGO.transform.localPosition = Vector3.zero;
            cardGO.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
    }

    void ShowOpponentBacks(Player opponent)
    {
        if (otherPlayerHandPanel == null) return;
        foreach (Transform child in otherPlayerHandPanel) { Destroy(child.gameObject); }
        // Ici on pourrait instancier des prefabs de dos de cartes pour l'immersion
    }

    void DrawIfNeeded(Player player)
    {
        while (player.hand.Count < 7 && deck.Count > 0) 
            player.hand.Add(DrawFromDeck());
    }

    Card DrawFromDeck()
    {
        Card c = deck[0];
        deck.RemoveAt(0);
        return c;
    }
}