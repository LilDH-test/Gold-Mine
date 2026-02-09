using UnityEngine;

/// <summary>
/// ArenaRenderer — Draws the battlefield background matching the HTML prototype:
/// Dark board with tile grid, river band, two bridges, lane divider, spawn zone glows.
/// Attach to a GameObject with a SpriteRenderer or use as a standalone renderer.
/// </summary>
[ExecuteAlways]
public class ArenaRenderer : MonoBehaviour
{
    [Header("Arena Sizing")]
    public float arenaWidth = 10f;
    public float arenaHeight = 20f;

    [Header("Colors")]
    public Color boardColor1 = new Color(0.078f, 0.094f, 0.157f, 0.95f);  // #141828
    public Color boardColor2 = new Color(0.039f, 0.047f, 0.071f, 0.98f);  // #0a0c12
    public Color gridColor = new Color(1f, 1f, 1f, 0.04f);
    public Color riverColor = new Color(0.157f, 0.431f, 0.863f, 0.25f);   // blue
    public Color riverHighlight = new Color(1f, 1f, 1f, 0.10f);
    public Color bridgeColor = new Color(1f, 0.82f, 0.4f, 0.14f);         // gold tint
    public Color bridgeOutline = new Color(1f, 1f, 1f, 0.12f);
    public Color laneDivider = new Color(1f, 0.82f, 0.4f, 0.08f);
    public Color spawnGlow = new Color(1f, 0.82f, 0.4f, 0.08f);
    public Color boardOutline = new Color(1f, 1f, 1f, 0.10f);

    [Header("Dimensions")]
    public float padding = 0.27f;
    public float tileSize = 0.92f;     // world units per grid cell
    public float riverHeight = 1.4f;
    public float bridgeWidth = 2.6f;

    private Texture2D arenaTex;
    private SpriteRenderer sr;

