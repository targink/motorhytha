// Carries the player's map choice from MotorcycleSelect (the map-select
// screen) across the scene change into motorcycle.tscn's GameComponent,
// since the two scenes don't share a node tree.
public static class MotorcycleSelection
{
    public static Map SelectedMap;

    public static bool HasSelection;
}
