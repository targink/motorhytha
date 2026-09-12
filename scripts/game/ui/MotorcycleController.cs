using System;
using Godot;

// Motorcycle mode's replacement for Grid: instead of positioning a cursor on
// a 3x3 grid, this moves the bike between 3 discrete lanes using A/D input
// and (optionally) follows it with a chase camera. Runs every frame via
// Process(), unlike the old grid's mouse-driven CameraMode pipeline, since
// keyboard input isn't tied to mouse-motion events.
public partial class MotorcycleController : UIComponent
{
    // Exponential smoothing "sharpness" - higher eases out faster/snappier,
    // lower feels heavier/more drifty. Frame-rate independent, unlike a
    // flat units/second MoveToward (which moved at constant speed then
    // stopped dead the instant it hit the target).
    private const float LaneSmoothing = 14f;

    private const float FreeRoamAccelSmoothing = 8f;

    private const float LeanSmoothing = 10f;

    private const float CameraSmoothing = 18f;

    [Export]
    public Node3D Bike { get; set; }

    [Export]
    public Camera3D Camera { get; set; }

    private bool wasLeftPressed;
    private bool wasRightPressed;
    private float previousPositionX;
    private float freeRoamVelocity;
    private float cameraX;

    public override void ApplySettings(SettingsProfile settings)
    {
    }

    public override void Process(double delta, Attempt state)
    {
        if (!state.Paused && !state.IsFailed)
        {
            if (state.FreeRoam)
            {
                handleFreeRoamInput(state, delta);
            }
            else
            {
                handleLaneInput(state);
                updateBikePosition(state, delta);
            }
        }

        updateBikeLean(state, delta);
        updateCamera(state, delta);
    }

    private void handleLaneInput(Attempt state)
    {
        bool leftPressed = Input.IsPhysicalKeyPressed(Key.A);
        bool rightPressed = Input.IsPhysicalKeyPressed(Key.D);

        if (leftPressed && !wasLeftPressed)
        {
            state.BikeLane = Math.Max(state.BikeLane - 1, -1);
        }

        if (rightPressed && !wasRightPressed)
        {
            state.BikeLane = Math.Min(state.BikeLane + 1, 1);
        }

        wasLeftPressed = leftPressed;
        wasRightPressed = rightPressed;
    }

    private void updateBikePosition(Attempt state, double delta)
    {
        float targetX = MotorcycleLanes.LaneWorldX(state.BikeLane);
        state.BikeLaneOffset = smoothTo(state.BikeLaneOffset, targetX, LaneSmoothing, delta);

        Bike.Position = new Vector3(state.BikeLaneOffset, Bike.Position.Y, Bike.Position.Z);
    }

    // Freeroam: smooth continuous steering across the whole road instead of
    // snapping between the 3 fixed lanes - no gates/scoring to line up with.
    // Accelerates/decelerates toward a target speed rather than jumping
    // straight to it, so starting, stopping, and reversing direction all
    // ease instead of snapping.
    private void handleFreeRoamInput(Attempt state, double delta)
    {
        bool leftPressed = Input.IsPhysicalKeyPressed(Key.A);
        bool rightPressed = Input.IsPhysicalKeyPressed(Key.D);

        float direction = (rightPressed ? 1f : 0f) - (leftPressed ? 1f : 0f);
        float targetVelocity = direction * Constants.MOTORCYCLE_FREEROAM_SPEED;
        freeRoamVelocity = smoothTo(freeRoamVelocity, targetVelocity, FreeRoamAccelSmoothing, delta);

        state.BikeLaneOffset = Mathf.Clamp(
            state.BikeLaneOffset + freeRoamVelocity * (float)delta,
            -Constants.MOTORCYCLE_FREEROAM_BOUND,
            Constants.MOTORCYCLE_FREEROAM_BOUND
        );

        Bike.Position = new Vector3(state.BikeLaneOffset, Bike.Position.Y, Bike.Position.Z);
    }

    // Lean the bike toward the direction it's steering, purely as visual feedback.
    private void updateBikeLean(Attempt state, double delta)
    {
        float velocity = delta > 0 ? (state.BikeLaneOffset - previousPositionX) / (float)delta : 0f;
        previousPositionX = state.BikeLaneOffset;

        float targetLean = Mathf.Clamp(-velocity * 0.15f, -Mathf.Pi / 6, Mathf.Pi / 6);
        state.BikeLean = smoothTo(state.BikeLean, targetLean, LeanSmoothing, delta);
        Bike.Rotation = new Vector3(Bike.Rotation.X, Bike.Rotation.Y, state.BikeLean);
    }

    private void updateCamera(Attempt state, double delta)
    {
        if (Camera == null)
        {
            return;
        }

        // A touch of lag behind the bike instead of locking to it exactly -
        // reads as a real chase camera rather than a rigidly-attached one.
        cameraX = smoothTo(cameraX, state.BikeLaneOffset, CameraSmoothing, delta);
        Camera.Position = new Vector3(cameraX, Camera.Position.Y, Camera.Position.Z);

        state.CameraPosition = Camera.Position;
        state.CameraRotation = Camera.Rotation;
    }

    // Frame-rate independent exponential ease toward a target - decelerates
    // as it approaches instead of moving at a constant speed and stopping
    // dead, and never overshoots.
    private static float smoothTo(float current, float target, float sharpness, double delta)
    {
        return Mathf.Lerp(current, target, 1f - Mathf.Exp(-sharpness * (float)delta));
    }
}
