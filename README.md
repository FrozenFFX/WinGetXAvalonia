# WinGetXAvalonia

A modern, open-source package manager client for Windows, built on Avalonia UI and
the Windows Package Manager (winget).

## Owner

Copyright (c) 2026 FrozenFFX. All rights reserved.

- Owner: [FrozenFFX](https://github.com/FrozenFFX)
- Repository: https://github.com/FrozenFFX/WinGetXAvalonia

All source files carry an attribution header. Do not remove or modify it.

## Features

- Dashboard with package, health and system statistics
- Browse, install, update and remove packages via winget
- Installed packages and available updates
- Operation center with live progress and history
- Studio: tool catalog and workspace management
- Automation, blueprints and security views
- Search history, favorites and notifications
- Full dark / light theming from a single token system
- Localization-ready views (no hardcoded UI text)

## Tech Stack

- Avalonia UI 12.x
- .NET 10
- CommunityToolkit.Mvvm
- xUnit (tests)

## Getting Started

```
dotnet restore
dotnet build WinGetXAvalonia.csproj
dotnet run --project WinGetXAvalonia.csproj
dotnet test tests\WinGetXAvalonia.Tests\WinGetXAvalonia.Tests.csproj
```

## Architecture

Layered architecture with a one-way dependency rule:

```
Presentation
    ↓
Application
    ↓
Core
    ↓
Infrastructure
```

Domain code is UI-agnostic. ViewModels hold UI state and call services; services
perform work; business rules live in Core/Domain. See `AGENTS.md` (WinGetX Coding
Constitution) and `docs/architecture.md` for details.

## Branch Strategy

```
main
│
├── release/v1.0
│
├── develop
│
├── feature/dashboard
├── feature/installed
├── feature/browse
├── feature/updates
├── feature/history
├── feature/settings
│
└── founder
      ├── founder/theme-engine
      ├── founder/game-mode
      ├── founder/widgets
      └── founder/lab
```

Founder features are developed as a separate product and never mix with the
open-source edition. See `docs/founder-vision.md` and `docs/founder-roadmap.md`.

## License

This project is owned by FrozenFFX. See `AGENTS.md` for ownership rules.
