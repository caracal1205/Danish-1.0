using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Nécessaire pour les opérations de listes (non utilisé ici, mais bonne pratique)

public static class Deck
{
    public static List<Card> CreateStandard52()
    {
        var cards = new List<Card>(52);
        // Correction de la casse : System.Enum
        foreach (Suit s in System.Enum.GetValues(typeof(Suit))) 
        {
            // Correction de la casse : System.Enum
            foreach (Rank r in System.Enum.GetValues(typeof(Rank))) 
                cards.Add(new Card(s, r));
        }
        return cards;
    }

    public static void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while(n > 1)
        {
            // Correction de l'index aléatoire (entre 0 et n-1)
            int k = rng.Next(n--); 
            
            // Échange des éléments
            (list[n], list[k]) =(list[k], list[n]);
        }
    }
}
