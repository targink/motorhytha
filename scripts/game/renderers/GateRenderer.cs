using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

// Motorcycle mode's replacement for NoteRenderer: renders existing map Note
// objects as track gates approaching the bike, and resolves them via
// MotorcycleHitJudgment. Reuses Note (X = lane, Millisecond = timing)
// instead of a new map object type, so map parsing/format is untouched -
// GameComponent.Play() is what loads a Map's notes into Attempt.Objects.
public partial class GateRenderer : Renderer, IRenderer<Note>
{
    // How far ahead of the bike (world units / milliseconds) gates become visible.
    private const float ApproachDistance = 12f;

    private const float ApproachTimeMs = 1500f;

    // Same rounded-square mesh Rhythia uses for its notes, so gates read as
    // "the same game" instead of generic placeholder boxes.
    private const string GateMeshPath = "res://user/meshes/squircle.obj";

    private const float GateScale = 0.85f;

    private MultiMeshInstance3D gateMesh { get; set; }

    private readonly MotorcycleHitJudgment hitJudgment = new();

    private Color transparent = new Color(0x00000000);

    // One bright, saturated color per lane (left/center/right) so gates read
    // at a glance which lane they belong to, same idea as lane-colored notes
    // in other rhythm games.
    private static readonly Color[] laneColors =
    [
        Color.FromHtml("ff4d6d"),
        Color.FromHtml("ffd23f"),
        Color.FromHtml("3fa9ff"),
    ];

    public override void _Ready()
    {
        gateMesh = new()
        {
            MaterialOverride = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                VertexColorUseAsAlbedo = true,
                CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            },
            Multimesh = new()
            {
                TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                UseColors = true,
                Mesh = GD.Load<Mesh>(GateMeshPath),
            }
        };
        AddChild(gateMesh);
    }

    private static Color laneColor(int lane) => laneColors[lane + 1];

    public void Render(double delta, double time, IList<Note> gates)
    {
        if (gates.Count > gateMesh.Multimesh.InstanceCount)
        {
            gateMesh.Multimesh.InstanceCount = gates.Count;
        }

        for (int i = 0; i < gates.Count; i++)
        {
            Note gate = gates[i];
            double msUntilHit = gate.Millisecond - time;

            if (gate.Hit || msUntilHit < -Constants.HIT_WINDOW || msUntilHit > ApproachTimeMs)
            {
                gateMesh.Multimesh.SetInstanceColor(i, transparent);
                continue;
            }

            int lane = MotorcycleLanes.LaneFromNoteX(gate.X);
            float laneX = MotorcycleLanes.LaneWorldX(lane);
            float z = -(float)(msUntilHit / ApproachTimeMs) * ApproachDistance;

            // Fade in as it approaches and gently spin, echoing the classic note approach/fade.
            float depth = Mathf.Clamp((float)(msUntilHit / ApproachTimeMs), 0f, 1f);
            float alpha = 1f - Mathf.Pow(depth, 3f);
            Basis basis = Basis.Identity.Rotated(Vector3.Up, depth * Mathf.Pi * 0.5f).Scaled(Vector3.One * GateScale);

            gateMesh.Multimesh.SetInstanceTransform(i, new Transform3D(basis, new Vector3(laneX, 0, z)));

            Color color = laneColor(lane);
            color.A = alpha;
            gateMesh.Multimesh.SetInstanceColor(i, color);
        }
    }

    public override void Process(double delta, Attempt attempt)
    {
        if (!attempt.Objects.TryGetValue(typeof(Note), out IList<object> objects))
        {
            return;
        }

        List<Note> gates = objects.Cast<Note>().ToList();

        hitJudgment.ProcessGates(attempt, gates);
        Render(delta, attempt.Progress, gates);
    }
}
