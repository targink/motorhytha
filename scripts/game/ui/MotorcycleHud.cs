using Godot;

// Minimal score/combo readout for motorcycle mode, plus what you're
// actually playing (song title, or which no-map mode is active) since
// there's no other on-screen context once you're in motorcycle.tscn.
public partial class MotorcycleHud : UIComponent
{
    [Export]
    public Label3D Label { get; set; }

    [Export]
    public Label3D SongLabel { get; set; }

    private bool songLabelSet;

    public override void ApplySettings(SettingsProfile settings)
    {
    }

    public override void Process(double delta, Attempt state)
    {
        Label.Text = $"Score {state.Score}\nCombo {state.Combo}";

        if (songLabelSet)
        {
            return;
        }

        SongLabel.Text = state.Map != null
            ? (string.IsNullOrEmpty(state.Map.PrettyTitle) ? state.Map.Name : state.Map.PrettyTitle)
            : (state.FreeRoam ? "Freeroam" : "Free Drive");
        songLabelSet = true;
    }
}
