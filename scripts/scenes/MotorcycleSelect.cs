using System.Collections.Generic;
using Godot;

// Map-select screen for motorcycle mode: lists whatever maps MapManager has
// already cached, plus a Free Drive option, and hands the choice off to
// GameComponent (via MotorcycleSelection) before switching to motorcycle.tscn.
public partial class MotorcycleSelect : Control
{
    [Export]
    public VBoxContainer MapList { get; set; }

    [Export]
    public Button FreeDriveButton { get; set; }

    [Export]
    public Label StatusLabel { get; set; }

    public override void _Ready()
    {
        FreeDriveButton.Pressed += () => selectMap(null);

        if (MapManager.Initialized)
        {
            populate(MapManager.Maps);
        }
        else
        {
            StatusLabel.Text = "Loading maps...";
            MapManager.MapsInitialized += populate;
        }
    }

    private void populate(List<Map> maps)
    {
        StatusLabel.Text = maps.Count > 0
            ? $"{maps.Count} map(s) found - pick one, or Free Drive below"
            : "No maps found - import one into your user folder's maps/ directory, or press Free Drive";

        foreach (Map map in maps)
        {
            Button button = new()
            {
                Text = string.IsNullOrEmpty(map.PrettyTitle) ? map.Name : map.PrettyTitle,
            };
            button.Pressed += () => selectMap(map);
            MapList.AddChild(button);
        }
    }

    private void selectMap(Map map)
    {
        MotorcycleSelection.SelectedMap = map;
        MotorcycleSelection.HasSelection = true;
        GetTree().ChangeSceneToFile("res://scenes/motorcycle.tscn");
    }
}
