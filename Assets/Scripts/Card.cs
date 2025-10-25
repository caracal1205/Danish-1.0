// Card.cs
using System;

[Serializable]
public class Card
{
    // correspond à card.value et card.suit utilisés dans GameManager
    public string suit;   // "Hearts", "Diamonds", "Clubs", "Spades"
    public int value;     // 2..14 (11=J,12=Q,13=K,14=A)

    public Card(string suit, int value)
    {
        this.suit = suit;
        this.value = value;
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
