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
- **Old map import** — pick an `.sspm`/`.phxm`/`.txt` file straight from the select screen (same decode path Rhythia already has), and it plays back with its notes collapsed onto 3 lanes.
- **Audio-synced playback** — the map's song plays and drives the game clock, so gates stay in sync with the music instead of drifting.
- **Basic scoring** — a Score/Combo HUD increments on cleared gates and resets on a miss.
- **A motorcycle that looks like one** — a small multi-part bike model (body/tank/seat/wheels), gates rendered with Rhythia's own rounded-square "squircle" note mesh in per-lane colors, and a glow-enabled environment so it isn't just flat gray boxes.
- **A real map-select screen** — lists every map in your library, lets you import more, plus a Free Drive option; this is what actually launches when you start the game.

All of the above has been verified against a real map file end-to-end (decoded, cached, listed, played, gates rendered in the right lanes, Score/Combo updating), not just claimed — see [`docs/MOTORCYCLE_FORK.md`](docs/MOTORCYCLE_FORK.md) if you want the details.

What's not built yet: health/fail state, proper hit-accuracy judgments (a gate currently only checks lane + timing window, not a graded hit), a real track/road model (lanes are still flat colored strips), and a packaged release build.

---

## Playing

There's no packaged release yet — see [Building and Exporting](#building-and-exporting) to run it from source. Once running, you'll land on the map-select screen:

- Press **Import Map** and pick an old Rhythia/Sound Space map file (`.sspm`, `.phxm`, or `.txt`) — it decodes, caches, and shows up in the list as a button. You can also drop an already-converted `.phxm` file straight into your user folder's `maps/` directory and it'll be picked up on next launch.
- No maps imported? Press **Free Drive** — no gates, just the bike and the road, useful for checking movement/visuals work.
- In-game: **A** / **D** move the bike one lane left/right.

Motorhytha uses its own user data folder, separate from a real Rhythia install on the same machine:

| Platform | Path |
|---|---|
| **Windows** | `%appdata%\Motorhytha` |
| **Linux** | `~/.local/share/Motorhytha` |

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
├── scenes/          # Main game scenes: motorcycle_select.tscn (entry point)
│                    #  and motorcycle.tscn (gameplay)
├── scripts/         # C# source code
│   ├── database/    # Database / persistence layer
│   ├── game/        # Core gameplay (attempts, renderers, mods, judgments,
│   │                #  motorcycle-mode scripts live alongside the rest here)
│   ├── map/         # Map parsing and management (unchanged old-format support)
│   ├── multiplayer/ # Multiplayer lobby and player logic
│   ├── scenes/      # Scene-specific scripts, including MotorcycleSelect.cs
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

#### Run it in the editor (fastest way to try changes)

1. Clone the repo and fetch large files:
   ```bash
   git clone <this-repo-url>
   cd motorhytha
   git lfs fetch --all
   git lfs pull
   ```
2. Open **Godot 4.6.2 (.NET/Mono build)**, choose **Import**, and select this folder's `project.godot`.
3. Press **Play** (F5). The project's main scene is `scenes/motorcycle.tscn`, so it boots straight into the motorcycle prototype — no menu to click through.

#### Build/compile from the command line

You don't need the Godot editor open just to check the C# compiles:

```bash
dotnet restore Rhythia.csproj
dotnet build Rhythia.csproj -c Debug
```

This produces `Rhythia.dll` under `.godot/mono/temp/bin/`. It's enough to catch compile errors, but it does **not** produce a runnable game on its own — that still needs Godot.

#### Export a standalone build

This is what actually produces a `.exe`/`.x86_64` you can hand someone. It needs the Godot **editor** (not just the .NET SDK) plus that platform's **export templates**, matching your project's Godot version exactly (currently `4.6.2`, mono/.NET build):

1. Download the Godot 4.6.2 **mono** editor and its matching **export templates** for your OS from the [Godot releases page](https://github.com/godotengine/godot/releases/tag/4.6.2-stable) — you need the `_mono_` build, not the standard one, since this project uses C#.
2. Install the templates so Godot can find them (Godot does this for you via **Editor → Manage Export Templates** if you point it at the downloaded `.tpz` file; on Linux this lands in `~/.local/share/godot/export_templates/4.6.2.stable.mono/`).
3. In the editor: **Project → Export**, add a preset for your target platform (Windows/Linux/macOS), and click **Export Project**.
   - Headless/CI equivalent, once an `export_presets.cfg` exists in the project:
     ```bash
     godot --headless --import      # make sure resources are imported first
     godot --headless --export-release "Linux" builds/linux/Motorhytha.x86_64
     ```
   - `export_presets.cfg` is gitignored (matching upstream Rhythia's convention) since it's environment-specific — you'll need to create your own preset via the editor once, or write one by hand, before the headless export command above will work.
4. There is no CI-built release yet, so exporting locally is currently the only way to get a runnable binary.

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
