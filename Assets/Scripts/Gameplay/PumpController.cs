using UnityEngine;

/// <summary>
/// PumpController — Energy pump building. Generates extra energy per second for its side.
/// </summary>
[RequireComponent(typeof(Damageable))]
public class PumpController : MonoBehaviour
{
    [Header("Pump Stats")]
    public float energyPerSecond = 0.5f;

    public int lane;

    private Damageable damageable;
    public bool IsAlive => damageable != null && damageable.IsAlive;
    public string Side => damageable.side;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
    }

    public void Init(string side, int lane, CardDatabase.CardDef card, Vector2 pos)
    {
        this.lane = lane;
        this.energyPerSecond = card.energyPerSecond;

        damageable.Init(card.hp, side);
        transform.position = new Vector3(pos.x, pos.y, 0f);
    }
}
