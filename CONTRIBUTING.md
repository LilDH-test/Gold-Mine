# Contributing to Gold Mine

Thank you for interest in contributing to Gold Mine! This document provides guidelines and instructions for contributing.

## Code of Conduct

- Be respectful and inclusive
- Welcome feedback and criticism
- Focus on what is best for the community
- Show empathy towards other community members

## How to Contribute

### Reporting Bugs

1. Check if bug already exists in Issues
2. Create new Issue with:
   - Clear title
   - Detailed description
   - Steps to reproduce
   - Expected vs actual behavior
   - Screenshots/logs if applicable
   - Your Unity version

### Suggesting Features

1. Check existing Feature Requests
2. Create new Issue with:
   - Clear title
   - Detailed description
   - Use cases and examples
   - Why this would benefit the game

### Code Contributions

1. **Fork the Repository**
   ```bash
   git clone https://github.com/yourusername/gold-mine.git
   cd gold-mine
   ```

2. **Create Feature Branch**
   ```bash
   git checkout -b feature/YourFeature
   ```

3. **Make Changes**
   - Follow code style guidelines (see below)
   - Write clean, documented code
   - Test your changes

4. **Commit Changes**
   ```bash
   git commit -m "Add: Brief description of changes"
   ```

5. **Push to Branch**
   ```bash
   git push origin feature/YourFeature
   ```

6. **Create Pull Request**
   - Reference related issues
   - Describe what changes do
   - Include any breaking changes

## Code Style Guidelines

### C# Naming Conventions

```csharp
// Classes and methods: PascalCase
public class GameManager { }
public void UpdateGameState() { }

// Private fields and local variables: camelCase
private float currentEnergy;
private void CalculateDamage() { }

// Constants: UPPER_CASE
private const float REGEN_RATE = 1.0f;

// Properties: PascalCase
public int Level { get; set; }
```

### Documentation

```csharp
/// <summary>
/// Brief one-line description.
/// </summary>
/// <param name="paramName">Description of parameter.</param>
/// <returns>Description of return value.</returns>
public int CalculateLevel(int xp)
{
    // Implementation
}
```

### File Organization

```csharp
using UnityEngine;
using System.Collections.Generic;

public class MyClass : MonoBehaviour
{
    // 1. Constants
    private const float SPEED = 5f;
    
    // 2. Serialized fields (for Inspector)
    [SerializeField] private float acceleration;
    
    // 3. Public properties
    public int Health { get; set; }
    
    // 4. Private fields
    private float currentVelocity;
    
    // 5. Unity lifecycle methods
    private void Start() { }
    private void Update() { }
    
    // 6. Public methods
    public void TakeDamage(float amount) { }
    
    // 7. Private methods
    private void ApplyVelocity() { }
}
```

## Git Commit Messages

Format: `Type: Brief description`

Types:
- **Add**: New feature
- **Fix**: Bug fix
- **Refactor**: Code restructuring
- **Docs**: Documentation updates
- **Style**: Code style/formatting
- **Test**: Testing updates
- **Perf**: Performance improvements

Examples:
```
Add: Energy pump building prefab
Fix: Unit pathfinding in tight corridors
Refactor: Consolidate tower targeting logic
Docs: Update card balance spreadsheet
```

## Pull Request Process

1. Update documentation as needed
2. Add tests if applicable
3. Ensure code follows style guide
4. Request review from maintainers
5. Address feedback and iterate
6. Maintainer will merge when approved

## Branching Strategy

- `main` - Stable, released version
- `develop` - Development branch
- `feature/*` - Feature branches
- `bugfix/*` - Bug fix branches
- `hotfix/*` - Critical fixes

## Testing

Before submitting PR:

1. Test in Unity Editor
2. Verify no console errors
3. Test both player and AI paths
4. Check balance (if game mechanics changed)
5. Test on target platforms (PC/Mobile)

## Asset Guidelines

### Sprites

- **Format**: PNG with transparency
- **Resolution**: 16x16, 32x32, or 64x64 (power of 2)
- **Style**: Consistent with 8-bit retro aesthetic
- **Filter Mode**: Point (no filter)

### Audio

- **Format**: WAV or OGG
- **Quality**: 44.1kHz or higher
- **License**: Royalty-free or CC licensed

## Documentation

- Update README if adding major features
- Add code comments for complex logic
- Document public APIs with XML docs
- Update CHANGELOG.md with changes

## Questions?

- Check existing Issues and Discussions
- Ask in Discord server
- Open a Discussion thread

Thank you for contributing! 🎮✨
