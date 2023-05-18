using Godot;
using System.Collections.Generic;

public static class Utilities 
{
	public static int RandNum(int range)
	{
		return Mathf.Abs((int)GD.Randi() % range);
	}

	public static bool IsLastFrame(AnimatedSprite sprite)
	{
		return sprite.Frame == sprite.Frames.GetFrameCount(sprite.Animation) - 1;
	}

    public static PackedScene GetRandomSceneFromList(List<PackedScene> packedScenesList)
    {
        return packedScenesList[Utilities.RandNum(packedScenesList.Count)];
    }
}
