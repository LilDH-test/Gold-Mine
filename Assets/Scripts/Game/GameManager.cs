using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameManager — Persistent singleton managing save/load, progression, and owned cards.
/// Dictionary is serialized manually since JsonUtility doesn't support it.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ---- Serializable save wrapper ----
    [System.Serializable]
    private class SavePayload
    {
        public int xp;
        public int gold;
        public float musicVolume;
        public float sfxVolume;
        public List<string> ownedCardIds = new List<string>();
    }

    // ---- Runtime player data ----
    public int xp { get; set; }
    public int gold { get; set; }
    public float musicVolume { get; set; } = 0.6f;
    public float sfxVolume { get; set; } = 0.8f;

    private HashSet<string> ownedCards = new HashSet<string>();

    private const string SAVE_KEY = "goldmine_save_v2";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadGame();
    }

    // ---- Save / Load ----
    public void LoadGame()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            var p = JsonUtility.FromJson<SavePayload>(json);
            xp = p.xp;
            gold = p.gold;
            musicVolume = p.musicVolume;
            sfxVolume = p.sfxVolume;
            ownedCards = new HashSet<string>(p.ownedCardIds);
        }
        else
        {
            xp = 0;
            gold = 0;
            ownedCards = new HashSet<string>();
        }

        // Auto-own basic cards
        foreach (var c in CardDatabase.BasicCards)
            ownedCards.Add(c.id);
    }

    public void SaveGame()
    {
        var p = new SavePayload
        {
            xp = this.xp,
            gold = this.gold,
            musicVolume = this.musicVolume,
            sfxVolume = this.sfxVolume,
            ownedCardIds = new List<string>(ownedCards)
        };
        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(p));
        PlayerPrefs.Save();
    }

    // ---- Level / XP helpers ----
    public int CurrentLevel => GetLevelFromXP(xp);

    public int GetLevelFromXP(int totalXp)
    {
        int level = 1;
        int remaining = totalXp;
        while (remaining >= XpForNextLevel(level) && level < 99)
        {
            remaining -= XpForNextLevel(level);
            level++;
        }
        return level;
    }

    public int XpForNextLevel(int level)
    {
        return Mathf.FloorToInt(120 * Mathf.Pow(level, 1.22f));
    }

    public void GetLevelProgress(out int level, out int xpInto, out int xpNeeded)
    {
        level = 1;
        int remaining = xp;
        int need = XpForNextLevel(level);
        while (remaining >= need && level < 99)
        {
            remaining -= need;
            level++;
            need = XpForNextLevel(level);
        }
        xpInto = remaining;
        xpNeeded = need;
    }

    /// <summary>Award XP/Gold after a match. Returns (xpGain, goldGain).</summary>
    public (int xpGain, int goldGain) AwardMatch(bool won)
    {
        int level = CurrentLevel;
        int xpGain = won ? Mathf.FloorToInt(45 + level * 6) : Mathf.FloorToInt(18 + level * 3);
        int goldGain = won ? Mathf.FloorToInt(60 + level * 10) : 0;
        xp += xpGain;
        gold += goldGain;
        SaveGame();
        return (xpGain, goldGain);
    }

    // ---- Gold helpers ----
    public bool CanAfford(int amount) => gold >= amount;

    public bool SpendGold(int amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        SaveGame();
        return true;
    }

    // ---- Card ownership ----
    public void OwnCard(string cardId)
    {
        ownedCards.Add(cardId);
        SaveGame();
    }

    public bool IsCardOwned(string cardId) => ownedCards.Contains(cardId);

    public List<CardDatabase.CardDef> GetOwnedCards()
    {
        var result = new List<CardDatabase.CardDef>();
        foreach (var c in CardDatabase.AllCards)
        {
            if (ownedCards.Contains(c.id))
                result.Add(c);
        }
        return result;
    }
}
