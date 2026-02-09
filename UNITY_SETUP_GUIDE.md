# Unity Conversion Guide - Gold Mine

## Step 1: Create New Unity Project

1. Open Unity Hub
2. Create new 2D project (2022.3 LTS or newer)
3. Set platform to "PC, Mac & Linux Standalone" (or Mobile)
4. Create project named "GoldMineUnity"

## Step 2: Project Setup

After project is created:

```
Assets/
├── Scripts/
│   ├── Game/
│   │   ├── GameManager.cs        (copy from GameManager.cs)
│   │   └── GameState.cs          (copy from GameState.cs)
│   ├── Cards/
│   │   └── CardData.cs           (copy from CardData.cs)
│   ├── Gameplay/
│   │   ├── Unit.cs
│   │   ├── Tower.cs
│   │   ├── Projectile.cs
│   │   └── AIController.cs
│   └── UI/
│       ├── MenuUI.cs
│       ├── HUDManager.cs
│       └── ShopUI.cs
├── Sprites/
│   ├── Units/
│   ├── Towers/
│   └── UI/
├── Prefabs/
└── Scenes/
    ├── MainMenu.unity
    ├── Gameplay.unity
    └── Shop.unity
```

## Step 3: Sprite Import Settings

For all pixel art sprites:
- **Texture Type**: Sprite (2D and UI)
- **Sprite Mode**: Single (or Multiple for atlases)
- **Filter Mode**: Point (no filter)
- **Compression**: None
- **Pixels Per Unit**: 16
- **Generate Mip Maps**: Off

## Step 4: Game Architecture

### Core Loop
```csharp
// GameManager.cs - Singleton managing game state
// GameState.cs - Current match state
// MatchController.cs - Manages turn/update loop
```

### Gameplay
```csharp
// Unit.cs - Soldier/unit behavior
// Tower.cs - Tower behavior and targeting
// Projectile.cs - Projectile movement
// AIController.cs - Enemy AI logic
```

### UI
```csharp
// MenuUI.cs - Main menu, shop, settings
// HUDManager.cs - In-game HUD (energy, cards, enemy info)
// CardDisplay.cs - Card UI prefab
```

## Step 5: Scene Setup

### MainMenu Scene
- Canvas with buttons: Play, Shop, Settings
- Player stats display (Level, Gold, XP)
- Retro pixel art background

### Gameplay Scene
- Canvas for HUD (top bar, bottom bar)
- Game board (canvas or world space for units)
- Camera setup

### Shop Scene
- Card grid display
- Purchase buttons
- Back button

## Step 6: Key Implementation Details

### Energy System
- Regen rate: 1.0 per second
- Max energy: 10
- Pump buildings add extra regen

### Card Costs (Energy)
- Melee units: 1-5 energy
- Ranged units: 2-6 energy
- Pumps: varies by tier

### Progression
- XP per win: 45 + level * 6
- XP per loss: 18 + level * 3
- Gold per win: 60 + level * 10
- Level formula: 120 * level^1.22

## Step 7: Pixel Art Assets

Create 16x16 or 32x32 pixel art for:
- [ ] Player units (melee, ranged)
- [ ] Enemy units
- [ ] Towers (player, enemy)
- [ ] King (player, enemy, locked)
- [ ] Pump/energy well
- [ ] Projectiles
- [ ] UI buttons and panels
- [ ] Background tiles

Alternatively, use existing generated assets or 8-bit tilesets.

## Step 8: Audio

Create audio system for:
- Background music (retro chipset style)
- SFX: attack, damage, spawn, levelup
- Settings to control volume

## Step 9: Testing & Balancing

1. Playtest core loop
2. Balance card costs and stats
3. Test AI difficulty scaling
4. Verify progression feels good

## Next Files to Create

1. **MatchController.cs** - Game loop, unit spawning, targeting
2. **AIController.cs** - Enemy decision making
3. **MenuUI.cs** - Main menu flow
4. **HUDManager.cs** - Update HUD each frame
5. **Unit.cs** - Unit movement, attack, health
6. **Tower.cs** - Tower targeting and attack

All files should follow the structure from the JS prototype but adapted for Unity's component system.
