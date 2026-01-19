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
    public Button pickupButton; // Bouton à lier dans l'Inspector pour "Ramasser la pile"

    // Règles de jeu
    private bool reversedRule = false;
    private bool sameSuitRule = false; // Règle non utilisée dans le Danish standard
    private Suit restrictedSuit;
    private int minRankRequired;

    void Start()
    {
        StartNewGame(); 
        StartTurn();
    }

    private GameObject GetPrefabForCard(Card card)
{
    // This assumes your objects are named like "Hearts_Ace", "Clubs_Two", etc.
    string targetName = $"{card.suit}_{card.rank}";
    
    foreach (GameObject prefab in allCardPrefabs)
    {
        if(prefab != null && prefab.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase))
        {
            return prefab;
        }
    }

    Debug.LogError($"Prefab for {targetName} not found in allCardPrefabs!");
    return null;
}
    public void StartNewGame()
    {
        deck = Deck.CreateStandard52();
        Deck.Shuffle(deck);

        players.Clear(); // Correction : Assurez-vous que la liste est vide
        players.Add(new Player("Ruben"));
        players.Add(new Player("Timothy"));

        // Correction des noms de variables (players[0], players[1])
        DealInitialCards(players[0]); 
        DealInitialCards(players[1]);

        pile.Clear();
        burned.Clear();
        reversedRule = false;
        sameSuitRule = false;

        // Défausser la première carte du jeu (la carte de la pile de jeu)
        pile.Add(DrawFromDeck()); 
        UpdatePileText();
        UpdatePileVisuals(); // Affiche la première carte
    }

    private void DealInitialCards(Player p)
    {
        // 3 cachées, 3 visibles, 7 en main (distribution standard)
        p.hidden.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        p.visible.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        p.hand.AddRange(deck.Take(7)); // 7 cartes en main
        deck.RemoveRange(0, 7);
    }

    // Appelée lorsque le joueur clique sur une carte de sa main
    public void OnCardClicked(CardUI clickedCard)
    {
        Player current = players[currentPlayerIndex];
        
        if (!IsCardPlayable(clickedCard.card))
        {
            infoText.text = "Carte non jouable ! Vous devez ramasser la pile ou jouer une carte valide.";
            return;
        }

        // Retirer la carte de la main du joueur
        current.hand.Remove(clickedCard.card);
        
        // Ajouter à la pile de défausse
        pile.Add(clickedCard.card);
        
        // Rafraîchir la main affichée
        ShowPlayerHand(current); 
        
        // Application de l'effet
        ApplyCardEffect(clickedCard.card, current);
        
        // Vérifier si le joueur a gagné
        if (current.HasNoCards)
        {
            infoText.text = $"{current.Name} a gagné ! Partie terminée.";
            // Ajoutez ici la logique de fin de jeu
            return;
        }

        // Piocher si nécessaire (jusqu'à 7 cartes en main, ou 3 selon la variante)
        DrawIfNeeded(current);
        
        // Passer au tour suivant
        NextTurn();
    }

    bool IsCardPlayable(Card card)
    {
        if (pile.Count == 0) return true; // La pile est vide, n'importe quelle carte est jouable
        
        Card top = pile.Last();

        // Utilisez (int) pour accéder à la valeur numérique de l'énumération Rank
        int cardValue = (int)card.rank;
        int topValue = (int)top.rank;

        // Règle du 2 (Joker) : Toujours jouable
        if (cardValue == 2) return true; 

        // Règle du 3 (Copie) : Toujours jouable (l'effet est géré dans ApplyCardEffect)
        if (cardValue == 3) return true;

        // Règle du 7 (Jeu descendant) : Doit être inférieur ou égal à 7
        if (topValue == 7)
        {
            return cardValue <= 7;
        }

        // Règle générale : la carte jouée doit être supérieure ou égale à la carte du dessus
        return cardValue >= topValue;
    }

    void ApplyCardEffect(Card card, Player player)
    {
        // Utilisez (int) pour accéder à la valeur numérique de l'énumération Rank
        int cardValue = (int)card.rank;

        if (cardValue == 3) 
        {
            if (pile.Count > 1) // Si la pile n'était pas vide avant ce coup
            {
                // Récupère la carte jouée AVANT le 3
                Card previousCard = pile[pile.Count - 2]; 
                infoText.text = $"{player.Name} joue un 3 et copie l'effet du {previousCard.rank} !";
                // Réapplique l'effet de la carte précédente (sans copier à nouveau le 3 !)
                if ((int)previousCard.rank != 3) 
                    ApplyCardEffect(previousCard, player); 
            }
        }
        else if (cardValue == 2) // Le 2 réinitialise le jeu
        {
            infoText.text = $"{player.Name} joue un 2 (Joker)";
        }
        else if (cardValue == 6) // Inverse le sens
        {
            reversedRule = !reversedRule;
            infoText.text = $"Sens du jeu inversé par {player.Name}!";
        }
        else if (cardValue == 7) // Oblige à jouer inférieur ou égal (géré dans IsCardPlayable)
        {
            infoText.text = $"Le prochain joueur doit jouer un 7 ou moins!";
        }
        else if (cardValue == 8) // Tour sauté
        {
            infoText.text = $"{player.Name} joue un 8. Tour sauté !";
            // Passe un joueur de plus
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        }
        else if (cardValue == 10) // Brûle la pile
        {
            infoText.text = $"{player.Name} brûle la pile !";
            // Ajoute toutes les cartes de la pile aux cartes brûlées
            burned.AddRange(pile); 
            pile.Clear();
            UpdatePileVisuals();
            
            // Le joueur qui brûle rejoue (on annule le changement d'index qui va suivre dans NextTurn)
            currentPlayerIndex = (currentPlayerIndex - 1 + players.Count) % players.Count; 
        }
        else if (cardValue == 14) // As : Valeur la plus haute
        {
            infoText.text = $"{player.Name} joue un As.";
        }
    }

    // Logique pour ramasser la pile lorsqu'on ne peut/veut pas jouer
    public void PickUpPile(Player player)
    {
        infoText.text = $"{player.Name} ramasse la pile ({pile.Count} cartes) !";
        
        // Ajouter toutes les cartes de la pile à la main du joueur
        player.hand.AddRange(pile);
        
        // Vider la pile
        pile.Clear();
        
        // Afficher la main mise à jour
        ShowPlayerHand(player);
        
        // Mettre à jour les visuels (la pile est maintenant vide)
        UpdatePileVisuals();
        
        // C'est au joueur suivant de jouer sur une pile vide
        NextTurn(); 
    }

    void DrawIfNeeded(Player player)
    {
        // Pioche si la main a moins de 7 cartes (jusqu'à épuisement de la pioche)
        while (player.hand.Count < 7 && deck.Count > 0) 
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
        // La mise à jour du visuel de la pioche (taille) sera faite dans UpdatePileVisuals
        return c;
    }

    void StartTurn()
    {
        Player current = players[currentPlayerIndex];
        infoText.text = $"Tour de : {current.Name}";
        
        // 1. Afficher la main du joueur (cartes cliquables)
        ShowPlayerHand(current);
        
        // 2. Afficher la main du Bot (dos de carte)
        ShowBotHand(players[(currentPlayerIndex + 1) % players.Count]); 

        // 3. Lier le bouton "Ramasser la pile"
        pickupButton.onClick.RemoveAllListeners();
        pickupButton.onClick.AddListener(() => PickUpPile(current));
    }

    void NextTurn()
    {
        // Inversion du sens de jeu
        int direction = reversedRule ? -1 : 1;
        currentPlayerIndex = (currentPlayerIndex + direction) % players.Count;
        if (currentPlayerIndex < 0) currentPlayerIndex += players.Count; // Gère l'index négatif

        UpdatePileText();
        UpdatePileVisuals();
        StartTurn();
    }

    // Affiche le texte de la carte du dessus pour le debug/info
    void UpdatePileText()
    {
        if (pile.Count > 0)
            pileText.text = "Pile : " + pile.Last();
        else
            pileText.text = "Pile vide";
    }

    // Met à jour les visuels 3D des piles
    void UpdatePileVisuals()
    {
        // 1. Pile de défausse (discardPos)
        foreach (Transform child in discardPos)
            Destroy(child.gameObject);

        if (pile.Count > 0)
        {
            Card topCard = pile.Last();
            
            // Instancier le CardPrefab à la position de défausse
            GameObject specificPrefab = GetPrefabForCard(topCard);
            GameObject cardGO = Instantiate(specificPrefab, discardPos);
            
            // Mettre à jour le CardUI (pour activer le bon modèle 3D)
            CardUI ui = cardGO.GetComponent<CardUI>();
                if(ui != null) {
        ui.Setup(topCard, this);
        ui.DisableInteraction(); 
        }
    
            cardGO.transform.localPosition = Vector3.zero;
            cardGO.transform.localRotation = Quaternion.identity;
        }

        // 2. Pioche (drawPilePos) - Utilise le dos de carte
        foreach (Transform child in drawPilePos)
            Destroy(child.gameObject);

        /*if (deck.Count > 0 && allCardPrefabs.Length > 0)
        {
    // Use the first card in your list as a placeholder
            GameObject cardGO = Instantiate(allCardPrefabs[0], drawPilePos); 

    // Flip the card over so the back face is up
            cardGO.transform.localRotation = Quaternion.Euler(0, 180, 0);
        } */    
    }


    // Affiche la m     ain du joueur (cartes en main)
    void ShowPlayerHand(Player player)
    {
        foreach (Transform child in playerHandPanel)
            Destroy(child.gameObject);

        float spread = 20f; 
        int n = player.hand.Count;
        float startAngle = -spread * (n - 1) / 2;

        for (int i = 0; i < n; i++)
        {
            GameObject specificPrefab = GetPrefabForCard(player.hand[i]);
            GameObject cardGO = Instantiate(specificPrefab, playerHandPanel);
    
            CardUI ui = cardGO.GetComponent<CardUI>();
            if(ui != null) {
                ui.Setup(player.hand[i], this); 
                ui.button.onClick.AddListener(() => OnCardClicked(ui));
            }

            RectTransform rt = cardGO.GetComponent<RectTransform>();
            rt.localRotation = Quaternion.Euler(0, 0, startAngle + spread * i);
        }
    }

    // Affiche la main du Bot (dos de carte pour la main, faces visibles pour les 3 cartes)
    void ShowBotHand(Player bot)
    {
        foreach (Transform child in BotHandPanel)
            Destroy(child.gameObject);
        
    /*   for (int i = 0; i < bot.hand.Count; i++)
        { 
    if (allCardPrefabs.Length > 0)
    {
        GameObject cardGO = Instantiate(allCardPrefabs[0], BotHandPanel); 
        cardGO.transform.localPosition = new Vector3(i * 30, 0, 0);

        // Flip the card
        cardGO.transform.localRotation = Quaternion.Euler(0, 180, 0);

        // Disable interaction so the player can't click bot cards
        CardUI ui = cardGO.GetComponent<CardUI>();
        if (ui != null) ui.DisableInteraction();
        }
    }*/
    }
}
