using Godot;
using System.Collections.Generic;

public static class Utilities 
{
	public static int RandNum(int range)
	{
		return Mathf.Abs((int)GD.Randi() % range);
	}

	public static bool IsLastFrame(AnimatedSprite2D sprite)
	{
		return sprite.Frame == sprite.SpriteFrames.GetFrameCount(sprite.Animation) - 1;
	}

	public static PackedScene GetRandomSceneFromList(List<PackedScene> packedScenesList)
	{
		return packedScenesList[RandNum(packedScenesList.Count)];
	}
}
