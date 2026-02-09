# Gold Mine - 8-Bit Tower Defense Game

A retro 8-bit styled tower defense card game built with Unity. Defend your towers and defeat your opponent's king!

![Gold Mine](https://img.shields.io/badge/Genre-Tower%20Defense-brightgreen)
![Unity](https://img.shields.io/badge/Engine-Unity%202022.3%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)

## 🎮 Game Overview

Gold Mine is a strategic tower defense game where you:
- Build units and defensive structures
- Manage limited energy resources
- Progress through levels to unlock new cards
- Defeat enemy AI opponents
- Collect gold and experience to level up

### Key Features

- **8-Bit Retro Aesthetic** - Pixel-perfect graphics and authentic retro styling
- **Card-Based System** - 19 unique cards to collect and upgrade
- **Energy Economy** - Real-time energy regeneration with pump buildings
- **AI Opponent** - Intelligent enemy that adapts to your strategy
- **Progression System** - Level up, unlock cards, earn gold
- **Cross-Platform** - Play on PC, Mac, Linux, and Mobile (iOS/Android)

## 🚀 Quick Start

### Prerequisites
- Unity 2022.3 LTS or newer
- Git

### Installation

1. Clone the repository
```bash
git clone https://github.com/yourusername/gold-mine.git
cd gold-mine
```

2. Open in Unity
   - Launch Unity Hub
   - Add project from the cloned folder
   - Open the project

3. Open Main Scene
   - Go to `Assets/Scenes/MainMenu.unity`
   - Press Play

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Game/
│   │   ├── GameManager.cs          # Core game singleton
│   │   ├── GameState.cs            # Current match state
│   │   └── MatchController.cs      # Game loop
│   ├── Gameplay/
│   │   ├── Unit.cs                 # Unit behavior
│   │   ├── Tower.cs                # Tower behavior
│   │   ├── Projectile.cs           # Projectile system
│   │   └── AIController.cs         # Enemy AI
│   ├── UI/
│   │   ├── MenuUI.cs               # Main menu
│   │   ├── HUDManager.cs           # In-game HUD
│   │   ├── ShopUI.cs               # Shop screen
│   │   └── CardDisplay.cs          # Card UI
│   └── Utils/
│       ├── GameConstants.cs        # Game constants
│       └── SaveSystem.cs           # Save/load system
├── Sprites/
│   ├── Characters/                 # Unit sprites
│   ├── Towers/                     # Tower sprites
│   ├── UI/                         # UI elements
│   └── Effects/                    # Particle effects
├── Prefabs/
│   ├── Units/                      # Unit prefabs
│   ├── Towers/                     # Tower prefabs
│   └── Effects/                    # Effect prefabs
├── Scenes/
│   ├── MainMenu.unity              # Main menu scene
│   ├── Gameplay.unity              # Game scene
│   └── Shop.unity                  # Shop scene
└── Resources/
    └── Data/                       # Game data (JSON)
```

## 🎯 Game Mechanics

### Cards (Energy Cost)

**Basic Cards (Always Owned):**
- Miner M1 (1 energy)
- Slinger R2 (2 energy)
- Guard M3 (3 energy)
- Archer R4 (4 energy)
- Bruiser M5 (5 energy)
- Rifle R6 (6 energy)

**Unlockable Cards:**
- Tier 7-10 Melee/Ranged units
- Pump I-V (Energy generators)

### Energy System

- Base Regen: 1 energy/second
- Max Energy: 10
- Pump buildings add 0.45-0.85 energy/second

### Progression

| Metric | Formula |
|--------|---------|
| XP per Win | 45 + level × 6 |
| XP per Loss | 18 + level × 3 |
| Gold per Win | 60 + level × 10 |
| Level XP | 120 × level^1.22 |

## 🛠️ Development

### Setup Development Environment

```bash
# Clone repository
git clone https://github.com/yourusername/gold-mine.git

# Open in Unity
# File → Open Project → Select folder
```

### Code Style

- Use PascalCase for public members
- Use camelCase for private members
- Follow Unity naming conventions
- Add XML documentation to public methods

### Creating New Cards

```csharp
// In CardData.cs
public static Card CreateUnitCard(string id, string name, int cost, string kind)
{
    return new Card 
    {
        id = id,
        name = name,
        type = CardType.Unit,
        cost = cost,
        kind = kind,
        // ... other stats
    };
}
```

### Testing

- Play test in Editor
- Test both player and AI actions
- Verify progression system
- Balance card costs

## 📊 Game Balance

### Unit Stats

**Melee Units:**
- Higher HP (85 + cost × 22)
- Slower attack (1.0 APS)
- Close range (18)

**Ranged Units:**
- Lower HP (70 + cost × 16)
- Faster attack (1.1 APS)
- Long range (120)

## 🐛 Known Issues

- [ ] Mobile touch input needs optimization
- [ ] Some edge cases in pathfinding
- [ ] Audio system not yet implemented

## 🗺️ Roadmap

- [x] Core gameplay loop
- [x] Card system
- [x] AI opponent
- [ ] Audio system
- [ ] Mobile optimization
- [ ] Multiplayer (online)
- [ ] Leaderboards
- [ ] Cosmetics/skins

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Credits

**Development Team:**
- Logan Bishop - Lead Developer
- Ben Garcia - Developer
- Mikey Bryant - Developer
- Gavin Giersdorf - Artist, Play-Tester
- Sebastian Melendez - Artist, Play-Tester
- Alan Estrada - Play-Tester, Lead QA

## 📞 Contact

- Discord: [Join Server]
- Twitter: [@GoldMineGame]
- Email: contact@goldmine.dev

## 🎨 Design Philosophy

Gold Mine embraces retro 8-bit aesthetic while delivering modern gameplay. Every design decision prioritizes:
- **Clarity** - Clear visual feedback for all actions
- **Accessibility** - Easy to learn, hard to master
- **Balance** - All cards viable in different strategies
- **Polish** - Smooth animations and responsive controls

---

**Made with ❤️ and lots of pixels** ✨
