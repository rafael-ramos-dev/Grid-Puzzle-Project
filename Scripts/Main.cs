using Godot;
using Game.Manager;

namespace Game;

public partial class Main : Node
{
    private GridManager gridManager;
    private Sprite2D cursor;
    private PackedScene buildingScene;
    private Button placeBuildingButton;

    private Vector2I? hoveredGridCell;    


    public override void _Ready()
    {
        buildingScene = GD.Load<PackedScene>("res://Scenes/Building/Building.tscn");
        gridManager = GetNode<GridManager>("GridManager");
        cursor = GetNode<Sprite2D>("Cursor");
        placeBuildingButton = GetNode<Button>("PlaceBuildingButton");

        cursor.Visible = false;

        placeBuildingButton.Pressed += HandlePlaceBuildingButtonPressed;
        // Another way to connect to a signal
        // placeBuildingButton.Connect(Button.SignalName.Pressed, Callable.From(HandlePlaceBuildingButtonPressed));
    }
    

    public override void _UnhandledInput(InputEvent evt)
    {
        if (hoveredGridCell.HasValue && evt.IsActionPressed("left_click") && 
            gridManager.IsTilePositionBuildable(hoveredGridCell.Value))
        {
            PlaceBuildingAtHoveredCellPosition();
            cursor.Visible = false;
        }
    }


    public override void _Process(double delta)
    {
        Vector2I gridPosition = gridManager.GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;

        if (cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell != gridPosition))
        {
            hoveredGridCell = gridPosition;
            gridManager.HighlightBuildableTiles();
        }
    }    


    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!hoveredGridCell.HasValue) { return; }

        Node2D building = buildingScene.Instantiate<Node2D>();
        AddChild(building);

        building.GlobalPosition = hoveredGridCell.Value * 64;

        hoveredGridCell = null;
        gridManager.ClearHighlightedTiles();
    }    


    private void HandlePlaceBuildingButtonPressed()
    {
        cursor.Visible = true;
    }
}
