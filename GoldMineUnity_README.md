# Gold Mine - Unity Edition

Converting the JavaScript prototype to a full Unity game with 8-bit retro aesthetic.

## Project Structure

```
GoldMineUnity/
├── Assets/
│   ├── Scripts/
│   │   ├── Game/
│   │   │   ├── GameManager.cs
│   │   │   ├── GameState.cs
│   │   │   └── MatchController.cs
│   │   ├── UI/
│   │   │   ├── MenuUI.cs
│   │   │   ├── HUDManager.cs
│   │   │   └── ShopUI.cs
│   │   ├── Gameplay/
│   │   │   ├── Unit.cs
│   │   │   ├── Tower.cs
│   │   │   ├── Projectile.cs
│   │   │   └── AIController.cs
│   │   └── Utils/
│   │       ├── GameConstants.cs
│   │       └── SaveSystem.cs
│   ├── Prefabs/
│   │   ├── Units/
│   │   ├── Towers/
│   │   └── Effects/
│   ├── Sprites/
│   │   ├── Characters/
│   │   ├── UI/
│   │   └── Effects/
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Gameplay.unity
│   │   └── Shop.unity
│   └── Resources/
│       └── Data/
│           ├── CardData.json
│           └── PlayerSave.json
└── ProjectSettings/
```

## Setup

1. Create new Unity project (2022.3 LTS)
2. Set pixel-per-unit: 16
3. Import scripts from here
4. Build UI with retro 8-bit style
5. Create/import pixel art sprites

## Next: Start with GameManager.cs and GameState.cs
