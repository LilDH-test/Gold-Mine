using System.Collections.Generic;

/// <summary>
/// DeckHelper — Shuffle, deal initial hand, and rotate cards after play.
/// </summary>
public static class DeckHelper
{
    /// <summary>Fisher-Yates shuffle.</summary>
    public static void Shuffle(List<CardDatabase.CardDef> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    /// <summary>Build a shuffled deck for a match.</summary>
    public static List<CardDatabase.CardDef> BuildDeck()
    {
        int level = GameManager.Instance.CurrentLevel;
        bool basicOnly = level == 1;

        List<CardDatabase.CardDef> pool;
        if (basicOnly)
        {
            pool = new List<CardDatabase.CardDef>(CardDatabase.BasicCards);
        }
        else
        {
            pool = GameManager.Instance.GetOwnedCards();
        }

        // Pad to minimum size
        while (pool.Count < GameConstants.MIN_DECK_SIZE)
        {
            pool.AddRange(CardDatabase.BasicCards);
        }

        Shuffle(pool);
        return pool;
    }

    /// <summary>Deal initial hand from the deck.</summary>
    public static void DealHand(GameState.PartyState party, List<CardDatabase.CardDef> deck)
    {
        party.hand.Clear();
        party.deckQueue.Clear();
        party.selectedIndex = 0;

        for (int i = 0; i < GameConstants.HAND_SIZE && deck.Count > 0; i++)
        {
            party.hand.Add(deck[0]);
            deck.RemoveAt(0);
        }

        party.deckQueue.AddRange(deck);
    }

    /// <summary>After playing a card, move it to the back of the deck and draw a new one.</summary>
    public static void RotateHand(GameState.PartyState party)
    {
        if (party.hand.Count == 0 || party.deckQueue.Count == 0) return;

        var played = party.hand[party.selectedIndex];
        party.hand.RemoveAt(party.selectedIndex);

        // Push to back of deck
        party.deckQueue.Add(played);

        // Draw from front
        var next = party.deckQueue[0];
        party.deckQueue.RemoveAt(0);
        party.hand.Add(next);

        // Clamp selection
        if (party.selectedIndex >= party.hand.Count)
            party.selectedIndex = party.hand.Count - 1;
        if (party.selectedIndex < 0)
            party.selectedIndex = 0;
    }
}
