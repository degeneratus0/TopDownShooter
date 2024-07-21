using Godot;
using Godot.Collections;
using System.Collections.Concurrent;

public static class NodePool<T> where T : Node, new()
{
    private static ConcurrentStack<T> objects = new ConcurrentStack<T>();
    private static Dictionary<string, int> limits = new Dictionary<string, int>();

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
        if (objects.IsEmpty)
        {
            for (int i = 0; i < GetLimit(typeof(T).Name); i++)
            {
                var newObject = new T();
                objects.Push(newObject);
            }
        }

        if (!objects.TryPop(out var obj))
        {
            return new T();
        }

        return obj;
    }

    public static void Release(T obj)
    {
        obj.Owner = null;

        objects.Push(obj);
    }
}