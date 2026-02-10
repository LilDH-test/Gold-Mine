using UnityEngine;

/// <summary>
/// UnitController — Behaviour for spawned units (melee and ranged).
/// Walks toward enemy base, attacks nearest target in range.
/// When both enemy towers are down, drifts toward center to attack the king.
/// </summary>
[RequireComponent(typeof(Damageable))]
public class UnitController : MonoBehaviour
{
    [Header("Unit Config")]
    public string kind; // "melee" or "ranged"
    public int lane;
    public float speed = 90f;
    public float damage = 13f;
    public float attacksPerSecond = 1f;
    public float attackRange = 1f;

    private float attackCooldown;
    private Damageable damageable;

    public bool IsAlive => damageable != null && damageable.IsAlive;
    public string Side => damageable.side;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
    }

    public void Init(string side, int lane, CardDatabase.CardDef card, Vector2 spawnPos)
    {
        this.lane = lane;
        this.kind = card.kind;
        this.speed = GameConstants.PxToWorld(card.speed);
        this.damage = card.damage;
        this.attacksPerSecond = card.attacksPerSecond;
        this.attackRange = GameConstants.PxToWorld(card.attackRange);
        this.attackCooldown = 0f;

        damageable.Init(card.hp, side);
        transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
    }

    private void Update()
    {
        if (!IsAlive) return;

        attackCooldown = Mathf.Max(0f, attackCooldown - Time.deltaTime);

        // Try to find a target in range
        Damageable target = TargetingHelper.FindNearest(transform.position, damageable.side, attackRange);

        if (target != null && target.IsAlive)
        {
            // Attack
            if (attackCooldown <= 0f)
            {
                attackCooldown = 1f / attacksPerSecond;

                if (kind == "ranged")
                {
                    MatchController.Instance.SpawnProjectile(
                        damageable.side, transform.position, target, damage);
                }
                else
                {
                    target.TakeDamage(damage);
                }
            }
            return; // Don't move while attacking
        }

        // Move toward target position
        var mc = MatchController.Instance;
        if (mc == null || mc.State == null) return;
        var state = mc.State;
        string defSide = damageable.side == "player" ? "enemy" : "player";

        bool kingOpen = false;
        try { kingOpen = state.KingUnlocked(defSide); } catch { }

        float targetX = kingOpen ? 0f : GameConstants.LaneX(lane) - GameConstants.ARENA_WIDTH * 0.5f;
        float targetY = damageable.side == "player" ? GameConstants.ENEMY_KING_Y : GameConstants.PLAYER_KING_Y;

        Vector2 pos = transform.position;
        Vector2 goal = new Vector2(targetX, targetY);
        Vector2 dir = (goal - pos);
        float dist = dir.magnitude;

        if (dist > 0.01f)
        {
            dir /= dist;
            pos += dir * speed * Time.deltaTime;

            // Clamp to arena
            float halfW = GameConstants.ARENA_WIDTH * 0.5f;
            float halfH = GameConstants.ARENA_HEIGHT * 0.5f;
            pos.x = Mathf.Clamp(pos.x, -halfW + 0.3f, halfW - 0.3f);
            pos.y = Mathf.Clamp(pos.y, -halfH + 0.3f, halfH - 0.3f);

            transform.position = new Vector3(pos.x, pos.y, 0f);
        }
    }

    private void OnDestroy()
    {
        // Cleanup from tracking lists handled by MatchController
    }
}
