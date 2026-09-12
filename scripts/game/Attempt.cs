using System;
using System.Collections.Generic;
using Godot;

public partial class Attempt : GodotObject
{
    public bool IsReplay { get; set; }

    public bool Paused { get; set; }

    public CameraMode CameraMode { get; set; } = new CameraLock();

    public Map Map { get; set; }

    public double Progress { get; set; }

    public Vector3 CameraPosition { get; set; } = new Vector3(0, 0, 3.75f);

    public Vector3 CameraRotation { get; set; } = Vector3.Zero;

    public Vector2 CursorPosition { get; set; } = new();

    public Vector2 RawCursorPosition { get; set; } = new();

    public Vector3 CameraBasisZ { get; set; } = new();

    public int Speed { get; set; }

    public List<Mod> Mods { get; set; } = new();

    public Dictionary<Type, IList<object>> Objects { get; set; } = new();

    public SettingsProfile Settings { get; set; } = new();

    public double DistanceMM { get; set; }

    public Replay? Replay { get; set; }

    // Motorcycle mode: which of the 3 lanes (-1, 0, 1) the bike currently
    // occupies. Driven by A/D input in MotorcycleController.
    public int BikeLane { get; set; }

    // Motorcycle mode: smoothed world-space X position of the bike as it
    // moves toward BikeLane's target position, used for rendering/camera.
    public float BikeLaneOffset { get; set; }

    // Motorcycle mode: bike lean angle in radians, purely cosmetic/feedback.
    public float BikeLean { get; set; }

    // Motorcycle mode: gates cleared in a row / total cleared, tracked by
    // MotorcycleHitJudgment and shown by MotorcycleHud.
    public int Combo { get; set; }

    public int Score { get; set; }

    // Motorcycle mode: when true, MotorcycleController steers with smooth
    // continuous movement across the whole road instead of snapping between
    // the 3 fixed lanes. Set from MotorcycleSelection's Freeroam choice.
    public bool FreeRoam { get; set; }

    // Motorcycle mode: 0-100, ticks down on a miss and up on a hit
    // (MotorcycleHitJudgment); hitting 0 sets IsFailed.
    public double Health { get; set; } = 100;

    public bool IsFailed { get; set; }

    // Motorcycle mode: grade of the most recently resolved gate ("Perfect",
    // "Good", or "Miss"), shown briefly by MotorcycleHud.
    public string LastHitGrade { get; set; } = "";
}
