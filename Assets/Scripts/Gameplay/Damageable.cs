using UnityEngine;

/// <summary>
/// Damageable — Base component for anything with HP (towers, kings, units, pumps).
/// Attach to any entity that can take damage and die.
/// </summary>
public class Damageable : MonoBehaviour
{
    [Header("Health")]
    public float maxHp = 100f;
    public float currentHp;
    public string side; // "player" or "enemy"

    public bool IsAlive => currentHp > 0f;
    public float HpRatio => Mathf.Clamp01(currentHp / maxHp);

    public event System.Action<Damageable> OnDeath;
    public event System.Action<float> OnDamaged;

    public void Init(float hp, string side)
    {
        this.maxHp = hp;
        this.currentHp = hp;
        this.side = side;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        currentHp -= amount;
        OnDamaged?.Invoke(amount);

        if (currentHp <= 0f)
        {
            currentHp = 0f;
            OnDeath?.Invoke(this);
        }
    }
}
