using UnityEngine;

/// <summary>
/// TowerController — Behaviour for base towers and king.
/// Auto-attacks nearest enemy in range using projectiles.
/// </summary>
[RequireComponent(typeof(Damageable))]
public class TowerController : MonoBehaviour
{
    [Header("Tower Config")]
    public string towerKind; // "base" or "king"
    public int lane = -1;    // 0 or 1, king = -1

    [Header("Combat")]
    public float damage = 12f;
    public float attacksPerSecond = 0.9f;
    public float attackRange = 4.5f;

    private float attackCooldown;
    private Damageable damageable;

    public bool IsAlive => damageable != null && damageable.IsAlive;
    public string Side => damageable.side;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
    }

    public void Init(string side, string kind, int lane, float hp, float dmg, float aps, float range, Vector2 pos)
    {
        this.towerKind = kind;
        this.lane = lane;
        this.damage = dmg;
        this.attacksPerSecond = aps;
        this.attackRange = range;
        this.attackCooldown = 0f;

        damageable.Init(hp, side);
        transform.position = new Vector3(pos.x, pos.y, 0f);
    }

    private void Update()
    {
        if (!IsAlive) return;

        attackCooldown = Mathf.Max(0f, attackCooldown - Time.deltaTime);

        if (attackCooldown <= 0f)
        {
            Damageable target = TargetingHelper.FindNearest(transform.position, damageable.side, attackRange);
            if (target != null)
            {
                attackCooldown = 1f / attacksPerSecond;
                MatchController.Instance.SpawnProjectile(
                    damageable.side, transform.position, target, damage);
            }
        }
    }

    /// <summary>Show king as locked/unlocked visually.</summary>
    public void SetKingLocked(bool locked)
    {
        // Adjust sprite alpha or overlay — placeholder for now
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = locked ? 0.35f : 1f;
            sr.color = c;
        }
    }
}
