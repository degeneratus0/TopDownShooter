using Godot;

public static class Controls
{
    public static readonly StringName Left = new StringName("left");
    public static readonly StringName Right = new StringName("right");
    public static readonly StringName Up = new StringName("up");
    public static readonly StringName Down = new StringName("down");

    public static readonly StringName Melee = new StringName("melee");
    public static readonly StringName Shoot = new StringName("shoot");
    public static readonly StringName Reload = new StringName("reload");
    public static readonly StringName LaserSwitch = new StringName("laser_switch");

    public static readonly StringName ToggleFullscreen = "toggle_fullscreen";
    public static readonly StringName Quit = new StringName("quit");
    public static readonly StringName Menu = new StringName("menu");
}

