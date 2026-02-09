using UnityEngine;

/// <summary>
/// PixelSpriteGenerator — Generates 8-bit pixel art sprites at runtime
/// matching the HTML prototype exactly (towers, kings, units, pumps, projectiles).
/// Call the static methods to create Sprite objects.
/// </summary>
public static class PixelSpriteGenerator
{
    private const int SIZE = 24; // 24x24 pixel sprites (scaled up in-game)

    /// <summary>Create player tower sprite (gold castle tower)</summary>
    public static Sprite PlayerTower() => MakeTower(Hex("#3a2b12"), Hex("#ffd166"));

    /// <summary>Create enemy tower sprite (red castle tower)</summary>
    public static Sprite EnemyTower() => MakeTower(Hex("#2b1212"), Hex("#ff6b6b"));

    /// <summary>Create player king sprite (gold crown)</summary>
    public static Sprite PlayerKing() => MakeKing(Hex("#3a2b12"), Hex("#f6b73c"));

    /// <summary>Create enemy king sprite (red crown)</summary>
    public static Sprite EnemyKing() => MakeKing(Hex("#2b1212"), Hex("#ff9f7a"));

    /// <summary>Create player unit sprite (green soldier)</summary>
    public static Sprite PlayerUnit() => MakeUnit(Hex("#3a2b12"), Hex("#6ee7a8"));

    /// <summary>Create enemy unit sprite (red soldier)</summary>
    public static Sprite EnemyUnit() => MakeUnit(Hex("#2b1212"), Hex("#ff6b6b"));

    /// <summary>Create pump sprite (gold energy well)</summary>
    public static Sprite Pump() => MakePump(Hex("#2f2414"), Hex("#ffd166"));

