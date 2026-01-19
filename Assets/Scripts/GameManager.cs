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

    // Références Canvas
    public Transform BotHandPanel;
    public Transform playerHandPanel;
    public Transform drawPilePos;
    public Transform discardPos;
    public Transform burnPilePos;

    // Références Prefabs/UI
    public GameObject[] allCardPrefabs;
    public Text pileText;
    public Text infoText;
    public Button pickupButton;

    // Règles de jeu
    private bool reversedRule = false;
    private bool sameSuitRule = false; 
    private Suit restrictedSuit;
    private int minRankRequired;

    void Awake()
    {
        Debug.Assert(playerHandPanel != null, "PlayerHandPanel manquant");
        Debug.Assert(BotHandPanel != null, "BotHandPanel manquant");
        Debug.Assert(drawPilePos != null, "drawPilePos manquant");
        Debug.Assert(discardPos != null, "discardPos manquant");
        Debug.Assert(pileText != null, "pileText manquant");
        Debug.Assert(infoText != null, "infoText manquant");
    }

    void Start()
    {
        Debug.Log("<color=cyan>--- Initialisation du jeu de Bataille Norvégienne ---</color>");
        StartNewGame(); 
        StartTurn();
    }

    private GameObject GetPrefabForCard(Card card)
    {
        string targetName = $"{card.suit}_{card.rank}";
        foreach (GameObject prefab in allCardPrefabs)
        {
            if(prefab != null && prefab.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase))
            {
                return prefab;
            }
        }
        Debug.LogError($"Prefab pour {targetName} introuvable !");
        return null;
    }

    public void StartNewGame()
    {
        deck = Deck.CreateStandard52();
        Debug.Log($"[Deck] Création du paquet : {deck.Count} cartes.");
        
        Deck.Shuffle(deck);
        Debug.Log("[Deck] Paquet mélangé.");

        players.Clear();
        players.Add(new Player("Ruben"));
        players.Add(new Player("Timothy"));

        foreach (Player p in players)
        {
            DealInitialCards(p);
        }

        pile.Clear();
        burned.Clear();
        reversedRule = false;

        Card firstCard = DrawFromDeck();
        pile.Add(firstCard); 
        Debug.Log($"[Pile] Première carte posée : {firstCard}.");

        UpdatePileText();
        UpdatePileVisuals();
    }

    private void DealInitialCards(Player p)
    {
        // 3 cachées
        p.hidden.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        
        // 3 visibles
        p.visible.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        
        // 7 en main
        p.hand.AddRange(deck.Take(7));
        deck.RemoveRange(0, 7);

        Debug.Log($"[Distribution] {p.Name} a reçu ses cartes (7 en main, 3 visibles, 3 cachées).");
    }

    public void OnCardClicked(CardUI clickedCard)
    {
        Player current = players[currentPlayerIndex];
        
        if (!IsCardPlayable(clickedCard.card))
        {
            Debug.Log($"[Action] {current.Name} a tenté de jouer {clickedCard.card}, mais ce n'est pas permis.");
            infoText.text = "Carte non jouable !";
            return;
        }

        Debug.Log($"[Action] {current.Name} joue : <color=green>{clickedCard.card}</color>");
        
        current.hand.Remove(clickedCard.card);
        pile.Add(clickedCard.card);
        
        ShowPlayerHand(current); 
        ApplyCardEffect(clickedCard.card, current);
        
        if (current.HasNoCards)
        {
            Debug.Log($"<color=yellow>[VICTOIRE] {current.Name} n'a plus de cartes et remporte la partie !</color>");
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

        if (topValue == 7)
        {
            bool res = cardValue <= 7;
            Debug.Log($"[Règle 7] Doit être <= 7. Joué: {cardValue}, Résultat: {res}");
            return res;
        }

        return cardValue >= topValue;
    }

    void ApplyCardEffect(Card card, Player player)
    {
        int cardValue = (int)card.rank;

        switch (cardValue)
        {
            case 3:
                Debug.Log($"[Effet] {player.Name} joue un 3 (Copie).");
                if (pile.Count > 1) 
                {
                    Card previousCard = pile[pile.Count - 2]; 
                    Debug.Log($"[Effet] Le 3 copie l'effet de : {previousCard.rank}");
                    if ((int)previousCard.rank != 3) ApplyCardEffect(previousCard, player); 
                }
                break;

            case 2:
                Debug.Log($"[Effet] {player.Name} joue un 2. La pile est réinitialisée (n'importe quelle carte peut suivre).");
                break;

            case 6:
                reversedRule = !reversedRule;
                Debug.Log($"[Effet] {player.Name} joue un 6. Inversion du sens ! (Inversé = {reversedRule})");
                break;

            case 7:
                Debug.Log($"[Effet] {player.Name} joue un 7. Le prochain joueur doit jouer <= 7.");
                break;

            case 8:
                Debug.Log($"[Effet] {player.Name} joue un 8. Le tour du prochain joueur est sauté !");
                int direction = reversedRule ? -1 : 1;
                currentPlayerIndex = (currentPlayerIndex + direction) % players.Count;
                if (currentPlayerIndex < 0) currentPlayerIndex += players.Count;
                break;

            case 10:
                Debug.Log($"[Effet] {player.Name} joue un 10. LA PILE BRÛLE ({pile.Count} cartes retirées) !");
                burned.AddRange(pile); 
                pile.Clear();
                UpdatePileVisuals();
                // Annule le changement de tour car le joueur rejoue
                currentPlayerIndex = (currentPlayerIndex - (reversedRule ? -1 : 1) + players.Count) % players.Count; 
                break;
        }
    }

    public void PickUpPile(Player player)
    {
        Debug.Log($"[Ramassage] {player.Name} ramasse la pile de {pile.Count} cartes.");
        player.hand.AddRange(pile);
        pile.Clear();
        
        ShowPlayerHand(player);
        UpdatePileVisuals();
        NextTurn(); 
    }

    void DrawIfNeeded(Player player)
    {
        int initialCount = player.hand.Count;
        while (player.hand.Count < 7 && deck.Count > 0) 
        {
            Card drawn = DrawFromDeck();
            player.hand.Add(drawn);
            Debug.Log($"[Pioche] {player.Name} pioche : {drawn}. (Reste dans deck: {deck.Count})");
        }
    }

    Card DrawFromDeck()
    {
        Card c = deck[0];
        deck.RemoveAt(0);
        return c;
    }

    void StartTurn()
    {
        Player current = players[currentPlayerIndex];
        Debug.Log($"<color=white>--- Début du tour : {current.Name} ---</color>");
        infoText.text = $"Tour de : {current.Name}";
        
        ShowPlayerHand(current);
        ShowBotHand(players[(currentPlayerIndex + 1) % players.Count]); 

        pickupButton.onClick.RemoveAllListeners();
        pickupButton.onClick.AddListener(() => PickUpPile(current));
    }

    void NextTurn()
    {
        int direction = reversedRule ? -1 : 1;
        currentPlayerIndex = (currentPlayerIndex + direction) % players.Count;
        if (currentPlayerIndex < 0) currentPlayerIndex += players.Count;

        UpdatePileText();
        UpdatePileVisuals();
        StartTurn();
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
        if (discardPos == null) return;

        foreach (Transform child in discardPos) { Destroy(child.gameObject); }

        if (pile.Count > 0)
        {
            Card topCard = pile.Last();
            GameObject specificPrefab = GetPrefabForCard(topCard);
            if (specificPrefab != null) 
            {
                GameObject cardGO = Instantiate(specificPrefab, discardPos);
                cardGO.transform.localPosition = Vector3.zero;
                cardGO.transform.localRotation = Quaternion.identity;
                cardGO.transform.localScale = Vector3.one;

                CardUI ui = cardGO.GetComponentInChildren<CardUI>();
                if(ui != null) { ui.Setup(topCard, this); ui.DisableInteraction(); }
            }
        }
    }

    void ShowPlayerHand(Player player)
    {
        foreach (Transform child in playerHandPanel) { Destroy(child.gameObject); }

        float spread = 20f; 
        int n = player.hand.Count;
        float startAngle = -spread * (n - 1) / 2;

        for (int i = 0; i < n; i++)
        {
            GameObject specificPrefab = GetPrefabForCard(player.hand[i]);
            if (specificPrefab == null) continue;
            
            GameObject cardGO = Instantiate(specificPrefab, playerHandPanel);
            cardGO.transform.localPosition = Vector3.zero;
            cardGO.transform.localRotation = Quaternion.identity;
            cardGO.transform.localScale = Vector3.one;
            
            CardUI ui = cardGO.GetComponentInChildren<CardUI>();
            if(ui != null) 
            {
                ui.Setup(player.hand[i], this); 
                ui.button.onClick.AddListener(() => OnCardClicked(ui));
            }
        }
    }


    void ShowBotHand(Player bot)
    {
        foreach (Transform child in BotHandPanel) { Destroy(child.gameObject); }
        // Logique visuelle du bot (dos des cartes) peut être ajoutée ici.
    }
}