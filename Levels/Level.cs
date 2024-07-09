using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Level : Node
{
    protected Node spawnersFolder;

    public override void _Ready()
    {
        spawnersFolder = GetNode<Node>("Spawns");
    }

    public virtual List<Spawner> GetSpawners(Vector2 playerPosition) => 
        spawnersFolder.GetChildren()
            .Select(x => (Spawner)x)
            .Where(x => !x.VisibilityNotifier.IsOnScreen())
            .ToList();

    public virtual void AddBonus(Bonus bonus, Vector2 position)
    {
        bonus.Position = position;
        CallDeferred(Node.MethodName.AddChild, bonus);
    }
}
