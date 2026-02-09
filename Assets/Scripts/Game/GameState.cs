using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameState - Represents the current match state (units, projectiles, buildings, energy)
/// </summary>
[System.Serializable]
public class GameState : MonoBehaviour
{
    [System.Serializable]
    public class PartyState
    {
        public float energy = 5f;
        public float maxEnergy = 10f;
        public float regenRate = 1.0f;
        public List<CardInstance> hand = new List<CardInstance>();
        public List<CardInstance> deck = new List<CardInstance>();
        public int selectedCardIndex = 0;
    }

    [System.Serializable]
    public class CardInstance
    {
        public string cardId;
        public string name;
        public CardType type;
        public int cost;
    }

    public enum CardType { Unit, Pump, Tower }

    public bool isGameOver { get; set; }
    public PartyState playerState { get; set; } = new PartyState();
    public PartyState enemyState { get; set; } = new PartyState();

    public List<Unit> units { get; set; } = new List<Unit>();
    public List<Projectile> projectiles { get; set; } = new List<Projectile>();
    public List<Building> buildings { get; set; } = new List<Building>();

    public Building playerLeftTower { get; set; }
    public Building playerRightTower { get; set; }
    public Unit playerKing { get; set; }

    public Building enemyLeftTower { get; set; }
    public Building enemyRightTower { get; set; }
    public Unit enemyKing { get; set; }

    public void ResetMatch()
    {
        isGameOver = false;
        playerState = new PartyState { energy = 5, maxEnergy = 10, regenRate = 1.0f };
        enemyState = new PartyState { energy = 5, maxEnergy = 10, regenRate = 1.0f };

        units.Clear();
        projectiles.Clear();
        buildings.Clear();
    }

    public float GetEnemyPumpEnergyPerSecond()
    {
        float eps = 0f;
        foreach (var building in buildings)
        {
            if (building.side == "enemy" && building.alive)
            {
                eps += building.energyPerSecond;
            }
        }
        return eps;
    }

    public float GetPlayerPumpEnergyPerSecond()
    {
        float eps = 0f;
        foreach (var building in buildings)
        {
            if (building.side == "player" && building.alive)
            {
                eps += building.energyPerSecond;
            }
        }
        return eps;
    }
}

[System.Serializable]
public class Unit : MonoBehaviour
{
    public string side; // "player" or "enemy"
    public int lane;
    public float hp;
    public float maxHp;
    public float speed;
    public float damage;
    public float attacksPerSecond;
    public float attackRange;
    public float attackCooldown;
    public string kind; // "melee" or "ranged"
    public bool alive = true;
}

[System.Serializable]
public class Building : MonoBehaviour
{
    public string side; // "player" or "enemy"
    public int lane;
    public float hp;
    public float maxHp;
    public float energyPerSecond;
    public bool alive = true;
}

[System.Serializable]
public class Projectile : MonoBehaviour
{
    public string side;
    public Vector2 position;
    public Vector2 velocity;
    public float damage;
    public bool alive = true;
}
