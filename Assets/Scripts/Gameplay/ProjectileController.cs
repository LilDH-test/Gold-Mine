using UnityEngine;

/// <summary>
/// ProjectileController — Moves toward a target, deals damage on hit, then destroys itself.
/// </summary>
public class ProjectileController : MonoBehaviour
{
    public string side;
    public float damage;
    public float speed = 10f;
    public float radius = 0.08f;

    private Damageable target;
    private Vector2 velocity;
    private bool alive = true;

    public void Init(string side, Vector2 from, Damageable target, float damage)
    {
        this.side = side;
        this.target = target;
        this.damage = damage;
        this.speed = GameConstants.PROJECTILE_SPEED;
        this.radius = GameConstants.PROJECTILE_RADIUS;

        transform.position = new Vector3(from.x, from.y, 0f);

        // Calculate initial velocity toward target
        Vector2 dir = ((Vector2)target.transform.position - from);
        float len = Mathf.Max(0.01f, dir.magnitude);
        velocity = (dir / len) * speed;
        alive = true;
    }

    private void Update()
    {
        if (!alive) return;

        // Move
        Vector2 pos = transform.position;
        pos += velocity * Time.deltaTime;
        transform.position = new Vector3(pos.x, pos.y, 0f);

        // Check if we're out of bounds
        float halfW = GameConstants.ARENA_WIDTH * 0.5f + 2f;
        float halfH = GameConstants.ARENA_HEIGHT * 0.5f + 2f;
        if (Mathf.Abs(pos.x) > halfW || Mathf.Abs(pos.y) > halfH)
        {
            Die();
            return;
        }

        // Check collision with all enemies
        var targets = TargetingHelper.GetAllTargetsFor(side);
        foreach (var t in targets)
        {
            if (!t.IsAlive) continue;
            float dist = Vector2.Distance(pos, (Vector2)t.transform.position);
            float hitDist = radius + 0.3f; // entity radius approx
            if (dist <= hitDist)
            {
                t.TakeDamage(damage);
                Die();
                return;
            }
        }
    }

    private void Die()
    {
        alive = false;
        Destroy(gameObject);
    }
}
