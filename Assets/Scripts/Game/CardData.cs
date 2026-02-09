using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// CardData - Defines all available cards in the game
/// </summary>
[CreateAssetMenu(fileName = "CardData", menuName = "Gold Mine/Card Data")]
public class CardData : ScriptableObject
{
    [System.Serializable]
    public class Card
    {
        public string id;
        public string name;
        public CardType type;
        public int cost;
        public int unlockLevel;
        public int priceGold;

        // Unit stats
        public float hp;
        public float speed;
        public float damage;
        public float attacksPerSecond;
        public float attackRange;
        public float radius;
        public string kind; // "melee" or "ranged"

        // Pump stats
        public float energyPerSecond;
    }

    public enum CardType { Unit, Pump }

    public List<Card> basicCards = new List<Card>();
    public List<Card> unlockCards = new List<Card>();

    public Card GetCard(string cardId)
    {
        foreach (var card in basicCards)
            if (card.id == cardId) return card;

        foreach (var card in unlockCards)
            if (card.id == cardId) return card;

        return null;
    }

    public List<Card> GetCardsAvailableAtLevel(int level)
    {
        var available = new List<Card>(basicCards);
        foreach (var card in unlockCards)
        {
            if (card.unlockLevel <= level)
            {
                available.Add(card);
            }
        }
        return available;
    }
}
