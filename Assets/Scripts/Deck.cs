using System.Collections.Generic;
using UnityEngine;

public static class Deck
{
    public static List<Card> CreateStandard52()
    {
        var cards = new List<Card>(52);
        foreach (Suit s in System.enum.GetValues(typeof(Suit)))
        {
            foreach (Rank r in System.enum.Getvalues(typeof(Rank)))
                cards.Add(new Card(s, r));
        }
        return cards;
    }
}

public static void Shuffle<T>(List<T> list)
{
    System.Random rng = new System.Random();
    int n = list.Count;
    while(n > 1)
    {
        int k = rng.Next(n-- + 1);
        (list[n], list[k]) =(list[k], list[n]);
        
    }
}
