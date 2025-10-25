using System;

[Serializable]
public struct Card
{
    public Suit suit;
    public Rank rank;

    public Card(Suit suit, Rank rank)
    {
        this.suit = suit;
        this.rank = rank;
    }

    public override string ToString()
    {
        // Affiche la carte au format "Rank de Suit"
        return $"{rank} de {suit}";
    }
}

public enum Suit
{
    Clubs, Diamonds, Hearts, Spades
}

public enum Rank
{
    Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, Ace = 14
}
