using UnityEngine;

/// <summary>
/// HealthBar — Simple world-space HP bar rendered with a SpriteRenderer or LineRenderer.
/// Uses two child quads: background (dark) and fill (colored).
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public Transform background;
    public Transform fill;
    public SpriteRenderer fillRenderer;

    [Header("Config")]
    public Vector2 size = new Vector2(1f, 0.12f);
    public Vector3 offset = new Vector3(0f, -0.6f, 0f);

    private Damageable target;

    public void Init(Damageable target, Color barColor)
    {
        this.target = target;

        if (fillRenderer != null)
            fillRenderer.color = barColor;

        if (background != null)
            background.localScale = new Vector3(size.x, size.y, 1f);

        UpdateBar();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Follow parent
        transform.localPosition = offset;
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (target == null || fill == null) return;

        float ratio = target.HpRatio;
        fill.localScale = new Vector3(size.x * ratio, size.y, 1f);

        // Anchor left
        float xOffset = -(size.x * (1f - ratio)) * 0.5f;
        fill.localPosition = new Vector3(xOffset, 0f, 0f);
    }
}
