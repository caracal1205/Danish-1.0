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

    public Transform BotHandPanel;
    public Transform playerHandPanel
    public Transform drawPilePos;
    public Transform discardPos;
    public Transform burnPilePos;

    public GameObject cardPrefab;
    public Text pileText;
    public text infoText;

    private bool reversedRule = false;
    private bool sameSuitRule = false;
    private Suit restrictedSuit;
    private int minRankRequired;

    void Start()
    {
        StartNewGame(); 
        //SimulateGame();
        StartTurn();
    }

    public void StartNewGame()
    {
        deck = Deck.CreateStandard52();
        Deck.Shuffle(deck);

        player.Add(new Player("Ruben"));
        player.Add(new Player("Timothy"));

        DealInitialCards(playerA);
        DealInitialCards(playerB);

        pile.Clear();
        burned.Clear();
        reversedRule = false;
        sameSuitRule = false;

        pile.Add(DrawFromDeck());
        UpdatePileText();


        /*currentPlayer = playerA;

        Debug.Log("Partie de Bataille Norvégienne commencée !");
        Debug.Log($"{playerA.Name} et {playerB.Name} ont leurs cartes.");*/
    }

    private void DealInitialCards(Player p)
    {
        // 3 cachées, 3 visibles, 7 en main
        p.hidden.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        p.visible.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        p.hand.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
    }

    /*private void SimulateGame()
    {
        int safety = 0;
        while (!playerA.HasNoCards && !playerB.HasNoCards && safety < 500)
        {
            PlayTurn(currentPlayer);
            currentPlayer = (currentPlayer == playerA) ? playerB : playerA;
            safety++;
        }

        if (playerA.HasNoCards) Debug.Log("🎉 Alice gagne !");
        else if (playerB.HasNoCards) Debug.Log("🎉 Bob gagne !");
        else Debug.Log("Fin forcée après 500 tours (sécurité).");
    }*/

    void StartTurn()
    {
        Player current = player[currentPlayerIndex];
        infoText.text = $"Tour de : {current.name}";

    }

    void ShowPlayerHand(Player player)
    {
    foreach (Transform child in playerHandPanel)
        Destroy(child.gameObject);

    float spread = 20f; // angle d’éventail
    int n = player.hand.Count;
    float startAngle = -spread * (n - 1) / 2;

    for (int i = 0; i < n; i++)
    {
        GameObject cardGO = Instantiate(cardPrefab, playerHandPanel);
        CardUI ui = cardGO.GetComponent<CardUI>();
        ui.Setup(player.hand[i]);

        // Positionner en éventail
        RectTransform rt = cardGO.GetComponent<RectTransform>();
        rt.localRotation = Quaternion.Euler(0, 0, startAngle + spread * i);
    }
    }

    public void OnCardClicked(CardUI clickedCard)
    {
        Player current = players[currentPlayer];
        if (!IsCardPlayable(clickedCard.card))
        {
            infoText.text = "Carte non jouable fdp";
            return;
        }

        current.hand.Remove(clickedCard.card);
        pile.Add(clickedCard.card);
        ApplyCardEffect(clickedCard.card, current);
        DrawIfNeeded(current);
        NextTurn();
    }

    void NextTrun()
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

        // Règles de la bataille norvégienne avec tes ajouts
        if (card.value == 3) return true; // le 3 copie
        if (card.value == 6) return true; // restriction gérée après
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
        // Discard (pile centrale)
        if (pile.Count > 0)
            discardPilePos.GetComponentInChildren<Text>().text = pile.Last().ToString();
        else
            discardPilePos.GetComponentInChildren<Text>().text = "Vide";

        // Draw pile
        drawPilePos.GetComponentInChildren<Text>().text = $"{deck.Count} cartes";

        // Burn pile (optionnel)
        burnPilePos.GetComponentInChildren<Text>().text = "🔥";
    }
}

