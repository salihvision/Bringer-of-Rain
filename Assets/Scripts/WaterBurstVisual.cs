using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WaterBurstVisual : MonoBehaviour
{
    private static readonly Dictionary<string, Sprite[]> CachedSprites = new();

    // Sprite Animation fields
    private Sprite[] frames;
    private int frameIndex;
    private float frameTime;
    private float nextFrameAt;
    private SpriteRenderer spriteRenderer;
    private Transform attachedTransform;
    private Vector3 attachOffset;

    // Procedural fields
    private bool isProcedural;
    private readonly List<SpriteRenderer> segmentRenderers = new();
    private readonly List<Vector3> startPositions = new();
    private readonly List<Vector3> endPositions = new();
    private readonly List<Vector3> startScales = new();
    private readonly List<Vector3> endScales = new();
    private float lifetime;
    private float elapsed;

    public static void Spawn(Vector2 position, float facingSign, bool isDownward, int variant = 0, Transform attachTo = null)
    {
        GameObject visualObject = new("WaterBurstVisual");
        visualObject.transform.position = new Vector3(position.x, position.y, -0.5f);

        WaterBurstVisual visual = visualObject.AddComponent<WaterBurstVisual>();
        visual.InitializeSprite(facingSign, isDownward, variant, attachTo);
    }

    public static void Spawn(Vector2 position, Vector2 size, float facingSign)
    {
        GameObject visualObject = new("WaterBurstVisual");
        visualObject.transform.position = new Vector3(position.x, position.y, -0.5f);

        WaterBurstVisual visual = visualObject.AddComponent<WaterBurstVisual>();
        visual.InitializeProcedural(size, facingSign);
    }

    private void InitializeSprite(float facingSign, bool isDownward, int variant, Transform attachTo)
    {
        isProcedural = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 18;
        
        attachedTransform = attachTo;
        if (attachTo != null)
        {
            attachOffset = transform.position - attachTo.position;
        }

        if (isDownward)
        {
            frames = GetOrLoadSprites("downward", "Effects/downwardattacksplash", 192, 5, 27, 0, new Vector2(0.05f, 0.5f));
            transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            float sign = facingSign == 0f ? 1f : Mathf.Sign(facingSign);
            transform.localScale = new Vector3(1.5f, 1.5f * sign, 1f);
            frameTime = 0.015f;
        }
        else
        {
            int startIndex = variant == 0 ? 0 : 18;
            frames = GetOrLoadSprites("whip" + variant, "Effects/whipattacksplash", 256, 5, 16, startIndex, new Vector2(0.35f, 0.5f));
            float sign = facingSign == 0f ? 1f : Mathf.Sign(facingSign);
            transform.localScale = new Vector3(sign * 1.6f, 1.6f, 1f);
            frameTime = 0.018f;
        }

        frameIndex = 0;
        ApplyFrame();
        nextFrameAt = Time.time + frameTime;
    }

    private void InitializeProcedural(Vector2 size, float facingSign)
    {
        isProcedural = true;
        lifetime = 0.16f;
        elapsed = 0f;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = null;

        float sign = facingSign == 0f ? 1f : Mathf.Sign(facingSign);
        CreateSegment(new Vector3(0.18f * sign, -0.06f, 0f), new Vector3(size.x * 0.2f * sign, size.y * 0.76f, 1f), new Vector3(0.55f * sign, 0f, 0f), new Vector3(size.x * 0.24f * sign, size.y * 0.86f, 1f), new Color(0.12f, 0.72f, 0.92f, 0.5f), 14);
        CreateSegment(new Vector3(0.82f * sign, -0.01f, 0f), new Vector3(size.x * 0.28f * sign, size.y * 0.68f, 1f), new Vector3(1.35f * sign, 0.08f, 0f), new Vector3(size.x * 0.34f * sign, size.y * 0.8f, 1f), new Color(0.18f, 0.86f, 1f, 0.64f), 15);
        CreateSegment(new Vector3(1.6f * sign, 0.08f, 0f), new Vector3(size.x * 0.34f * sign, size.y * 0.52f, 1f), new Vector3(2.18f * sign, 0.14f, 0f), new Vector3(size.x * 0.42f * sign, size.y * 0.6f, 1f), new Color(0.7f, 1f, 1f, 0.8f), 16);
        CreateSegment(new Vector3(2.54f * sign, 0.15f, 0f), new Vector3(size.x * 0.14f * sign, size.y * 0.34f, 1f), new Vector3(3.02f * sign, 0.22f, 0f), new Vector3(size.x * 0.18f * sign, size.y * 0.4f, 1f), new Color(0.92f, 1f, 1f, 0.9f), 17);
    }

    private void CreateSegment(Vector3 localStartPosition, Vector3 localStartScale, Vector3 localEndPosition, Vector3 localEndScale, Color color, int sortingOrder)
    {
        GameObject segment = new("Segment");
        segment.transform.SetParent(transform, false);
        segment.transform.localPosition = localStartPosition;
        segment.transform.localScale = localStartScale;

        SpriteRenderer segmentRenderer = segment.AddComponent<SpriteRenderer>();
        segmentRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        segmentRenderer.color = color;
        segmentRenderer.sortingOrder = sortingOrder;

        segmentRenderers.Add(segmentRenderer);
        startPositions.Add(localStartPosition);
        endPositions.Add(localEndPosition);
        startScales.Add(localStartScale);
        endScales.Add(localEndScale);
    }

    private void Update()
    {
        if (isProcedural)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / lifetime);

            for (int i = 0; i < segmentRenderers.Count; i++)
            {
                SpriteRenderer renderer = segmentRenderers[i];
                renderer.transform.localPosition = Vector3.Lerp(startPositions[i], endPositions[i], progress);
                renderer.transform.localScale = Vector3.Lerp(startScales[i], endScales[i], progress);

                Color color = renderer.color;
                color.a = Mathf.Lerp(color.a, 0f, progress);
                renderer.color = color;
            }

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (attachedTransform != null)
            {
                transform.position = attachedTransform.position + attachOffset;
            }

            if (Time.time >= nextFrameAt)
            {
                frameIndex++;
                if (frameIndex >= frames.Length)
                {
                    Destroy(gameObject);
                }
                else
                {
                    ApplyFrame();
                    nextFrameAt = Time.time + frameTime;
                }
            }
        }
    }

    private void ApplyFrame()
    {
        spriteRenderer.sprite = frames[frameIndex];
    }

    private static Sprite[] GetOrLoadSprites(string key, string path, int cellSize, int cols, int frameCount, int startIndex, Vector2 pivot)
    {
        if (CachedSprites.TryGetValue(key, out Sprite[] sprites))
        {
            return sprites;
        }

        Texture2D texture = Resources.Load<Texture2D>(path);
        if (texture == null)
        {
            Debug.LogError($"Sprite sheet not found at Resources/{path}");
            return new Sprite[0];
        }

        texture.filterMode = FilterMode.Bilinear;

        sprites = new Sprite[frameCount];
        int count = 0;
        for (int i = startIndex; i < startIndex + frameCount; i++)
        {
            int x = i % cols;
            int y = i / cols;

            int yCoord = texture.height - ((y + 1) * cellSize);
            Rect rect = new Rect(x * cellSize, yCoord, cellSize, cellSize);
            sprites[count] = Sprite.Create(texture, rect, pivot, 100f);
            count++;
        }

        CachedSprites[key] = sprites;
        return sprites;
    }
}
