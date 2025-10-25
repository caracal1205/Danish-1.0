using System;

[Serializable]
public class Card
{
    public int value;  // 2..14
    public Suit suit;  // type enum

    public Card(int value, Suit suit)
    {
        this.value = value;
        this.suit = suit;
    }

    public override string ToString()
    {
        string rankStr;
        switch (value)
        {
            case 11: rankStr = "J"; break;
            case 12: rankStr = "Q"; break;
            case 13: rankStr = "K"; break;
            case 14: rankStr = "A"; break;
            default: rankStr = value.ToString(); break;
        }
        return $"{rankStr} of {suit}";
    }
}

public enum Suit
{
    Hearts,    // ♥
    Diamonds,  // ♦
    Clubs,     // ♣
    Spades     // ♠
}
