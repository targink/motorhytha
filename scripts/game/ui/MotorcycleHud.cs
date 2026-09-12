using Godot;

// Score/combo/health readout for motorcycle mode, what you're actually
// playing (song title, or which no-map mode is active), and a center-screen
// message while paused or failed.
public partial class MotorcycleHud : UIComponent
{
    [Export]
    public Label3D Label { get; set; }

    [Export]
    public Label3D SongLabel { get; set; }

    [Export]
    public Label3D StatusLabel { get; set; }

    private bool songLabelSet;

    public override void ApplySettings(SettingsProfile settings)
    {
    }

    public override void Process(double delta, Attempt state)
    {
        string grade = string.IsNullOrEmpty(state.LastHitGrade) ? "" : $"\n{state.LastHitGrade}";
        Label.Text = $"Score {state.Score}\nCombo {state.Combo}\nHealth {Mathf.RoundToInt((float)state.Health)}%{grade}";

        if (!songLabelSet)
        {
            SongLabel.Text = state.Map != null
                ? (string.IsNullOrEmpty(state.Map.PrettyTitle) ? state.Map.Name : state.Map.PrettyTitle)
                : (state.FreeRoam ? "Freeroam" : "Free Drive");
            songLabelSet = true;
        }

        StatusLabel.Text = state.IsFailed
            ? "FAILED\nQ: Quit to Menu"
            : state.Paused
                ? "PAUSED\nEsc: Resume    Q: Quit to Menu"
                : "";
    }
}
