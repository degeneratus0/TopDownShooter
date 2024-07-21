using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Level : Node
{
    protected Node spawnersFolder;
    protected Marker2D playerSpawn;

    private PackedScene playerScene = GD.Load<PackedScene>("res://Entities/Player/Player.tscn");

    public override void _Ready()
    {
        spawnersFolder = GetNode<Node>("Spawns");
        playerSpawn = GetNode<Marker2D>("PlayerSpawn");

        var player = (Player)playerScene.Instantiate();
        player.Position = playerSpawn.Position;
        AddChild(player);

        var remoteTransform = new RemoteTransform2D();
        remoteTransform.RemotePath = GetParent().GetNode<Camera2D>("Camera2D").GetPath();
        player.AddChild(remoteTransform);
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
