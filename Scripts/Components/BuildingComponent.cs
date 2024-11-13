using Game.Autoload;
using Godot;

namespace Game.Component;


public partial class BuildingComponent : Node2D
{
	[Export]
	public int BuildableRadius { get; private set; }


	public override void _Ready()
	{
		AddToGroup(nameof(BuildingComponent));

		// The Callable.From() has been used to be possible to use "CallDeferred()" for the
		// method inside the Callable.From(). CallDeferred() asks Godot to call this method
		// last (after the rest of the code has run) so the rest of the code (other components as well) 
		// is in order when this method get called.
		Callable.From(() => GameEvents.EmitBuildingPlaced(this)).CallDeferred();
	}


	public Vector2I GetGridCellPosition()
	{
        Vector2 gridPositionFloat = GlobalPosition / 64;
        gridPositionFloat = gridPositionFloat.Floor();

		Vector2I gridPosition = new Vector2I((int)gridPositionFloat.X, (int)gridPositionFloat.Y);

        return gridPosition;
	}
}
