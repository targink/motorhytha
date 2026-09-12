using Godot;

// Minimal score/combo readout for motorcycle mode.
public partial class MotorcycleHud : UIComponent
{
    [Export]
    public Label3D Label { get; set; }

    public override void ApplySettings(SettingsProfile settings)
    {
    }

    public override void Process(double delta, Attempt state)
    {
        Label.Text = $"Score {state.Score}\nCombo {state.Combo}";
    }
}