    /// <summary>Create projectile sprite (bright circle)</summary>
    public static Sprite Projectile()
    {
        int s = 8;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color clear = Color.clear;
        Color[] px = new Color[s * s];
        for (int i = 0; i < px.Length; i++) px[i] = clear;

        Color c = Hex("#ffd700");
        // 8x8 circle
        Block(px, s, 2, 0, c); Block(px, s, 3, 0, c); Block(px, s, 4, 0, c); Block(px, s, 5, 0, c);
        Block(px, s, 1, 1, c); Block(px, s, 2, 1, c); Block(px, s, 3, 1, c); Block(px, s, 4, 1, c); Block(px, s, 5, 1, c); Block(px, s, 6, 1, c);
        for (int y = 2; y <= 5; y++)
            for (int x = 0; x < 8; x++)
                Block(px, s, x, y, c);
        Block(px, s, 1, 6, c); Block(px, s, 2, 6, c); Block(px, s, 3, 6, c); Block(px, s, 4, 6, c); Block(px, s, 5, 6, c); Block(px, s, 6, 6, c);
        Block(px, s, 2, 7, c); Block(px, s, 3, 7, c); Block(px, s, 4, 7, c); Block(px, s, 5, 7, c);

        // Bright center
        Color bright = Hex("#ffed4e");
        Block(px, s, 3, 3, bright); Block(px, s, 4, 3, bright);
        Block(px, s, 3, 4, bright); Block(px, s, 4, 4, bright);

        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    // ---- Tower: castle tower shape ----
    static Sprite MakeTower(Color primary, Color accent)
    {
        var tex = CreateTex();
        Color[] px = ClearPixels();

        // Crenellations (top)
        Block(px, SIZE, 6, 21, primary); Block(px, SIZE, 8, 21, primary);
        Block(px, SIZE, 10, 21, primary); Block(px, SIZE, 12, 21, primary);
        Block(px, SIZE, 14, 21, primary); Block(px, SIZE, 16, 21, primary);

        Block(px, SIZE, 6, 22, primary); Block(px, SIZE, 8, 22, primary);
        Block(px, SIZE, 10, 22, primary); Block(px, SIZE, 12, 22, primary);
        Block(px, SIZE, 14, 22, primary); Block(px, SIZE, 16, 22, primary);

        // Top wall
        for (int x = 5; x <= 17; x++)
        {
            Block(px, SIZE, x, 20, primary);
            Block(px, SIZE, x, 19, accent);
        }

        // Body
        for (int y = 6; y <= 18; y++)
        {
            Block(px, SIZE, 5, y, primary);
            Block(px, SIZE, 17, y, primary);
            Block(px, SIZE, 6, y, primary);
            Block(px, SIZE, 16, y, primary);

            if (y % 3 == 0)
            {
                for (int x = 7; x <= 15; x++)
                    Block(px, SIZE, x, y, accent);
            }
            else
            {
                for (int x = 7; x <= 15; x++)
                    Block(px, SIZE, x, y, primary);
            }
        }

        // Door
        Block(px, SIZE, 10, 6, accent); Block(px, SIZE, 11, 6, accent); Block(px, SIZE, 12, 6, accent);
        Block(px, SIZE, 10, 7, accent); Block(px, SIZE, 12, 7, accent);
        Block(px, SIZE, 10, 8, accent); Block(px, SIZE, 12, 8, accent);

        // Base
        for (int x = 4; x <= 18; x++)
        {
            Block(px, SIZE, x, 5, primary);
            Block(px, SIZE, x, 4, primary);
        }

        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), SIZE / 1.5f);
    }

    // ---- King: crown shape ----
    static Sprite MakeKing(Color primary, Color accent)
    {
        var tex = CreateTex();
        Color[] px = ClearPixels();

        // Crown points
        Block(px, SIZE, 6, 22, accent); Block(px, SIZE, 7, 22, accent);
        Block(px, SIZE, 11, 22, accent); Block(px, SIZE, 12, 22, accent);
        Block(px, SIZE, 16, 22, accent); Block(px, SIZE, 17, 22, accent);

        Block(px, SIZE, 6, 21, accent); Block(px, SIZE, 7, 21, accent);
        Block(px, SIZE, 11, 21, accent); Block(px, SIZE, 12, 21, accent);
        Block(px, SIZE, 16, 21, accent); Block(px, SIZE, 17, 21, accent);

        // Crown body
        for (int y = 14; y <= 20; y++)
        {
            for (int x = 5; x <= 18; x++)
                Block(px, SIZE, x, y, primary);
        }

        // Jewels on crown
        Block(px, SIZE, 8, 18, accent); Block(px, SIZE, 9, 18, accent);
        Block(px, SIZE, 11, 17, accent); Block(px, SIZE, 12, 17, accent);
        Block(px, SIZE, 14, 18, accent); Block(px, SIZE, 15, 18, accent);

        // Crown band
        for (int x = 5; x <= 18; x++)
            Block(px, SIZE, x, 13, accent);

        // Body/robe
        for (int y = 4; y <= 12; y++)
        {
            for (int x = 6; x <= 17; x++)
                Block(px, SIZE, x, y, primary);
        }

        // Face area
        Block(px, SIZE, 9, 10, accent); Block(px, SIZE, 14, 10, accent); // eyes
        for (int x = 10; x <= 13; x++)
            Block(px, SIZE, x, 8, accent); // mouth

        // Base
        for (int x = 5; x <= 18; x++)
        {
            Block(px, SIZE, x, 3, primary);
        }

        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), SIZE / 2f);
    }

    // ---- Unit: soldier/character ----
    static Sprite MakeUnit(Color primary, Color accent)
    {
        var tex = CreateTex();
        Color[] px = ClearPixels();

        // Head
        for (int x = 9; x <= 14; x++)
        {
            Block(px, SIZE, x, 21, accent);
            Block(px, SIZE, x, 20, accent);
            Block(px, SIZE, x, 19, accent);
        }

        // Helmet top
        Block(px, SIZE, 10, 22, accent); Block(px, SIZE, 11, 22, accent);
        Block(px, SIZE, 12, 22, accent); Block(px, SIZE, 13, 22, accent);

        // Eyes
        Block(px, SIZE, 10, 20, primary); Block(px, SIZE, 13, 20, primary);

        // Body/armor
        for (int y = 12; y <= 18; y++)
        {
            for (int x = 8; x <= 15; x++)
                Block(px, SIZE, x, y, primary);
        }

        // Shield highlight
        Block(px, SIZE, 9, 16, accent); Block(px, SIZE, 10, 16, accent);
        Block(px, SIZE, 9, 15, accent); Block(px, SIZE, 10, 15, accent);

        // Arms
        Block(px, SIZE, 6, 16, primary); Block(px, SIZE, 7, 16, primary);
        Block(px, SIZE, 6, 15, primary); Block(px, SIZE, 7, 15, primary);
        Block(px, SIZE, 16, 16, primary); Block(px, SIZE, 17, 16, primary);
        Block(px, SIZE, 16, 15, primary); Block(px, SIZE, 17, 15, primary);

        // Weapon (right hand)
        Block(px, SIZE, 17, 17, accent); Block(px, SIZE, 17, 18, accent);
        Block(px, SIZE, 17, 19, accent);

        // Legs
        Block(px, SIZE, 9, 10, primary); Block(px, SIZE, 10, 10, primary);
        Block(px, SIZE, 9, 11, primary); Block(px, SIZE, 10, 11, primary);
        Block(px, SIZE, 13, 10, primary); Block(px, SIZE, 14, 10, primary);
        Block(px, SIZE, 13, 11, primary); Block(px, SIZE, 14, 11, primary);

        // Feet
        Block(px, SIZE, 8, 9, primary); Block(px, SIZE, 9, 9, primary); Block(px, SIZE, 10, 9, primary);
        Block(px, SIZE, 13, 9, primary); Block(px, SIZE, 14, 9, primary); Block(px, SIZE, 15, 9, primary);

        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), SIZE / 1.2f);
    }

    // ---- Pump: energy well ----
    static Sprite MakePump(Color primary, Color accent)
    {
        var tex = CreateTex();
        Color[] px = ClearPixels();

        // Top funnel
        Block(px, SIZE, 10, 22, accent); Block(px, SIZE, 11, 22, accent);
        Block(px, SIZE, 12, 22, accent); Block(px, SIZE, 13, 22, accent);

        Block(px, SIZE, 9, 21, accent); Block(px, SIZE, 10, 21, accent);
        Block(px, SIZE, 11, 21, accent); Block(px, SIZE, 12, 21, accent);
        Block(px, SIZE, 13, 21, accent); Block(px, SIZE, 14, 21, accent);

        // Beam
        Block(px, SIZE, 11, 20, primary); Block(px, SIZE, 12, 20, primary);
        Block(px, SIZE, 11, 19, primary); Block(px, SIZE, 12, 19, primary);

        // Body (well structure)
        for (int y = 8; y <= 18; y++)
        {
            Block(px, SIZE, 6, y, primary);
            Block(px, SIZE, 7, y, primary);
            Block(px, SIZE, 16, y, primary);
            Block(px, SIZE, 17, y, primary);
        }

        // Fill interior
        for (int y = 8; y <= 18; y++)
        {
            for (int x = 8; x <= 15; x++)
            {
                if (y <= 12)
                    Block(px, SIZE, x, y, accent);  // energy glow
                else
                    Block(px, SIZE, x, y, primary);
            }
        }

        // Energy sparkle
        Block(px, SIZE, 10, 11, Color.white); Block(px, SIZE, 13, 10, Color.white);

        // Base
        for (int x = 5; x <= 18; x++)
        {
            Block(px, SIZE, x, 7, primary);
            Block(px, SIZE, x, 6, primary);
        }

        // Gear/wheel
        Block(px, SIZE, 10, 5, accent); Block(px, SIZE, 11, 5, accent);
        Block(px, SIZE, 12, 5, accent); Block(px, SIZE, 13, 5, accent);

        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), SIZE / 1.5f);
    }

    // ---- Helpers ----

    static Texture2D CreateTex()
    {
        var tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    static Color[] ClearPixels()
    {
        var px = new Color[SIZE * SIZE];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        return px;
    }

    static void Block(Color[] px, int stride, int x, int y, Color c)
    {
        if (x >= 0 && x < stride && y >= 0 && y < stride)
            px[y * stride + x] = c;
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