    private void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -100;
        GenerateArena();
    }

    private void OnValidate()
    {
        GenerateArena();
    }

    public void GenerateArena()
    {
        int texW = 512;
        int texH = 1024;

        if (arenaTex == null || arenaTex.width != texW || arenaTex.height != texH)
        {
            arenaTex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
            arenaTex.filterMode = FilterMode.Point;
            arenaTex.wrapMode = TextureWrapMode.Clamp;
        }

        Color[] pixels = new Color[texW * texH];

        // Fill with dark vignette background
        for (int y = 0; y < texH; y++)
        {
            for (int x = 0; x < texW; x++)
            {
                float nx = (float)x / texW;
                float ny = (float)y / texH;
                float dist = Mathf.Sqrt((nx - 0.5f) * (nx - 0.5f) + (ny - 0.5f) * (ny - 0.5f));

                Color bg = Color.Lerp(
                    new Color(0.063f, 0.071f, 0.149f), // #101226
                    new Color(0.012f, 0.016f, 0.039f), // #03040a
                    Mathf.Clamp01(dist * 1.6f)
                );
                pixels[y * texW + x] = bg;
            }
        }

        // Board area
        int padX = (int)(padding / arenaWidth * texW);
        int padY = (int)(padding / arenaHeight * texH);
        int bx0 = padX, by0 = padY;
        int bw = texW - padX * 2, bh = texH - padY * 2;

        for (int y = by0; y < by0 + bh; y++)
        {
            for (int x = bx0; x < bx0 + bw; x++)
            {
                float t = (float)(y - by0) / bh;
                Color board = Color.Lerp(boardColor1, boardColor2, t);
                pixels[y * texW + x] = board;
            }
        }

        // Board outline
        DrawRectOutline(pixels, texW, texH, bx0, by0, bw, bh, boardOutline);

        // Tile grid
        float gridStepX = tileSize / arenaWidth * texW;
        float gridStepY = tileSize / arenaHeight * texH;

        for (float gx = bx0 + gridStepX; gx < bx0 + bw - 4; gx += gridStepX)
        {
            int ix = Mathf.RoundToInt(gx);
            for (int y = by0 + 4; y < by0 + bh - 4; y++)
                pixels[y * texW + ix] = BlendColor(pixels[y * texW + ix], gridColor);
        }
        for (float gy = by0 + gridStepY; gy < by0 + bh - 4; gy += gridStepY)
        {
            int iy = Mathf.RoundToInt(gy);
            for (int x = bx0 + 4; x < bx0 + bw - 4; x++)
                pixels[iy * texW + x] = BlendColor(pixels[iy * texW + x], gridColor);
        }

        // River band at center
        int midY = texH / 2;
        int rH = (int)(riverHeight / arenaHeight * texH);
        int ry0 = midY - rH / 2;
        int ry1 = midY + rH / 2;

        for (int y = ry0; y < ry1; y++)
        {
            float t = (float)(y - ry0) / (ry1 - ry0);
            float alpha = 0.22f + 0.08f * Mathf.Sin(t * Mathf.PI);
            Color riv = new Color(riverColor.r, riverColor.g, riverColor.b, alpha);
            for (int x = bx0 + 4; x < bx0 + bw - 4; x++)
                pixels[y * texW + x] = BlendColor(pixels[y * texW + x], riv);
        }

        // River highlight line
        for (int x = bx0 + 4; x < bx0 + bw - 4; x++)
        {
            pixels[midY * texW + x] = BlendColor(pixels[midY * texW + x], riverHighlight);
            if (midY + 1 < texH)
                pixels[(midY + 1) * texW + x] = BlendColor(pixels[(midY + 1) * texW + x],
                    new Color(riverHighlight.r, riverHighlight.g, riverHighlight.b, riverHighlight.a * 0.5f));
        }

        // Bridges at lane positions
        float lane0Frac = 0.28f;
        float lane1Frac = 0.72f;
        int bridgeTexW = (int)(bridgeWidth / arenaWidth * texW);
        int bridgeTexH = rH + (int)(0.6f / arenaHeight * texH);

        DrawBridge(pixels, texW, texH, (int)(lane0Frac * texW), midY, bridgeTexW, bridgeTexH);
        DrawBridge(pixels, texW, texH, (int)(lane1Frac * texW), midY, bridgeTexW, bridgeTexH);

        // Lane divider (subtle vertical line at center)
        int centerX = texW / 2;
        for (int y = by0 + 4; y < by0 + bh - 4; y++)
        {
            pixels[y * texW + centerX] = BlendColor(pixels[y * texW + centerX], laneDivider);
            if (centerX + 1 < texW)
                pixels[y * texW + centerX + 1] = BlendColor(pixels[y * texW + centerX + 1],
                    new Color(laneDivider.r, laneDivider.g, laneDivider.b, laneDivider.a * 0.5f));
        }

        // Spawn zone glows (top and bottom)
        DrawSpawnGlow(pixels, texW, texH, texW / 2, (int)(texH * 0.14f), spawnGlow);
        DrawSpawnGlow(pixels, texW, texH, texW / 2, (int)(texH * 0.86f), spawnGlow);

        // Pump zone markers (subtle circles)
        float pumpPlayerY = 0.74f;
        float pumpEnemyY = 0.26f;
        DrawZoneCircle(pixels, texW, texH, (int)(lane0Frac * texW), (int)(pumpPlayerY * texH), 18, new Color(1f, 0.82f, 0.4f, 0.06f));
        DrawZoneCircle(pixels, texW, texH, (int)(lane1Frac * texW), (int)(pumpPlayerY * texH), 18, new Color(1f, 0.82f, 0.4f, 0.06f));
        DrawZoneCircle(pixels, texW, texH, (int)(lane0Frac * texW), (int)(pumpEnemyY * texH), 18, new Color(1f, 0.42f, 0.42f, 0.05f));
        DrawZoneCircle(pixels, texW, texH, (int)(lane1Frac * texW), (int)(pumpEnemyY * texH), 18, new Color(1f, 0.42f, 0.42f, 0.05f));

        arenaTex.SetPixels(pixels);
        arenaTex.Apply();

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = Sprite.Create(arenaTex,
                new Rect(0, 0, texW, texH),
                new Vector2(0.5f, 0.5f),
                texW / arenaWidth);
        }
    }

    // ---- Drawing helpers ----

    void DrawBridge(Color[] px, int w, int h, int cx, int cy, int bw, int bh)
    {
        int bx0 = cx - bw / 2;
        int by0 = cy - bh / 2;

        // Fill
        for (int y = by0; y < by0 + bh; y++)
        {
            if (y < 0 || y >= h) continue;
            float t = (float)(y - by0) / bh;
            float alpha = Mathf.Lerp(0.18f, 0.25f, t);
            Color bc = new Color(bridgeColor.r, bridgeColor.g, bridgeColor.b, alpha);
            for (int x = bx0; x < bx0 + bw; x++)
            {
                if (x < 0 || x >= w) continue;
                px[y * w + x] = BlendColor(px[y * w + x], bc);
            }
        }

        // Outline
        DrawRectOutline(px, w, h, bx0, by0, bw, bh, bridgeOutline);

        // Planks
        int plankCount = 7;
        for (int i = 0; i < plankCount; i++)
        {
            int py = by0 + 3 + (int)(i * ((bh - 6f) / (plankCount - 1)));
            if (py < 0 || py >= h) continue;
            Color plankColor = new Color(1f, 1f, 1f, 0.06f);
            for (int x = bx0 + 3; x < bx0 + bw - 3; x++)
            {
                if (x < 0 || x >= w) continue;
                px[py * w + x] = BlendColor(px[py * w + x], plankColor);
            }
        }
    }

    void DrawSpawnGlow(Color[] px, int w, int h, int cx, int cy, Color glowColor)
    {
        int radius = (int)(w * 0.45f);
        for (int y = cy - radius; y < cy + radius; y++)
        {
            if (y < 0 || y >= h) continue;
            for (int x = cx - radius; x < cx + radius; x++)
            {
                if (x < 0 || x >= w) continue;
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float t = 1f - Mathf.Clamp01(dist / radius);
                Color g = new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a * t * t);
                px[y * w + x] = BlendColor(px[y * w + x], g);
            }
        }
    }

    void DrawZoneCircle(Color[] px, int w, int h, int cx, int cy, int radius, Color color)
    {
        // Circle outline only
        for (int y = cy - radius - 1; y <= cy + radius + 1; y++)
        {
            if (y < 0 || y >= h) continue;
            for (int x = cx - radius - 1; x <= cx + radius + 1; x++)
            {
                if (x < 0 || x >= w) continue;
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (Mathf.Abs(dist - radius) < 1.5f)
                {
                    float edge = 1f - Mathf.Clamp01(Mathf.Abs(dist - radius));
                    Color c = new Color(color.r, color.g, color.b, color.a * edge);
                    px[y * w + x] = BlendColor(px[y * w + x], c);
                }
            }
        }
    }

    void DrawRectOutline(Color[] px, int w, int h, int rx, int ry, int rw, int rh, Color color)
    {
        for (int x = rx; x < rx + rw; x++)
        {
            if (x < 0 || x >= w) continue;
            if (ry >= 0 && ry < h) px[ry * w + x] = BlendColor(px[ry * w + x], color);
            int bot = ry + rh - 1;
            if (bot >= 0 && bot < h) px[bot * w + x] = BlendColor(px[bot * w + x], color);
        }
        for (int y = ry; y < ry + rh; y++)
        {
            if (y < 0 || y >= h) continue;
            if (rx >= 0 && rx < w) px[y * w + rx] = BlendColor(px[y * w + rx], color);
            int rt = rx + rw - 1;
            if (rt >= 0 && rt < w) px[y * w + rt] = BlendColor(px[y * w + rt], color);
        }
    }

    static Color BlendColor(Color dst, Color src)
    {
        float a = src.a;
        float ia = 1f - a;
        return new Color(
            dst.r * ia + src.r * a,
            dst.g * ia + src.g * a,
            dst.b * ia + src.b * a,
            Mathf.Clamp01(dst.a + a * (1f - dst.a))
        );
    }
}
