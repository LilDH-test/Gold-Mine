using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameManager - Core singleton managing game state, progression, and flow
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [System.Serializable]
    public class PlayerData
    {
        public int level = 1;
        public int xp = 0;
        public int gold = 0;
        public Dictionary<string, bool> ownedCards = new Dictionary<string, bool>();
        public GameSettings settings = new GameSettings();
    }

    [System.Serializable]
    public class GameSettings
    {
        public float musicVolume = 0.6f;
        public float sfxVolume = 0.8f;
    }

    public PlayerData playerData { get; private set; }
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

    public void LoadGame()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json))
        {
            playerData = new PlayerData();
        }
        else
        {
            playerData = JsonUtility.FromJson<PlayerData>(json);
        }
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public int GetLevelFromXP(int xp)
    {
        int level = 1;
        int remaining = xp;
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

    public void AwardMatch(bool won)
    {
        int level = GetLevelFromXP(playerData.xp);
        int xpGain = won ? Mathf.FloorToInt(45 + level * 6) : Mathf.FloorToInt(18 + level * 3);
        int goldGain = won ? Mathf.FloorToInt(60 + level * 10) : 0;

        playerData.xp += xpGain;
        playerData.gold += goldGain;
        SaveGame();
    }

    public void AddGold(int amount)
    {
        playerData.gold += amount;
        SaveGame();
    }

    public void SpendGold(int amount)
    {
        playerData.gold -= Mathf.Max(0, amount);
        SaveGame();
    }

    public void OwnCard(string cardId)
    {
        if (!playerData.ownedCards.ContainsKey(cardId))
        {
            playerData.ownedCards[cardId] = true;
            SaveGame();
        }
    }

    public bool IsCardOwned(string cardId)
    {
        return playerData.ownedCards.ContainsKey(cardId) && playerData.ownedCards[cardId];
    }
}
