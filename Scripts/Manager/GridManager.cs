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


	public void HighlightExpandedBuildableTiles(Vector2I rootCell, int radius)
	{
		ClearHighlightedTiles();
		HighlightBuildableTiles();

		HashSet<Vector2I> validTiles = GetValidTilesInRadius(rootCell, radius).ToHashSet();

		IEnumerable<Vector2I> expandedTiles = validTiles.Except(validBuildableTiles)
			.Except(GetOccupiedTiles());

		Vector2I atlasCoords = new Vector2I(1, 0);

		// foreach (Vector2I tilePosition in expandedTiles)
		// {
        //     highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
		// }

		// The same ForEach loop above written in LINQ
		expandedTiles.ToList().ForEach(tilePosition => highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords));
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
		List<Vector2I> validTiles = GetValidTilesInRadius(rootCell, buildingComponent.BuildableRadius);

		validBuildableTiles.UnionWith(validTiles);
		
		validBuildableTiles.ExceptWith(GetOccupiedTiles());
	}


	private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
	{
		// List<Vector2I> result = new List<Vector2I>();
		// for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        // {
        //     for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
        //     {
		// 		Vector2I tilePosition = new Vector2I(x, y);
				
		// 		if (!IsTilePositionValid(tilePosition)) continue;

		// 		result.Add(tilePosition);
        //     }
        // }
		// return result;

		// Bellow is the same as the above but written with LINQ
		return Enumerable.Range(rootCell.X - radius, radius * 2 + 1)
        .SelectMany(x => Enumerable.Range(rootCell.Y - radius, radius * 2 + 1)
        .Select(y => new Vector2I(x, y)))
        .Where(tilePosition => IsTilePositionValid(tilePosition))
        .ToList();
	}

 
	private IEnumerable<Vector2I> GetOccupiedTiles()
	{
		IEnumerable<BuildingComponent> buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent))
			.Cast<BuildingComponent>();
		
		IEnumerable<Vector2I> occupiedTiles = buildingComponents.Select(x => x.GetGridCellPosition());

		return occupiedTiles;
	}


	private void HandleBuildingPlaced(BuildingComponent buildingComponent)
    {
		UpdateValidBuildableTiles(buildingComponent);
    }
}
