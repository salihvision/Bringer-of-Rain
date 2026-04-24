using UnityEngine;

public class WaterBurstVisual : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 startScale;
    private Vector3 endScale;
    private float lifetime;
    private float elapsed;

    public static void Spawn(Vector2 position, Vector2 size, float facingSign)
    {
        GameObject visualObject = new("WaterBurstVisual");
        visualObject.transform.position = new Vector3(position.x, position.y, -0.5f);

        SpriteRenderer renderer = visualObject.AddComponent<SpriteRenderer>();
        renderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        renderer.color = new Color(0.16f, 0.86f, 1f, 0.75f);
        renderer.sortingOrder = 15;

        WaterBurstVisual visual = visualObject.AddComponent<WaterBurstVisual>();
        visual.Initialize(renderer, size, facingSign);
    }

    private void Initialize(SpriteRenderer renderer, Vector2 size, float facingSign)
    {
        spriteRenderer = renderer;
        lifetime = 0.14f;

        float sign = facingSign == 0f ? 1f : Mathf.Sign(facingSign);
        startScale = new Vector3(size.x * 0.8f * sign, size.y * 0.8f, 1f);
        endScale = new Vector3(size.x * 1.2f * sign, size.y * 1.15f, 1f);
        transform.localScale = startScale;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / lifetime);

        transform.localScale = Vector3.Lerp(startScale, endScale, progress);

        Color color = spriteRenderer.color;
        color.a = Mathf.Lerp(0.75f, 0f, progress);
        spriteRenderer.color = color;

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
