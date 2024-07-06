using Godot;

namespace Shootem.Entities
{
    public partial class Pickable : Area2D
    {
        [Signal] public delegate void PickedEventHandler(Pickable pickable);

        public override void _Ready()
        {
            BodyEntered += OnPickableBodyEntered;
            Picked += GetNode<World>("/root/World").OnPicked; //?
        }

        public void OnPickableBodyEntered(Node body)
        {
            if (body is Player)
            {
                EmitSignal(SignalName.Picked, this);
                QueueFree();
            }
        }
    }
}
