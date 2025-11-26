# Contributing to The Heartof the Garden

Thank you for considering contributing! This document explains how to work with this repository.

---

## Getting Started
1. Clone the repository:
   ```bash
   git clone <https://github.com/Jobberson/The-Heart-of-the-Garden>
   ```
2. Install [Git LFS](https://git-lfs.github.com/) and run:
   ```bash
   git lfs install
   ```
3. Open the project in **Unity 6 (6000.0.62f1 LTS)**.

---

## Branching Strategy
- **main** → Stable production-ready builds.
- **develop** → Integration branch for new features.
- **feature/** → For new features (e.g., `feature/inventory-system`).
- **fix/** → For bug fixes (e.g., `fix/audio-crash`).
- **hotfix/** → Urgent fixes on main.

Create a branch:
```bash
git checkout -b feature/<name>
```

---

## Commit Message Convention
Follow [Conventional Commits](https://www.conventionalcommits.org/):
```
<type>(scope): short description
```
Examples:
- `feat(audio): add dynamic music system`
- `fix(ui): correct button alignment`

Types:
- `feat` | `fix` | `docs` | `style` | `refactor` | `perf` | `test` | `chore` | `asset` | `level`

---

## Pull Requests
- Ensure your branch is up to date with `develop`.
- Use clear titles and descriptions.
- Link related issues (e.g., `Closes #42`).

---

## Code Style
- Use **C# conventions**:
  - Braces on the next line after declaration.
  - PascalCase for classes and methods.
  - camelCase for variables.
- Keep scripts modular and documented.

---

## Assets
- Commit `.meta` files.
- Do **not** commit `Library/`, `Temp/`, or build folders.
- Large files (textures, audio, models) must be tracked with **Git LFS**.

---

## Testing
- Test your changes in Unity before submitting.
- If adding new systems, include basic unit tests if possible.

---

## Reporting Issues
Use the **Issue Templates** provided:
- Bug Report
- Feature Request
- Task

---

## License
This project is **proprietary**. All rights reserved.
