// Player.cs
using System.Collections.Generic;

[System.Serializable]
public class Player
{
    // correspond à player.name et aux listes hand, visible, hidden
    public string name;
    public List<Card> hand = new List<Card>();
    public List<Card> visible = new List<Card>();
    public List<Card> hidden = new List<Card>();

    public Player(string name)
    {
        this.name = name;
    }

    public bool HasNoCards => hand.Count == 0 && visible.Count == 0 && hidden.Count == 0;
}
