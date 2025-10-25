using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Logique complète du jeu de bataille norvégienne à 2 joueurs (logique console).
/// Attache à un GameObject vide dans Unity.
/// </summary>
public class GameManager : MonoBehaviour
{
    private List<Card> deck = new List<Card>();
    private List<Card> pile = new List<Card>();
    private List<Card> burned = new List<Card>();

    private Player playerA;
    private Player playerB;
    private Player currentPlayer;

    private bool reversedRule = false;
    private bool sameSuitRule = false;
    private Suit restrictedSuit;
    private int minRankRequired;

    void Start()
    {
        StartNewGame();
        SimulateGame();
    }

    public void StartNewGame()
    {
        deck = Deck.CreateStandard52();
        Deck.Shuffle(deck);

        playerA = new Player("Alice");
        playerB = new Player("Bob");

        DealInitialCards(playerA);
        DealInitialCards(playerB);

        pile.Clear();
        burned.Clear();
        reversedRule = false;
        sameSuitRule = false;

        currentPlayer = playerA;

        Debug.Log("Partie de Bataille Norvégienne commencée !");
        Debug.Log($"{playerA.Name} et {playerB.Name} ont leurs cartes.");
    }

    private void DealInitialCards(Player p)
    {
        // 3 cachées, 3 visibles, 7 en main
        p.hidden.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        p.visible.AddRange(deck.Take(3));
        deck.RemoveRange(0, 3);
        p.hand.AddRange(deck.Take(7));
        deck.RemoveRange(0, 7);
    }

    private void SimulateGame()
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
    }

    private void PlayTurn(Player player)
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
            return;
        }

        // pour cette version : on joue la plus faible valide
        var card = validCards.OrderBy(c => (int)c.rank).First();
        player.hand.Remove(card);
        pile.Add(card);
        Debug.Log($"{player.Name} joue {card}");

        ApplyCardEffect(card, player);
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
    }
}
