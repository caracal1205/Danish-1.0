using System.Collections.Generic;

public class Player
{
    public string Name;
    public List<Card> hand = new List<Card>();
    public List<Card> visible = new List<Card>();
    public List<Card> hidden = new List<Card>();

    public Player(string name) => Name = name;

    public bool HasNoCards => hand.Count == 0 && visible.Count == 0 && hidden.Count == 0;

    /* (Méthodes commentées de la version originale laissées ici)
    public Card DrawHidden()
    {
        if (hidden.Count == 0) return default;
        var c = hidden[0];
        hidden.RemoveAt(0);
        return c;
    }

    public void AddToHand(List<Card> cards)
    {
        hand.AddRange(cards);
    }

    public void AddToHand(Card c)
    {
        hand.Add(c);
    }*/
}
