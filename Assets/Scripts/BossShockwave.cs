using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BossShockwave : MonoBehaviour
{
    private const float Lifetime = 1.55f;
    private const float TravelSpeed = 7f;
    private const int Damage = 1;

    private SpriteRenderer waveRenderer;
    private float bornAt;
    private float horizontalSign;
    private float nextHitAt;

    public static void Spawn(Vector2 origin, float direction)
    {
        GameObject waveObject = new("BossShockwave");
        waveObject.transform.position = new Vector3(origin.x, origin.y, -0.4f);
        waveObject.transform.localScale = new Vector3(1.2f, 0.55f, 1f);

        SpriteRenderer renderer = waveObject.AddComponent<SpriteRenderer>();
        renderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        renderer.color = new Color(0.18f, 0.86f, 1f, 0.92f);
        renderer.sortingOrder = 12;

        BoxCollider2D collider2D = waveObject.AddComponent<BoxCollider2D>();
        collider2D.size = Vector2.one;
        collider2D.isTrigger = true;

        BossShockwave wave = waveObject.AddComponent<BossShockwave>();
        wave.waveRenderer = renderer;
        wave.bornAt = Time.time;
        wave.horizontalSign = direction >= 0f ? 1f : -1f;
    }

    private void Update()
    {
        float age = (Time.time - bornAt) / Lifetime;
        if (age >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 position = transform.position;
        position.x += horizontalSign * TravelSpeed * Time.deltaTime;
        transform.position = position;

        Vector3 scale = transform.localScale;
        scale.y = Mathf.Lerp(0.55f, 0.18f, age);
        transform.localScale = scale;

        Color color = waveRenderer.color;
        color.a = Mathf.Lerp(0.92f, 0f, age);
        waveRenderer.color = color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (Time.time < nextHitAt)
        {
            return;
        }

        if (other.TryGetComponent(out PlayerController hitPlayer))
        {
            nextHitAt = Time.time + 0.6f;
            hitPlayer.TakeDamage(Damage, transform.position);
        }
    }
}
