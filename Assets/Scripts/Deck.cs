using System.Collections.Generic;
using UnityEngine;

public static class Deck
{
    /// <summary>
    /// Crée un jeu standard de 52 cartes (13 valeurs × 4 couleurs)
    /// </summary>
    public static List<Card> CreateStandard52()
    {
        var cards = new List<Card>(52);

        foreach (Suit s in System.Enum.GetValues(typeof(Suit)))
        {
            for (int v = 1; v <= 13; v++) // 1 = As, 13 = Roi
            {
                cards.Add(new Card(v, s));
            }
        }

        return cards;
    }

    /// <summary>
    /// Mélange une liste (Fisher–Yates shuffle)
    /// </summary>
    public static void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
