using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameState — Pure data container for the current match (no MonoBehaviour).
/// Entities (units, buildings, projectiles) live as GameObjects in the scene;
/// this class only tracks references and party-level data (energy, hand, deck).
/// </summary>
[System.Serializable]
public class GameState
{
    // ---- Party (player or enemy) energy + hand ----
    [System.Serializable]
    public class PartyState
    {
        public float energy = 5f;
        public float maxEnergy = 10f;
        public float regenRate = 1.0f;
        public List<CardDatabase.CardDef> hand = new List<CardDatabase.CardDef>();
        public List<CardDatabase.CardDef> deckQueue = new List<CardDatabase.CardDef>();
        public int selectedIndex = 0;

        /// <summary>Spend energy if affordable.</summary>
        public bool TrySpend(int cost)
        {
            if (energy < cost) return false;
            energy -= cost;
            return true;
        }

        public void AddEnergy(float amount)
        {
            energy = Mathf.Min(maxEnergy, energy + amount);
        }
    }

    public bool isGameOver;

    public PartyState player = new PartyState();
    public PartyState enemy = new PartyState();

    // AI timer
    public float aiTimer;
    public float aiInterval = 1.75f;

    // ---- References to scene objects (set by MatchController) ----
    public TowerController playerLeftTower;
    public TowerController playerRightTower;
    public TowerController playerKing;

    public TowerController enemyLeftTower;
    public TowerController enemyRightTower;
    public TowerController enemyKing;

    // ---- Reset ----
    public void ResetMatch()
    {
        isGameOver = false;
        player = new PartyState { energy = 5f, maxEnergy = 10f, regenRate = 1.0f };
        enemy = new PartyState { energy = 5f, maxEnergy = 10f, regenRate = 1.0f };
        aiTimer = 0f;
        aiInterval = 1.65f + Random.Range(0f, 0.45f);
    }

    // ---- Tower helpers ----
    public TowerController[] AllPlayerTowers =>
        new[] { playerLeftTower, playerRightTower, playerKing };

    public TowerController[] AllEnemyTowers =>
        new[] { enemyLeftTower, enemyRightTower, enemyKing };

    public bool KingUnlocked(string defenderSide)
    {
        // Use a helper to safely handle destroyed Unity objects
        if (defenderSide == "player")
            return TowerDead(playerLeftTower) && TowerDead(playerRightTower);
        else
            return TowerDead(enemyLeftTower) && TowerDead(enemyRightTower);
    }

    /// <summary>Returns true if tower is null, destroyed, or dead.</summary>
    private static bool TowerDead(TowerController t)
    {
        // ReferenceEquals catches C# null; == null catches Unity "fake null" (destroyed)
        if (ReferenceEquals(t, null) || t == null) return true;
        try { return !t.IsAlive; }
        catch { return true; }
    }
}
