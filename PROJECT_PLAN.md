# Gold Mine - Complete Project Overview

## 🎮 What is Gold Mine?

Gold Mine is an 8-bit retro tower defense card game where players:
- Collect and play cards to spawn units and buildings
- Manage limited energy resources
- Defeat opponents by destroying their king
- Progress through levels to unlock new cards
- Compete against intelligent AI

**Target Audience**: Casual gamers, tower defense fans, retro game enthusiasts

## 📊 Project Status

**Phase**: Early Development (Alpha)
**Version**: 0.1.0-alpha
**Last Updated**: February 9, 2026

### Completed
- ✅ HTML web prototype with full gameplay
- ✅ 8-bit retro aesthetic (fonts, colors, UI style)
- ✅ Game mechanics (energy, cards, units, towers)
- ✅ AI system
- ✅ Progression system
- ✅ Unity project structure created
- ✅ GitHub repository setup documentation

### In Progress
- 🔄 Unity core scripts (GameManager, GameState, CardData)
- 🔄 Project documentation

### To Do
- ⬜ MatchController and game loop
- ⬜ Unit and Tower systems
- ⬜ UI implementation
- ⬜ Sprite assets
- ⬜ Audio system
- ⬜ Mobile optimization

## 🏗️ Architecture Overview

### Game Loop
```
Update Energy → Check AI Decision → Spawn Units → Update Units → 
Check Projectiles → Check Damage → Update UI → Check Win/Loss
```

### Core Systems

**1. Energy System**
- Players and enemies have 10 max energy
- Base regen: 1 energy/sec
- Pumps add 0.45-0.85 energy/sec
- Cards cost 1-10 energy to play

**2. Card System**
- 19 total cards (6 basic + 13 unlockable)
- Types: Unit (Melee/Ranged), Tower, Pump
- Each card has stats: HP, damage, speed, range
- Cards unlocked via leveling up

**3. Combat System**
- Units target enemies within range
- Ranged units fire projectiles
- Melee units deal instant damage
- Towers auto-attack
- King is final objective

**4. Progression System**
- XP earned from wins/losses
- Level increases unlock new cards
- Gold earned only from wins
- Cards purchased with gold from shop

**5. AI System**
- Makes random affordable card choices
- Prefers pumps early game
- Uses lane awareness for placement
- Difficulty scales with player level

## 📁 Project Structure

```
gold-mine/
├── Assets/
│   ├── Scripts/
│   │   ├── Game/              # Core game systems
│   │   ├── Gameplay/          # Unit, Tower, Projectile, AI
│   │   ├── UI/                # Menu, HUD, Shop
│   │   └── Utils/             # Constants, Save system
│   ├── Sprites/               # All pixel art
│   ├── Prefabs/               # Reusable objects
│   ├── Scenes/                # Game scenes
│   └── Resources/             # Data files
├── Docs/                      # Documentation
├── .gitignore                 # Git ignore file
├── README.md                  # Main documentation
├── LICENSE                    # MIT License
├── CONTRIBUTING.md            # Contribution guidelines
├── CHANGELOG.md               # Version history
├── GITHUB_SETUP.md            # GitHub setup guide
└── PROJECT_PLAN.md            # This file
```

## 🎯 Feature Roadmap

### Phase 1: Core Gameplay (Weeks 1-2)
- [ ] MatchController and game loop
- [ ] Unit behavior and pathfinding
- [ ] Tower targeting system
- [ ] Projectile system
- [ ] Damage and collision
- [ ] Win/loss conditions

### Phase 2: UI & Polish (Weeks 3-4)
- [ ] Main menu UI
- [ ] In-game HUD
- [ ] Shop interface
- [ ] Settings screen
- [ ] Card display and selection
- [ ] Game over screen

### Phase 3: Art & Audio (Weeks 5-6)
- [ ] Unit sprite art
- [ ] Tower sprites
- [ ] UI graphics
- [ ] Particle effects
- [ ] Background tiles
- [ ] Background music (chiptune)
- [ ] Sound effects

### Phase 4: Polish & Testing (Week 7)
- [ ] Game balance
- [ ] Bug fixes
- [ ] Performance optimization
- [ ] User testing
- [ ] Balance adjustments

### Phase 5: Release & Deployment (Week 8+)
- [ ] Beta release
- [ ] Community feedback
- [ ] Version 1.0 release
- [ ] Store submissions (Steam, etc.)

## 👥 Team & Roles

