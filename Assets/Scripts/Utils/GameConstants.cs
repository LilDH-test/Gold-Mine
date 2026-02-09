using UnityEngine;

/// <summary>
/// GameConstants — All balance values and world layout constants.
/// Matches the HTML prototype exactly.
/// </summary>
public static class GameConstants
{
    // ---- Arena layout (world units) ----
    public const float ARENA_WIDTH = 10f;
    public const float ARENA_HEIGHT = 20f;

    // Lane X positions (fraction of width)
    public static float LaneX(int lane) =>
        lane == 0 ? ARENA_WIDTH * 0.28f : ARENA_WIDTH * 0.72f;

    // Spawn Y
    public const float PLAYER_SPAWN_Y = -7.2f;   // bottom
    public const float ENEMY_SPAWN_Y = 7.2f;      // top

    // Tower Y
    public const float PLAYER_TOWER_Y = -5.6f;
    public const float ENEMY_TOWER_Y = 5.6f;

    // King Y
    public const float PLAYER_KING_Y = -8.0f;
    public const float ENEMY_KING_Y = 8.0f;

    // Pump fixed Y
    public const float PLAYER_PUMP_Y = -4.8f;
    public const float ENEMY_PUMP_Y = 4.8f;

    // River / midline
    public const float MID_Y = 0f;

    // ---- Base stats ----
    public const float BASE_TOWER_HP = 450f;
    public const float BASE_TOWER_DMG = 12f;
    public const float BASE_TOWER_APS = 0.9f;
    public const float BASE_TOWER_RANGE = 4.5f;  // scaled from 165px

    public const float KING_HP = 650f;
    public const float KING_DMG = 18f;
    public const float KING_APS = 0.85f;
    public const float KING_RANGE = 5.2f;  // scaled from 190px

    // ---- Energy ----
    public const float START_ENERGY = 5f;
    public const float MAX_ENERGY = 10f;
    public const float BASE_REGEN = 1.0f;  // per second

    // ---- Hand / Deck ----
    public const int HAND_SIZE = 4;
    public const int MIN_DECK_SIZE = 8;

    // ---- Projectile ----
    public const float PROJECTILE_SPEED = 10f;  // world units/sec
    public const float PROJECTILE_RADIUS = 0.08f;

    // ---- AI timing ----
    public const float AI_MIN_INTERVAL = 1.35f;
    public const float AI_MAX_INTERVAL = 2.10f;
    public const float AI_PUMP_BIAS = 0.75f;

    // ---- Pixel-to-world scale ----
    // The HTML uses ~370px arena width. We use 10 world units.
    // So 1px ≈ 0.027 world units. We round ranges for clarity.
    public static float PxToWorld(float px) => px * (ARENA_WIDTH / 370f);
}
