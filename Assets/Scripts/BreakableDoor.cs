using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BreakableDoor : MonoBehaviour, IWaterReactive
{
    [SerializeField] private int health = 3;
    [SerializeField] private GameObject particlePrefab;

    public void ReactToWaterBurst(WaterBurstData burst)
    {
        if (health <= 0) return;

        health--;
        
        if (health <= 0)
        {
            Break();
        }
    }

    private void Break()
    {
        if (particlePrefab != null)
        {
            Instantiate(particlePrefab, transform.position, Quaternion.identity);
        }
        
        gameObject.SetActive(false);
    }
}
