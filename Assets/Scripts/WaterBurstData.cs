using UnityEngine;

public readonly struct WaterBurstData
{
    public WaterBurstData(Vector2 origin, Vector2 direction, int damage, float force, GameObject source)
    {
        Origin = origin;
        Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        Damage = damage;
        Force = force;
        Source = source;
    }

    public Vector2 Origin { get; }
    public Vector2 Direction { get; }
    public int Damage { get; }
    public float Force { get; }
    public GameObject Source { get; }
}
