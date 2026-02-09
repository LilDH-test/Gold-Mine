using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// CardDatabase — Static card definitions matching the HTML prototype exactly.
/// No ScriptableObject needed; all data is generated from formulas at startup.
/// </summary>
public static class CardDatabase
{
    // ---- Card definition ----
    [System.Serializable]
    public class CardDef
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

    // ---- Pre-built lists ----
    public static readonly List<CardDef> BasicCards;
    public static readonly List<CardDef> UnlockCards;
    public static readonly List<CardDef> AllCards;

    static CardDatabase()
    {
        BasicCards = new List<CardDef>
        {
            MakeUnit("b_m1", "Miner (M1)",   1, "melee"),
            MakeUnit("b_r2", "Slinger (R2)",  2, "ranged"),
            MakeUnit("b_m3", "Guard (M3)",    3, "melee"),
            MakeUnit("b_r4", "Archer (R4)",   4, "ranged"),
            MakeUnit("b_m5", "Bruiser (M5)",  5, "melee"),
            MakeUnit("b_r6", "Rifle (R6)",    6, "ranged"),
        };
        foreach (var c in BasicCards)
        {
            c.unlockLevel = 1;
            c.priceGold = 0;
        }

        UnlockCards = new List<CardDef>
        {
            MakeUnit("u_m7",  "Melee 7",    7, "melee"),
            MakeUnit("u_r7",  "Ranged 7",   7, "ranged"),
            MakeUnit("u_m8",  "Melee 8",    8, "melee"),
            MakeUnit("u_r8",  "Ranged 8",   8, "ranged"),
            MakeUnit("u_m9",  "Melee 9",    9, "melee"),
            MakeUnit("u_r9",  "Ranged 9",   9, "ranged"),
            MakeUnit("u_m10", "Melee 10",  10, "melee"),
            MakeUnit("u_r10", "Ranged 10", 10, "ranged"),

            MakePump("pm_1", "Pump I",   1),
            MakePump("pm_2", "Pump II",  2),
            MakePump("pm_3", "Pump III", 3),
            MakePump("pm_4", "Pump IV",  4),
            MakePump("pm_5", "Pump V",   5),
        };
        for (int i = 0; i < UnlockCards.Count; i++)
        {
            UnlockCards[i].unlockLevel = 2 + i / 2;
            if (UnlockCards[i].type == CardType.Unit)
                UnlockCards[i].priceGold = UnlockCards[i].cost * 85;
        }

        AllCards = new List<CardDef>();
        AllCards.AddRange(BasicCards);
        AllCards.AddRange(UnlockCards);
    }

    // ---- Factory methods (match HTML formulas) ----
    private static CardDef MakeUnit(string id, string name, int cost, string kind)
    {
        bool melee = kind == "melee";
        float hp  = Mathf.Round((melee ? 85f : 70f) + cost * (melee ? 22f : 16f));
        float dmg = (float)System.Math.Round((melee ? 11f : 9f) + cost * (melee ? 2.2f : 1.6f), 1);
        float aps = (float)System.Math.Round((melee ? 1.00f : 1.10f) + cost * 0.03f, 2);
        float spd = Mathf.Round((melee ? 88f : 82f) + cost * (melee ? 1.0f : 0.8f));
        float range = melee ? 18f : 120f;

        return new CardDef
        {
            id = id,
            name = name,
            type = CardType.Unit,
            cost = Mathf.Clamp(cost, 1, 10),
            kind = kind,
            hp = hp,
            speed = spd,
            attackRange = range,
            damage = dmg,
            attacksPerSecond = aps,
            radius = melee ? 12f : 11f,
            energyPerSecond = 0f,
            unlockLevel = 1,
            priceGold = cost * 75,
        };
    }

    private static CardDef MakePump(string id, string name, int tier)
    {
        float hp = Mathf.Round(240 + tier * 60);
        float eps = (float)System.Math.Round(0.45f + tier * 0.10f, 2);
        float raw = hp * 0.010f + eps * 6.0f;
        int cost = Mathf.Clamp(Mathf.RoundToInt(raw), 1, 10);

        return new CardDef
        {
            id = id,
            name = name,
            type = CardType.Pump,
            cost = cost,
            kind = "",
            hp = hp,
            speed = 0f,
            attackRange = 0f,
            damage = 0f,
            attacksPerSecond = 0f,
            radius = 18f,
            energyPerSecond = eps,
            unlockLevel = 1,
            priceGold = cost * 85,
        };
    }

    // ---- Lookup ----
    public static CardDef GetCard(string cardId) =>
        AllCards.FirstOrDefault(c => c.id == cardId);

    public static List<CardDef> GetCardsAvailableAtLevel(int level) =>
        AllCards.Where(c => c.unlockLevel <= level).ToList();
}
