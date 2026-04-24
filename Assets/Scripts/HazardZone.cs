using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HazardZone : MonoBehaviour
{
    private readonly Dictionary<int, float> nextDamageByTarget = new();

    private bool instantRespawn;
    private int damage;
    private float damageInterval;

    public void Configure(int damageAmount, bool respawnImmediately, float repeatInterval)
    {
        damage = damageAmount;
        instantRespawn = respawnImmediately;
        damageInterval = repeatInterval;
    }

    private void Awake()
    {
        BoxCollider2D collider2D = GetComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ApplyHazard(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        ApplyHazard(other);
    }

    private void ApplyHazard(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController player))
        {
            return;
        }

        if (instantRespawn)
        {
            player.ForceRespawn();
            return;
        }

        int id = player.gameObject.GetHashCode();
        if (nextDamageByTarget.TryGetValue(id, out float nextTime) && Time.time < nextTime)
        {
            return;
        }

        nextDamageByTarget[id] = Time.time + damageInterval;
        player.TakeDamage(damage, transform.position);
    }
}