| Role | Responsibility |
|------|---|
| **Lead Developer** | Project management, core systems |
| **Programmer** | Gameplay systems, AI, UI |
| **Pixel Artist** | Sprites, animations, effects |
| **Playtester** | Testing, balance feedback, QA |
| **Sound Designer** | Music, SFX (future) |

## 💾 Data Storage

### Player Data (Local)
```json
{
  "level": 5,
  "xp": 1500,
  "gold": 2300,
  "ownedCards": ["b_m1", "b_r2", "u_m7"],
  "settings": {
    "musicVolume": 0.6,
    "sfxVolume": 0.8
  }
}
```

### Card Data
```json
{
  "id": "b_m1",
  "name": "Miner (M1)",
  "type": "unit",
  "cost": 1,
  "unlockLevel": 1,
  "kind": "melee",
  "hp": 85,
  "damage": 11,
  "aps": 1.0,
  "speed": 88,
  "range": 18
}
```

## 🎨 Art Direction

### Visual Style
- **Resolution**: 16x16 or 32x32 base tiles
- **Palette**: Limited color palette (8-16 colors)
- **Animation**: 2-4 frame simple animations
- **UI**: Retro computer aesthetic
- **Colors**: Gold, black, dark blue, lime green, red

### Typography
- **Font**: Press Start 2P (8-bit style)
- **Sizes**: 11px, 13px, 14px (scaled)
- **Colors**: Gold on black backgrounds

## 🎮 Gameplay Balance

### Card Cost Progression
- Basic: 1-6 energy
- Unlockable: 7-10 energy
- Pumps: 2-8 energy (scaled by tier)

### Unit Stats
- **Melee**: Tankier, slower attack, close range
- **Ranged**: Fragile, faster attack, long range

### Energy Regen
- Player pump: +0.45-0.85 eps
- Enemy pump: +0.45-0.85 eps
- Base regen: 1.0 eps (both)

### Tower Stats
- HP: 450
- Damage: 12
- APS: 0.9
- Range: 165

### King Stats
- HP: 650
- Damage: 18
- APS: 0.85
- Range: 190

## 🌐 Platform Targets

### Primary (MVP)
- Windows PC (x86_64)
- macOS (x86_64 + Apple Silicon)

### Secondary
- Linux (x86_64)
- iOS (future)
- Android (future)
- Web (via WebGL)

## 📈 Success Metrics

- [ ] Core gameplay loop stable
- [ ] All 19 cards balanced and viable
- [ ] AI provides challenge
- [ ] Progression feels rewarding
- [ ] UI responsive and intuitive
- [ ] No major bugs post-release
- [ ] Community feedback positive

## 🤝 Community & Support

### Channels
- **GitHub Issues**: Bug reports, feature requests
- **GitHub Discussions**: Questions, feedback
- **Discord**: Community chat (future)
- **Twitter**: Updates and announcements

### Contributing
- Fork repository
- Create feature branch
- Submit pull request
- Follow code guidelines (see CONTRIBUTING.md)

## 📚 Resources

- [Unity Documentation](https://docs.unity3d.com)
- [Game Design Patterns](https://gameprogrammingpatterns.com)
- [8-bit Graphics Inspiration](https://itch.io)
- [Tower Defense Game Design](https://gamedeveloper.com)

## 📝 Notes for Development

### Important Considerations
1. Keep retro aesthetic consistent throughout
2. Balance must favor skill over RNG
3. UI must be mobile-friendly
4. Performance: target 60 FPS on all platforms
5. Save system must be reliable
6. AI should not feel unfair

### Known Challenges
- Implementing smooth pathfinding in limited space
- Balancing AI difficulty with fairness
- Creating satisfying pixel art animations
- Mobile touch input responsiveness

## 🚀 How to Get Started

1. **Clone Repository**
   ```bash
   git clone https://github.com/yourusername/gold-mine.git
   ```

2. **Open in Unity** (2022.3 LTS)
   ```
   File → Open Project → Select folder
   ```

3. **Review Documentation**
   - Read README.md
   - Check UNITY_SETUP_GUIDE.md
   - Review existing code

4. **Start Development**
   - Choose task from Phase 1
   - Create feature branch
   - Follow code guidelines
   - Submit pull request

## 📞 Questions?

- Check GitHub Issues for existing answers
- Open new Discussion for questions
- Email: contact@goldmine.dev
- Discord: [Join Server] (future)

---

**Last Updated**: February 9, 2026
**Version**: 0.1.0-alpha
**Status**: Early Development

Good luck developing Gold Mine! 🎮✨
