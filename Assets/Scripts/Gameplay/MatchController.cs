using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// MatchController — The main game loop for a match.
/// Manages energy, spawning, win/loss, and coordinates AI + UI.
/// Attach to a persistent GameObject in the Battle scene.
/// </summary>
public class MatchController : MonoBehaviour
{
    public static MatchController Instance { get; private set; }

    [Header("Prefabs (assign in Inspector)")]
    public GameObject towerPrefab;
    public GameObject kingPrefab;
    public GameObject unitPrefab;
    public GameObject pumpPrefab;
    public GameObject projectilePrefab;

    [Header("Sprites (assign in Inspector)")]
    public Sprite playerTowerSprite;
    public Sprite enemyTowerSprite;
    public Sprite playerKingSprite;
    public Sprite enemyKingSprite;
    public Sprite playerUnitSprite;
    public Sprite enemyUnitSprite;
    public Sprite pumpSprite;
    public Sprite projectileSprite;

    [Header("UI")]
    public HUDManager hud;

    [Header("AI")]
    public AIController ai;

    // ---- Runtime ----
    public GameState State { get; private set; }
    private List<UnitController> activeUnits = new List<UnitController>();
    private List<PumpController> activePumps = new List<PumpController>();
    private Transform entityRoot;

    // ---- Events for UI ----
    public event System.Action OnMatchStart;
    public event System.Action<bool> OnMatchEnd; // true = player won

    private void Awake()
    {
        Instance = this;
        State = new GameState();
    }

    // ---- Public API called by MenuUI ----
    public void StartMatch()
    {
        // Clear old entities
        ClearEntities();

        State.ResetMatch();

        // Build decks & deal hands
        var playerDeck = DeckHelper.BuildDeck();
        var enemyDeck = DeckHelper.BuildDeck();
        DeckHelper.DealHand(State.player, playerDeck);
        DeckHelper.DealHand(State.enemy, enemyDeck);

        // Spawn towers & kings
        SpawnBase();

        OnMatchStart?.Invoke();
    }

    // ---- Update loop ----
    private void Update()
    {
        if (State == null || State.isGameOver) return;

        float dt = Time.deltaTime;

        UpdateEnergy(dt);
        ai?.Tick(State, dt);

        // Update king locked state
        UpdateKingLock();

        // Clean dead units & pumps
        CleanDeadEntities();

        // Check game over
        CheckGameOver();

        // Update HUD
        hud?.UpdateHUD(State);
    }

    // ---- Energy ----
    private void UpdateEnergy(float dt)
    {
        // Base regen
        State.player.AddEnergy(State.player.regenRate * dt);
        State.enemy.AddEnergy(State.enemy.regenRate * dt);

        // Pump energy
        float playerPumpEPS = 0f;
        float enemyPumpEPS = 0f;
        foreach (var p in activePumps)
        {
            if (p == null || !p.IsAlive) continue;
            if (p.Side == "player") playerPumpEPS += p.energyPerSecond;
            else enemyPumpEPS += p.energyPerSecond;
        }
        State.player.AddEnergy(playerPumpEPS * dt);
        State.enemy.AddEnergy(enemyPumpEPS * dt);
    }

