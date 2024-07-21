using Godot;
using System.Collections.Concurrent;
using System.Collections.Generic;

public static class ScenePool<[MustBeVariant]T>
	where T : Node
{
	private static ConcurrentStack<T> instances = new ConcurrentStack<T>();

	private static Dictionary<string, int> limits = new Dictionary<string, int>();
	private static Dictionary<string, PackedScene> scenes = new Dictionary<string, PackedScene>()
	{
		{ nameof(Bullet), GD.Load<PackedScene>("res://Entities/Player/PlayerObjects/Bullet.tscn") },
		{ nameof(Zombie), GD.Load<PackedScene>("res://Entities/Zombie/Zombie.tscn") }
	};

	private static int GetLimit(string objName)
	{
		if (!limits.ContainsKey(objName))
		{
			limits.Add(objName, 5);
		}

		limits[objName] *= 2;
		return limits[objName];
	}

	public static T Take()
	{
		if (instances.IsEmpty)
		{
			for (int i = 0; i < GetLimit(typeof(T).Name); i++)
			{
				var instance = (T)scenes[typeof(T).Name].Instantiate();
				instances.Push(instance);
			}
		}

		if (!instances.TryPop(out var obj))
		{
			return (T)scenes[typeof(T).Name].Instantiate();
		}

		return obj;
	}

	public static void Release(T obj)
	{
		Callable.From<T>(x =>
		{
			var parent = x.GetParent();
			if (parent != null)
			{
				parent.RemoveChild(x);
			}

			x.Owner = null;

			instances.Push(x);
		}).CallDeferred(obj);
	}
}
