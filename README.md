<p align="center">
  <img src="https://github.com/Rhythia/Client/blob/master/textures/RhythiaSquircle.png?raw=true" alt="Motorhytha" width="400"/>
</p>

<h3 align="center">A motorcycle rhythm game: steer your bike across 3 lanes in time with the music.</h3>

<p align="center">
  <img src="https://img.shields.io/badge/Godot-4.6-%23478CBF?logo=godot-engine&logoColor=white" alt="Godot 4.6">
  <img src="https://img.shields.io/badge/status-early%20beta-orange" alt="Early beta">
</p>

---

> [!CAUTION]
> **Motorhytha is NOT affiliated with CAPO Games, Steam Rhythia, rhythia.com, or the official Rhythia project in any way.** We cannot provide support for issues related to those platforms.

---

## Table of Contents

- [About](#about)
- [Status](#status)
- [Playing](#playing)
- [Development](#development)
  - [Prerequisites](#prerequisites)
  - [Project Structure](#project-structure)
  - [Building and Exporting](#building-and-exporting)
- [Contributing](#contributing)
- [Credits](#credits)
- [License](#license)

---

## About

**Motorhytha** is a motorcycle rhythm game built on a fork of [Rhythia](https://github.com/Rhythia/Client)'s Godot 4.6 + C# game framework.

Rhythia is a rhythm game where you move a mouse cursor across a 3×3 grid to hit notes. Motorhytha keeps Rhythia's underlying engine (audio timing, map parsing, judgments/scoring, mods, camera handling) but replaces the core mechanic entirely: instead of a cursor on a grid, you steer a motorcycle across **3 lanes** with **A** and **D**, dodging and hitting gates that approach in time with the beat.

Old Rhythia/Sound Space map files still work — a map's 3×3 grid is collapsed down to 3 lanes (its column becomes the lane, its row is ignored), so any existing chart can be played as a motorcycle track with no conversion step.

See [`docs/MOTORCYCLE_FORK.md`](docs/MOTORCYCLE_FORK.md) for the full design rationale and a mapping between Rhythia's original concepts and Motorhytha's.

---

## Status

Motorhytha is an early, actively-developed beta. What works today:

- **A/D lane movement** — the bike moves between 3 discrete lanes with eased motion and a cosmetic lean.
- **Old map import** — any previously-imported map plays back, with its notes collapsed onto 3 lanes.
- **Audio-synced playback** — the map's song plays and drives the game clock, so gates stay in sync with the music instead of drifting.
- **Basic scoring** — a Score/Combo HUD increments on cleared gates and resets on a miss.

What's not built yet: a map-select screen (it currently auto-picks the first map in your library), health/fail state, proper hit-accuracy judgments (a gate currently only checks lane + timing window, not a graded hit), bike/track art (everything is placeholder boxes), and a packaged release build.

---

## Playing

There's no packaged release yet — see [Building and Exporting](#building-and-exporting) to run it from source. Once running:

- **A** / **D** — move the bike one lane left/right.
- Import a map into your Rhythia/Motorhytha user folder the same way you would for Rhythia, and it'll be picked up automatically.

---

## Development

### Prerequisites

| Tool | Version | Notes |
|---|---|---|
| [Godot Engine](https://godotengine.org/download) | **4.6** | .NET (C#) build required |
| [.NET SDK](https://dotnet.microsoft.com/download) | **10.0** | |
| [Git LFS](https://git-lfs.github.com/) | Latest | Required for large binary assets |

### Project Structure

```
├── addons/          # Third-party addons (ffmpeg, etc.)
├── docs/            # Design docs (see MOTORCYCLE_FORK.md)
├── fonts/           # Font assets
├── meshes/          # 3D mesh assets
├── prefabs/         # Reusable scene prefabs (UI elements, etc.)
├── scenes/          # Main game scenes, including motorcycle.tscn
├── scripts/         # C# source code
│   ├── database/    # Database / persistence layer
│   ├── game/        # Core gameplay (attempts, renderers, mods, judgments,
│   │                #  motorcycle-mode scripts live alongside the rest here)
│   ├── map/         # Map parsing and management (unchanged old-format support)
│   ├── multiplayer/ # Multiplayer lobby and player logic
│   ├── scenes/      # Scene-specific scripts
│   ├── shaders/     # Shader code
│   ├── skinning/    # Skin loading and management
│   ├── spaces/      # Space-related logic
│   ├── ui/          # UI components and notifications
│   └── util/        # Utility / helper classes
├── sounds/          # Audio assets
├── textures/        # Texture and image assets
├── themes/          # Godot UI themes
└── user/            # Default user data scaffold
```

### Building and Exporting

1. Clone the repo and fetch large files:
   ```bash
   git clone <this-repo-url>
   cd motorhytha
   git lfs fetch --all
   git lfs pull
   ```
2. Open **Godot 4.6 (.NET)**, choose **Import**, and select this folder's `project.godot`.
3. Press **Play** — the project's main scene is `scenes/motorcycle.tscn`, so it boots straight into the motorcycle prototype.
4. To produce a standalone build: **Project → Export**, add a preset for your target platform (you'll need that platform's export templates installed), and export. There is no CI-built release yet, so this is currently the only way to get a runnable binary.

---

## Contributing

Contributions are welcome — bug fixes, gameplay features, art, or documentation.

1. Fork the repository and clone your fork.
2. Make your changes on a branch.
3. Test locally in the Godot editor before submitting.
4. Open a pull request describing what changed and why.

Keep PRs focused — one feature or fix per PR — and follow the existing code style.

---

## Credits

Motorhytha is built on top of [Rhythia](https://github.com/Rhythia/Client) (formerly *Sound Space Plus*), an open-source rhythm game by the Rhythia team. Motorhytha is a separate, unaffiliated project.

---

## License

Motorhytha, like the Rhythia code it's built on, is licensed under the [GNU Affero General Public License v3.0](LICENSE).
