# GitHub Repository Setup Guide

Follow these steps to create and manage the Gold Mine GitHub repository.

## Step 1: Create GitHub Repository

1. Go to [github.com](https://github.com)
2. Sign in to your account (or create one)
3. Click **"+"** in top-right → **"New repository"**
4. Fill in details:
   - **Repository name**: `gold-mine`
   - **Description**: `8-bit Tower Defense Card Game - Built with Unity`
   - **Visibility**: Public (or Private if preferred)
   - **Initialize**: Uncheck "Add .gitignore" and "Add license" (we have them)
5. Click **"Create repository"**

## Step 2: Install Git

### Windows
1. Download from [git-scm.com](https://git-scm.com)
2. Run installer with defaults
3. Restart terminal/PowerShell

### macOS
```bash
brew install git
```

### Linux
```bash
sudo apt-get install git
```

Verify installation:
```bash
git --version
```

## Step 3: Configure Git

```bash
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

## Step 4: Initialize Local Repository

```bash
cd c:\Users\lbisho\Documents\vex-vscode-projects\82793A
git init
git add .
git commit -m "Initial commit: Gold Mine 8-bit tower defense game"
```

## Step 5: Connect to GitHub

Replace `yourusername` with your GitHub username:

```bash
git branch -M main
git remote add origin https://github.com/yourusername/gold-mine.git
git push -u origin main
```

Or using SSH (if configured):
```bash
git remote add origin git@github.com:yourusername/gold-mine.git
git push -u origin main
```

## Step 6: Verify on GitHub

1. Visit `https://github.com/yourusername/gold-mine`
2. You should see all files uploaded
3. Click on files to preview them

## Step 7: GitHub Settings

### Branch Protection

1. Go to **Settings** → **Branches**
2. Add rule for `main` branch:
   - Require pull request reviews
   - Require 1 approval
   - Dismiss stale reviews
   - Require branches to be up to date

### Access Control

1. **Settings** → **Collaborators**
2. Add team members (if needed)
3. Assign appropriate permissions

### Labels

Create labels for organizing issues:
- `bug` - Red
- `feature` - Green
- `documentation` - Blue
- `help wanted` - Pink
- `good first issue` - Orange

## Step 8: Set Up GitHub Projects

1. Go to **Projects** tab
2. Create new project: "Gold Mine Development"
3. Create columns:
   - To Do
   - In Progress
   - In Review
   - Done

## Step 9: Regular Workflow

### Making Changes

```bash
# Create feature branch
git checkout -b feature/CardSystem

# Make changes...

# Stage changes
git add .

# Commit
git commit -m "Add: Card system implementation"

# Push to GitHub
git push origin feature/CardSystem
```

### Create Pull Request

1. Push branch to GitHub
2. Go to repository
3. Click **"Compare & pull request"**
4. Fill in PR details
5. Request review
6. Merge after approval

### Keep Local Synced

```bash
# Fetch latest changes
git fetch origin

# Merge main branch
git merge origin/main

# Or pull directly
git pull origin main
```

## Step 10: Documentation Updates

Update these files as project evolves:

| File | When to Update |
|------|---|
| README.md | New features, setup changes |
| CHANGELOG.md | Every release |
| CONTRIBUTING.md | Process changes |
| UNITY_SETUP_GUIDE.md | Setup procedure changes |

## Step 11: Release Process

When ready to release (e.g., v0.1.0):

```bash
# Make sure everything is committed
git status

# Create version tag
git tag -a v0.1.0 -m "Release version 0.1.0"

# Push tag to GitHub
git push origin v0.1.0
```

Then on GitHub:
1. Go to **Releases** tab
2. Click **"Draft a new release"**
3. Select tag v0.1.0
4. Add release notes
5. Publish

## Useful Git Commands

```bash
# Check status
git status

# View commit history
git log --oneline

# View changes
git diff

# Undo last commit (keep changes)
git reset --soft HEAD~1

# Undo last commit (discard changes)
git reset --hard HEAD~1

# Switch branches
git checkout branch-name

# Create and switch to new branch
git checkout -b feature/new-feature

# Delete branch
git branch -d branch-name
```

## Troubleshooting

### Push rejected
```bash
git pull origin main
git push origin feature-branch
```

### Wrong branch
```bash
git checkout -b feature/correct-name
```

### Accidentally committed to main
```bash
git reset --soft HEAD~1  # Undo commit
git checkout -b feature/my-feature  # Create branch
git commit -m "message"  # Re-commit on branch
```

## GitHub Pages (Optional)

Set up documentation site:

1. **Settings** → **Pages**
2. Select `main` branch
3. Select `/docs` folder
4. Site builds automatically

Add `/docs/index.md`:
```markdown
# Gold Mine Documentation

[Game Overview](#)
[Installation](#)
[Contributing](#)
```

---

**Ready to share your project with the world!** 🚀

Need help? See [GitHub Docs](https://docs.github.com)