    // ---- Spawning ----
    private void SpawnBase()
    {
        if (entityRoot != null) Destroy(entityRoot.gameObject);
        entityRoot = new GameObject("Entities").transform;

        // Convert lane positions from game coords to world coords (centered at 0)
        float laneLeftX = GameConstants.LaneX(0) - GameConstants.ARENA_WIDTH * 0.5f;
        float laneRightX = GameConstants.LaneX(1) - GameConstants.ARENA_WIDTH * 0.5f;

        // Player towers
        State.playerLeftTower = SpawnTower("player", "base", 0,
            GameConstants.BASE_TOWER_HP, GameConstants.BASE_TOWER_DMG,
            GameConstants.BASE_TOWER_APS, GameConstants.BASE_TOWER_RANGE,
            new Vector2(laneLeftX, GameConstants.PLAYER_TOWER_Y),
            playerTowerSprite);

        State.playerRightTower = SpawnTower("player", "base", 1,
            GameConstants.BASE_TOWER_HP, GameConstants.BASE_TOWER_DMG,
            GameConstants.BASE_TOWER_APS, GameConstants.BASE_TOWER_RANGE,
            new Vector2(laneRightX, GameConstants.PLAYER_TOWER_Y),
            playerTowerSprite);

        State.playerKing = SpawnTower("player", "king", -1,
            GameConstants.KING_HP, GameConstants.KING_DMG,
            GameConstants.KING_APS, GameConstants.KING_RANGE,
            new Vector2(0f, GameConstants.PLAYER_KING_Y),
            playerKingSprite);

        // Enemy towers
        State.enemyLeftTower = SpawnTower("enemy", "base", 0,
            GameConstants.BASE_TOWER_HP, GameConstants.BASE_TOWER_DMG,
            GameConstants.BASE_TOWER_APS, GameConstants.BASE_TOWER_RANGE,
            new Vector2(laneLeftX, GameConstants.ENEMY_TOWER_Y),
            enemyTowerSprite);

        State.enemyRightTower = SpawnTower("enemy", "base", 1,
            GameConstants.BASE_TOWER_HP, GameConstants.BASE_TOWER_DMG,
            GameConstants.BASE_TOWER_APS, GameConstants.BASE_TOWER_RANGE,
            new Vector2(laneRightX, GameConstants.ENEMY_TOWER_Y),
            enemyTowerSprite);

        State.enemyKing = SpawnTower("enemy", "king", -1,
            GameConstants.KING_HP, GameConstants.KING_DMG,
            GameConstants.KING_APS, GameConstants.KING_RANGE,
            new Vector2(0f, GameConstants.ENEMY_KING_Y),
            enemyKingSprite);

        // Lock kings initially
        State.playerKing.SetKingLocked(true);
        State.enemyKing.SetKingLocked(true);
    }

    private TowerController SpawnTower(string side, string kind, int lane,
        float hp, float dmg, float aps, float range, Vector2 pos, Sprite sprite)
    {
        GameObject prefab = kind == "king" ? kingPrefab : towerPrefab;
        if (prefab == null) prefab = CreateDefaultPrefab();

        var go = Instantiate(prefab, entityRoot);
        go.name = $"{side}_{kind}_L{lane}";

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null && sprite != null) sr.sprite = sprite;

        var tower = go.GetComponent<TowerController>();
        if (tower == null) tower = go.AddComponent<TowerController>();

        var dmgable = go.GetComponent<Damageable>();
        if (dmgable == null) dmgable = go.AddComponent<Damageable>();

