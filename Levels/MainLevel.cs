using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class MainLevel : Level
{
	public override List<Spawner> GetSpawners(Vector2 playerPosition)
	{
		return spawnersFolder.GetChildren()
			.Select(x => (Spawner)x)
			.Where(x => !x.VisibilityNotifier.IsOnScreen() && x.GlobalPosition.DistanceTo(playerPosition) < 1500)
			.ToList();
	}
}
