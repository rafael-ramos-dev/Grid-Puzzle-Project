using Game.Autoload;
using Game.Component;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Game.Manager;

public partial class GridManager : Node
{
	[Export]
	private TileMapLayer highlightTileMapLayer;
	[Export]
	private TileMapLayer baseTerrainTileMapLayer;

	private HashSet<Vector2I> validBuildableTiles = new HashSet<Vector2I>();


    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += HandleBuildingPlaced;
    }


    public bool IsTilePositionValid(Vector2I tilePosition)
	{
		TileData customData = baseTerrainTileMapLayer.GetCellTileData(tilePosition);

		if (customData == null) return false;

		return (bool)customData.GetCustomData("Buildable");
	}


	public bool IsTilePositionBuildable(Vector2I tilePosition)
	{
		return validBuildableTiles.Contains(tilePosition);
	}


	public void HighlightBuildableTiles()
	{
		foreach (Vector2I tilePosition in validBuildableTiles)
		{
			// parameters can be checked inside the game engine on the TileMap subsection
            // by hovering the mouse over the tile inside it except for the first
            // parameter that is the current grid position the for loop is getting for us
            highlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}	


	public void ClearHighlightedTiles()
	{
		highlightTileMapLayer.Clear();
	}


	public Vector2I GetMouseGridCellPosition()
    {
        Vector2 mousePosition = highlightTileMapLayer.GetGlobalMousePosition();
        Vector2 gridPositionFloat = mousePosition / 64;
        gridPositionFloat = gridPositionFloat.Floor();

		Vector2I gridPosition = new Vector2I((int)gridPositionFloat.X, (int)gridPositionFloat.Y);

        return gridPosition;
    }

	
	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		Vector2I rootCell = buildingComponent.GetGridCellPosition();

		for (var x = rootCell.X - buildingComponent.BuildableRadius; x <= 
				rootCell.X + buildingComponent.BuildableRadius; x++)
        {
            for (var y = rootCell.Y - buildingComponent.BuildableRadius; y <= 
					rootCell.Y + buildingComponent.BuildableRadius; y++)
            {
				Vector2I tilePosition = new Vector2I(x, y);
				
				if (!IsTilePositionValid(tilePosition)) continue;

				validBuildableTiles.Add(tilePosition);
            }
        }

		validBuildableTiles.Remove(buildingComponent.GetGridCellPosition());
	}


	private void HandleBuildingPlaced(BuildingComponent buildingComponent)
    {
		UpdateValidBuildableTiles(buildingComponent);
    }
}
