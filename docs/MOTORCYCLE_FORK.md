# Motorhytha: Motorcycle Rhythm Fork

This repository is a fork of [Rhythia](https://github.com/Rhythia/Client) (the
mouse/3×3-grid rhythm game built on Godot 4.6 + C#). We are **not** changing
Rhythia's underlying framework — the `Attempt` state object, the
`GameComponent` game loop, the `UIComponent`/`Renderer` pipeline, and the
judgment/mod/scoring systems are all reused as-is. What we're building on top
is a new game mode: a **motorcycle rhythm racer**, where the player steers a
bike down a track in time with the music instead of moving a mouse cursor
around a grid.

## Why fork instead of build from scratch

Rhythia already solves the hard, boring parts of a rhythm game: audio timing,
map/replay parsing, judgments (hit/health/score), mods, camera handling,
settings, and the Godot project scaffolding. Reusing that means the
motorcycle mode can focus entirely on new gameplay: steering, lanes, and
obstacle/gate charts, instead of re-deriving timing and scoring.

## Mapping old mechanic -> new mechanic

| Rhythia concept | Motorcycle concept |
|---|---|
| Mouse cursor moving continuously across a 3×3 grid (`Attempt.CursorPosition`) | A/D keys moving the bike between 3 discrete lanes (`Attempt.BikeLane`) |
| 3×3 grid (3 columns × 3 rows) | Collapsed to 3×1: a chart's note **X** (column, -1/0/1) becomes the lane, **Y** (row) is ignored, via `MotorcycleLanes.LaneFromNoteX` |
| `Grid` (`scripts/game/ui/Grid.cs`) drawing the cursor | `MotorcycleController` (`scripts/game/ui/MotorcycleController.cs`) driving bike lane position, lean, and chase camera |
| `Note` objects + `NoteRenderer` | Same `Note` objects (unchanged map format), reused as track gates and drawn by `GateRenderer` |
| `HitJudgment` / `ScoreJudgment` / `HealthJudgment` (unimplemented stubs upstream) | `MotorcycleHitJudgment` — basic lane-vs-timing check that resolves each gate as the bike reaches it |
| `GameComponent.Play()` | Now also loads `Attempt.Map.Notes` (decoded by the existing `MapParser`) into `Attempt.Objects[typeof(Note)]`, so old map files work unchanged, and plays that map's audio through its own `AudioStreamPlayer` |
| Free-running/song-driven progress clock | `Attempt.Progress` is driven by the song's actual playback position (`AudioStreamPlayer.GetPlaybackPosition()`) whenever one is playing, falling back to a delta-time clock in freeplay with no map |
| `ScoreJudgment` (unimplemented stub upstream) | `MotorcycleHitJudgment` also tracks `Attempt.Score`/`Attempt.Combo`, shown by a new `MotorcycleHud` |
| Note mesh: `user/meshes/squircle.obj`, unshaded + vertex-colored | `GateRenderer` reuses the same squircle mesh for gates (instead of a plain `BoxMesh`), colored per lane, fading in and spinning as they approach |
| Cursor sprite / note glow (bloom from the menu's own `WorldEnvironment`) | `motorcycle.tscn` has its own `WorldEnvironment` (glow enabled, dark background) since it runs standalone and doesn't go through `main.tscn`'s background/space setup |
| `project.godot`'s `config/name`/`custom_user_dir_name` = `Rhythia` | Both renamed to `Motorhytha`, so this uses its own settings/maps folder instead of colliding with a real Rhythia install on the same machine |
| Rhythia's map-select UI (`scripts/ui/menu/play/*`) | `MotorcycleSelect` (`scripts/scenes/MotorcycleSelect.cs`, `scenes/motorcycle_select.tscn`) — a minimal list of cached maps + Free Drive/Freeroam, now the project's actual entry point (`project.godot`'s `run/main_scene`); it hands its choice to `GameComponent` via the static `MotorcycleSelection` class before switching scenes |
| N/A - no equivalent in grid mode | Freeroam: `Attempt.FreeRoam`, set from `MotorcycleSelection.FreeRoam`. `MotorcycleController` branches on it - normal/Free Drive play snaps between `MotorcycleLanes`' 3 fixed lanes, Freeroam moves continuously within `Constants.MOTORCYCLE_FREEROAM_BOUND` instead |
| Rhythia's `ImportButton`/`ImportDialog` (`scripts/ui/menu/ImportButton.cs`, `ImportDialog.cs`) | Mirrored directly in `MotorcycleSelect` - same `MapParser.BulkImport` call, same `.sspm`/`.phxm`/`.txt` filters, so old map files work through the actual UI, not just ones already sitting in the cache |

## Status

Playable loop end-to-end: `GameComponent` auto-selects the first map in the
player's library, loads its notes and audio, and plays it back in sync. The
bike moves between 3 lanes with A/D, gates resolve as hit/missed against
`HIT_WINDOW`, and Score/Combo update on a HUD. The bike is a small multi-part
model (body/tank/seat/wheels) rather than a single box, and gates use
Rhythia's own squircle note mesh with per-lane colors and a glow environment.
Verified in an actual exported build (screenshot-tested via a headless run),
not just the editor.

A real map-select screen (`MotorcycleSelect`) is now the project's entry
point: it lists cached maps, lets you import an old map file directly
(`.sspm`/`.phxm`/`.txt`, via the same `MapParser.BulkImport` Rhythia's own
import button uses), and has Free Drive and Freeroam options - hands the
choice to `GameComponent` before switching to `motorcycle.tscn`. Freeroam
drops the 3-lane snapping entirely for smooth continuous steering across the
whole road (verified in an exported build: held D, watched the bike drive
straight past the lane markers onto the open ground instead of snapping
between them).

This whole pipeline was verified against a real map file, not just assumed
to work: a generated `.phxm` (native format, embedded audio, notes on all 3
lanes) was decoded, appeared in the select screen, and playing it rendered
squircle gates in the correct lane colors with Score/Combo updating
correctly. That test also caught a real bug - `GateRenderer`'s `MultiMesh`
never set `TransformFormat` to `Transform3D` (it defaults to `Transform2D`),
so gates were silently failing to render entirely once a map actually had
notes. Fixed now.

Still missing: health/fail state, graded hit accuracy (currently pass/fail
only, not timing-graded), and a real track/road model — the lanes themselves
are still flat colored strips, not a modeled road.
