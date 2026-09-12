using System.Collections.Generic;
using System.Linq;
using Godot;

// Map-select screen for motorcycle mode: lists whatever maps MapManager has
// already cached, lets the player import an old map file directly (.sspm/
// .phxm/.txt, same formats/parser Rhythia already supports), and offers a
// Free Drive option. Hands the choice off to GameComponent (via
// MotorcycleSelection) before switching to motorcycle.tscn.
public partial class MotorcycleSelect : Control
{
    [Export]
    public VBoxContainer MapList { get; set; }

    [Export]
    public Button FreeDriveButton { get; set; }

    [Export]
    public Button ImportButton { get; set; }

    [Export]
    public FileDialog ImportDialog { get; set; }

    [Export]
    public Label StatusLabel { get; set; }

    public override void _Ready()
    {
        FreeDriveButton.Pressed += () => selectMap(null);
        ImportButton.Pressed += () => ImportDialog.Show();
        ImportDialog.FilesSelected += onFilesSelected;
        MapParser.Instance.MapsImportFinished += onImportFinished;

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

    private async void onFilesSelected(string[] paths)
    {
        StatusLabel.Text = $"Importing {paths.Length} map(s)...";
        await MapParser.BulkImport(paths);
    }

    private void onImportFinished(Map[] imported)
    {
        MapCache.OrderAndSetMaps();

        foreach (Node child in MapList.GetChildren())
        {
            child.QueueFree();
        }

        populate(MapManager.Maps);

        if (imported.Length > 0)
        {
            StatusLabel.Text = $"Imported {imported.Length} map(s)";
        }
    }

    private void populate(List<Map> maps)
    {
        if (MapList.GetChildCount() == 0)
        {
            StatusLabel.Text = maps.Count > 0
                ? $"{maps.Count} map(s) found - pick one, import more, or Free Drive"
                : "No maps yet - press Import Map to add an .sspm/.phxm/.txt file, or press Free Drive";
        }

        foreach (Map map in maps.Where(map => !MapList.GetChildren().OfType<Button>().Any(b => b.Text == mapLabel(map))))
        {
            Button button = new() { Text = mapLabel(map) };
            button.Pressed += () => selectMap(map);
            MapList.AddChild(button);
        }
    }

    private static string mapLabel(Map map) => string.IsNullOrEmpty(map.PrettyTitle) ? map.Name : map.PrettyTitle;

    private void selectMap(Map map)
    {
        MotorcycleSelection.SelectedMap = map;
        MotorcycleSelection.HasSelection = true;
        GetTree().ChangeSceneToFile("res://scenes/motorcycle.tscn");
    }
}
