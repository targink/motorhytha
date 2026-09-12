using System.Collections;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using static LegacyRunner;

[GlobalClass]
public partial class GameComponent : Node3D
{
    [Export]
    public bool Standalone { get; set; } = true;

    [Export]
    public Camera3D Camera { get; set; }

    [Export]
    public Array<UIComponent> InterfaceComponents { get; set; }

    [Export]
    public Array<Renderer> Renderers { get; set; }

    [Signal]
    public delegate void AttemptProcessEventHandler(Attempt attempt);

    public HealthJudgment HealthProcessor { get; } = new HealthJudgment();

    public HitJudgment HitJudgement { get; } = new HitJudgment();

    public bool Playing { get; private set; } = true;

    public Attempt CurrentAttempt { get; private set; } = new();

    // Owns its own player instead of the shared SoundManager autoload, since
    // this component (and motorcycle mode) needs to run standalone.
    private AudioStreamPlayer song;

    private bool wasEscapePressed;
    private bool wasQuitPressed;

    public void Play(Attempt attempt)
    {
        Input.MouseMode = CurrentAttempt.Settings.AbsoluteInput.Value ? Input.MouseModeEnum.ConfinedHidden : Input.MouseModeEnum.Captured;
        Input.UseAccumulatedInput = false;

        HealthProcessor.ApplyAttempt(attempt);

        // Load the (old-format) map's notes into the attempt so renderers/
        // judgments (e.g. GateRenderer for motorcycle mode) can read them.
        attempt.Objects[typeof(Note)] = attempt.Map != null ? new List<object>(attempt.Map.Notes) : new List<object>();
        attempt.Progress = 0;
        attempt.BikeLane = 0;
        attempt.BikeLaneOffset = 0;
        attempt.FreeRoam = MotorcycleSelection.HasSelection && MotorcycleSelection.FreeRoam;
        attempt.Health = 100;
        attempt.IsFailed = false;
        attempt.LastHitGrade = "";
        attempt.Paused = false;

        song.Stop();

        if (attempt.Map != null)
        {
            string audioPath = $"{MapUtil.MapsCacheFolder}/{attempt.Map.Name}/audio.{attempt.Map.AudioExt}";
            song.Stream = Util.Audio.LoadFromFile(audioPath);
            song.VolumeDb = SoundManager.ComputeVolumeDb((float)attempt.Settings.VolumeMusic.Value, (float)attempt.Settings.VolumeMaster.Value, 70);
            song.Play();
        }

        ApplySettings(attempt.Settings);
    }

    public override void _Ready()
    {
        song = new AudioStreamPlayer { Name = "Song" };
        AddChild(song);

        ApplySettings(CurrentAttempt.Settings);

        // Automatically attempt to start the game if standalone
        if (Standalone)
        {
            if (MotorcycleSelection.HasSelection)
            {
                // Came from MotorcycleSelect with an explicit choice (a map,
                // or Free Drive/null) - honor it, don't second-guess it.
                CurrentAttempt.Map = MotorcycleSelection.SelectedMap;
                Play(CurrentAttempt);
            }
            else if (MapManager.Initialized)
            {
                // Running motorcycle.tscn directly (e.g. F6 in the editor)
                // without going through the select screen - fall back to
                // whatever's first in the library, or free-drive if empty.
                if (MapManager.Maps.Count > 0)
                {
                    CurrentAttempt.Map = MapManager.Maps[0];
                }

                Play(CurrentAttempt);
            }
            else
            {
                MapManager.MapsInitialized += maps =>
                {
                    if (maps.Count > 0)
                    {
                        CurrentAttempt.Map = maps[0];
                    }

                    Play(CurrentAttempt);
                };
            }
        }
    }

    public override void _Process(double delta)
    {
        if (Standalone)
        {
            handlePauseAndQuitInput();
        }

        if (Playing && !CurrentAttempt.Paused && !CurrentAttempt.IsFailed)
        {
            // Drive the clock from the song's own playback position when one
            // is playing, so notes/gates stay in sync with the audio instead
            // of drifting against a free-running delta-time clock.
            CurrentAttempt.Progress = song.Playing ? song.GetPlaybackPosition() * 1000.0 : CurrentAttempt.Progress + delta * 1000;
        }

        if (CurrentAttempt.IsFailed && song.Playing)
        {
            song.StreamPaused = true;
        }

        // Update rendering (notes/objects) on attempt state
        foreach (var renderer in Renderers)
        {
            renderer.Process(delta, CurrentAttempt);
        }

        // Update interface components based on attempt state
        foreach (var component in InterfaceComponents)
        {
            component.Process(delta, CurrentAttempt);
        }

        EmitSignalAttemptProcess(CurrentAttempt);

    }

    // Escape toggles pause; Q (only while paused or failed) returns to the
    // select screen. Previously there was no way to leave a running session
    // short of force-quitting the app.
    private void handlePauseAndQuitInput()
    {
        bool escapePressed = Input.IsPhysicalKeyPressed(Key.Escape);
        bool quitPressed = Input.IsPhysicalKeyPressed(Key.Q);

        if (escapePressed && !wasEscapePressed && !CurrentAttempt.IsFailed)
        {
            CurrentAttempt.Paused = !CurrentAttempt.Paused;
            song.StreamPaused = CurrentAttempt.Paused;
        }

        if (quitPressed && !wasQuitPressed && (CurrentAttempt.Paused || CurrentAttempt.IsFailed))
        {
            GetTree().ChangeSceneToFile("res://scenes/motorcycle_select.tscn");
        }

        wasEscapePressed = escapePressed;
        wasQuitPressed = quitPressed;
    }

    public void ApplySettings(SettingsProfile settings)
    {
        CurrentAttempt.Settings = settings;

        foreach (var component in InterfaceComponents)
        {
            component.ApplySettings(settings);
        }

        foreach (var renderer in Renderers)
        {
            renderer.ApplySettings(settings);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!Playing || !Standalone) { return; }

        if (@event is InputEventMouseMotion eventMouseMotion && Playing)
        {
            CurrentAttempt.CameraMode.Process(CurrentAttempt, Camera, eventMouseMotion.Relative);

            CurrentAttempt.DistanceMM += eventMouseMotion.Relative.Length() / CurrentAttempt.Settings.Sensitivity.Value / 57.5;
        }
    }
}