/*    private void PlayTurn(Player player)
    {
        // choix d’une carte valide (simulation simple)
        var validCards = GetPlayableCards(player);
        if (validCards.Count == 0)
        {
            Debug.Log($"{player.Name} ne peut pas jouer, il ramasse la pile ({pile.Count} cartes).");
            player.AddToHand(pile);
            pile.Clear();
            reversedRule = false;
            sameSuitRule = false;
            DrawIfNeeded(player);
            return;
        }

        // pour cette version : on joue la plus faible valide
        var card = validCards.OrderBy(c => (int)c.rank).First();
        player.hand.Remove(card);
        pile.Add(card);
        Debug.Log($"{player.Name} joue {card}");

        ApplyCardEffect(card, player);
        DrawIfNeeded(player);
    }

    private void DrawIfNeeded(Player player)
    {
        // Tant que le joueur a moins de 3 cartes et que le deck n’est pas vide
        while (player.hand.Count < 3 && deck.Count > 0)
        {
            var drawn = deck[0];
            deck.RemoveAt(0);
            player.hand.Add(drawn);
            Debug.Log($"{player.Name} pioche {drawn}");
        }
    }
    private List<Card> GetPlayableCards(Player player)
    {
        if (pile.Count == 0)
            return new List<Card>(player.hand);

        Card top = pile.Last();
        var valid = new List<Card>();

        foreach (var c in player.hand)
        {
            if (IsCardPlayable(c, top))
                valid.Add(c);
        }

        return valid;
    }

    private bool IsCardPlayable(Card played, Card top)
    {
        if (sameSuitRule)
            return played.suit == restrictedSuit && (int)played.rank >= minRankRequired;

        int topValue = (int)top.rank;
        int playValue = (int)played.rank;

        if (reversedRule)
            return playValue <= topValue;
        else
            return playValue >= topValue || played.rank == Rank.Two || played.rank == Rank.Ten;
    }

    public void OnCardClicked(CardUI clickedCard)
    {
        if (!IsCardPlayable(clickedCard.card, pile.Last()))
        {
            Debug.Log("Carte non jouable !");
            return;
        }

        currentPlayer.hand.Remove(clickedCard.card);
        pile.Add(clickedCard.card);
        ApplyCardEffect(clickedCard.card, currentPlayer);
        DrawIfNeeded(currentPlayer);
        NextPlayer();
    }


    private void ApplyCardEffect(Card card, Player player)
    {
        reversedRule = false;
        sameSuitRule = false;

        switch (card.rank)
        {
            case Rank.Two:
                Debug.Log("Effet 2️⃣ : la pile est réinitialisée.");
                pile.Clear();
                break;

            case Rank.Three:
                CopyUnderEffect();
                break;

            case Rank.Six:
                Debug.Log("Effet 6️⃣ : même couleur et au-dessus requis au prochain tour.");
                sameSuitRule = true;
                restrictedSuit = card.suit;
                minRankRequired = (int)Rank.Six + 1;
                break;

            case Rank.Seven:
                Debug.Log("Effet 7️⃣ : la règle s'inverse (cartes inférieures ou égales).");
                reversedRule = true;
                break;

            case Rank.Eight:
                Debug.Log("Effet 8️⃣ : le joueur suivant passe son tour !");
                SkipNextPlayer();
                break;

            case Rank.Ten:
                Debug.Log("Effet 🔥10 : la pile est brûlée, " + player.Name + " rejoue !");
                burned.AddRange(pile);
                pile.Clear();
                PlayTurn(player); // rejoue immédiatement
                break;

            default:
                // rien de spécial
                break;
        }
    }

    private void CopyUnderEffect()
    {
        if (pile.Count < 2) return;

        // Cherche la première carte non-3 sous le ou les 3
        for (int i = pile.Count - 2; i >= 0; i--)
        {
            var under = pile[i];
            if (under.rank != Rank.Three)
            {
                Debug.Log($"Effet 3️⃣ : copie de {under}");
                ApplyCardEffect(under, currentPlayer);
                break;
            }
        }
    }

    private void SkipNextPlayer()
    {
        // Inverser le joueur deux fois revient au même que passer un tour
        currentPlayer = (currentPlayer == playerA) ? playerB : playerA;
        Debug.Log($"Le tour de {currentPlayer.Name} est sauté !");
        currentPlayer = (currentPlayer == playerA) ? playerB : playerA;
     }*/