        tower.Init(side, kind, lane, hp, dmg, aps, range, pos);
        return tower;
    }

    public void SpawnUnit(string side, int lane, CardDatabase.CardDef card)
    {
        float x = GameConstants.LaneX(lane) - GameConstants.ARENA_WIDTH * 0.5f;
        float y = side == "player" ? GameConstants.PLAYER_SPAWN_Y : GameConstants.ENEMY_SPAWN_Y;

        var go = unitPrefab != null
            ? Instantiate(unitPrefab, entityRoot)
            : Instantiate(CreateDefaultPrefab(), entityRoot);

        go.name = $"{side}_unit_{card.name}";

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = side == "player" ? playerUnitSprite : enemyUnitSprite;
        }

        var unit = go.GetComponent<UnitController>();
        if (unit == null) unit = go.AddComponent<UnitController>();

        var dmgable = go.GetComponent<Damageable>();
        if (dmgable == null) dmgable = go.AddComponent<Damageable>();

        unit.Init(side, lane, card, new Vector2(x, y));

        // Subscribe to death for cleanup
        dmgable.OnDeath += (d) => { if (go != null) Destroy(go, 0.2f); };
        activeUnits.Add(unit);
    }

    public void SpawnPump(string side, int lane, CardDatabase.CardDef card)
    {
        float x = GameConstants.LaneX(lane) - GameConstants.ARENA_WIDTH * 0.5f;
        float y = side == "player" ? GameConstants.PLAYER_PUMP_Y : GameConstants.ENEMY_PUMP_Y;

        var go = pumpPrefab != null
            ? Instantiate(pumpPrefab, entityRoot)
            : Instantiate(CreateDefaultPrefab(), entityRoot);

        go.name = $"{side}_pump_L{lane}";

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null && pumpSprite != null)
        {
            sr.sprite = pumpSprite;
        }

        var pump = go.GetComponent<PumpController>();
        if (pump == null) pump = go.AddComponent<PumpController>();

        var dmgable = go.GetComponent<Damageable>();
        if (dmgable == null) dmgable = go.AddComponent<Damageable>();

        pump.Init(side, lane, card, new Vector2(x, y));
        dmgable.OnDeath += (d) => { if (go != null) Destroy(go, 0.2f); };
        activePumps.Add(pump);
    }

    public void SpawnProjectile(string side, Vector2 from, Damageable target, float damage)
    {
        var go = projectilePrefab != null
            ? Instantiate(projectilePrefab, entityRoot)
            : Instantiate(CreateDefaultPrefab(), entityRoot);

        go.name = "projectile";
        go.transform.localScale = Vector3.one * 0.15f;

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = projectileSprite;
            sr.color = side == "player"
                ? new Color(1f, 0.82f, 0.4f) // gold
                : new Color(1f, 0.42f, 0.42f); // red
        }

        var proj = go.GetComponent<ProjectileController>();
        if (proj == null) proj = go.AddComponent<ProjectileController>();

        proj.Init(side, from, target, damage);
    }

    // ---- Player input ----
    /// <summary>Called when player taps the battlefield. lane 0=left, 1=right.</summary>
    public void PlayerPlayCard(int lane)
    {
        if (State.isGameOver) return;

        var party = State.player;
        if (party.hand.Count == 0) return;

        var card = party.hand[party.selectedIndex];
        if (!party.TrySpend(card.cost)) return;

        if (card.type == CardDatabase.CardType.Unit)
            SpawnUnit("player", lane, card);
        else if (card.type == CardDatabase.CardType.Pump)
            SpawnPump("player", lane, card);

        DeckHelper.RotateHand(party);
        hud?.RefreshHand(State);
    }

    // ---- King lock visuals ----
    private void UpdateKingLock()
    {
        bool playerKingOpen = State.KingUnlocked("player");
        bool enemyKingOpen = State.KingUnlocked("enemy");

        State.playerKing?.SetKingLocked(!playerKingOpen);
        State.enemyKing?.SetKingLocked(!enemyKingOpen);
    }

    // ---- Cleanup ----
    private void CleanDeadEntities()
    {
        activeUnits.RemoveAll(u => u == null || !u.IsAlive);
        activePumps.RemoveAll(p => p == null || !p.IsAlive);
    }

    private void ClearEntities()
    {
        activeUnits.Clear();
        activePumps.Clear();
        if (entityRoot != null)
        {
            Destroy(entityRoot.gameObject);
            entityRoot = null;
        }
    }

    // ---- Game over ----
    private void CheckGameOver()
    {
        bool enemyKingDead = State.enemyKing != null && !State.enemyKing.IsAlive;
        bool playerKingDead = State.playerKing != null && !State.playerKing.IsAlive;

        if (enemyKingDead || playerKingDead)
        {
            State.isGameOver = true;
            bool playerWon = enemyKingDead && !playerKingDead;

            var (xpGain, goldGain) = GameManager.Instance.AwardMatch(playerWon);
            OnMatchEnd?.Invoke(playerWon);
        }
    }

    // ---- Fallback prefab (creates a basic sprite GO at runtime) ----
    private GameObject CreateDefaultPrefab()
    {
        var go = new GameObject("Entity");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = Color.white;
        return go;
    }
}
