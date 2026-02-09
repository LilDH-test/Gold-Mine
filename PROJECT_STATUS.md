# Gold Mine — Project Status

## ✅ COMPLETE — Full Unity Game Scripts

All game logic from the HTML prototype has been ported to 19 Unity C# scripts.

### Architecture

```
Assets/Scripts/
├── Game/           ← Core data & persistence (3 files)
│   ├── GameManager.cs    — Singleton: save/load, XP/level/gold, card ownership
│   ├── GameState.cs      — Match state: energy, hand/deck, tower references
│   └── CardData.cs       — Static card database (19 cards, formulas match HTML)
│
├── Gameplay/       ← Runtime behaviours (11 files)
│   ├── MatchController.cs   — Main game loop: spawning, energy, win/loss
│   ├── TowerController.cs   — Base towers & king auto-attack
│   ├── UnitController.cs    — Unit movement, targeting, melee/ranged combat
│   ├── PumpController.cs    — Energy pump buildings
│   ├── ProjectileController.cs — Projectile movement & collision
│   ├── AIController.cs      — Enemy AI decision-making
│   ├── DeckHelper.cs        — Shuffle, deal hand, rotate after play
│   ├── TargetingHelper.cs   — Find nearest enemy in range
│   ├── Damageable.cs        — HP component for all entities
│   ├── HealthBar.cs         — World-space HP bar display
│   └── BattlefieldInput.cs  — Touch/click → lane → play card
│
├── UI/             ← User interface (4 files)
│   ├── MenuUI.cs         — Main menu, shop, settings, credits screens
│   ├── HUDManager.cs     — In-game HUD: energy bar, hand, game over overlay
│   ├── CardUI.cs         — Individual card in the hand
│   └── ShopItemUI.cs     — Individual shop item
│
└── Utils/          ← Constants (1 file)
    └── GameConstants.cs  — All balance values & arena layout
```

### What's Done ✅

- [x] GameManager singleton with proper save/load (no Dictionary serialization issues)
- [x] CardDatabase with all 19 cards matching HTML formulas exactly
- [x] GameState (pure C# class, not MonoBehaviour)
- [x] MatchController — full game loop (energy, spawning, win/loss, AI)
- [x] TowerController — auto-attack with projectiles
- [x] UnitController — movement, targeting, melee/ranged combat, king-drift
- [x] PumpController — energy generation buildings
- [x] ProjectileController — movement & collision
- [x] AIController — lane bias, pump priority, random timing
- [x] DeckHelper — shuffle, deal, rotate
- [x] TargetingHelper — nearest enemy in range, king lock check
- [x] Damageable — shared HP component with events
- [x] HealthBar — world-space HP display
- [x] BattlefieldInput — tap to play cards
- [x] MenuUI — all menu screens with shop purchasing
- [x] HUDManager — energy bar, hand display, game over overlay
- [x] CardUI — card display in hand
- [x] ShopItemUI — shop card display
- [x] GameConstants — all balance values

### To Open in Unity

1. Create a **Unity 2022.3+ LTS** project (2D template)
2. Copy `Assets/Scripts/` into the Unity project
3. Create prefabs (Tower, King, Unit, Pump, Projectile) with SpriteRenderer + components
4. Set up Canvas with MenuUI and HUDManager
5. Wire up Inspector references
6. Create pixel art sprites (16 PPU, Point filtering)
