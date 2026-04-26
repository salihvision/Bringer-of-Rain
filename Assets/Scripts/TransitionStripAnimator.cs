using System.Collections.Generic;
using UnityEngine;

public class TransitionStripAnimator : MonoBehaviour
{
    private static readonly Dictionary<string, Sprite[]> CachedFrames = new();

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private float frameDuration;
    private float nextFrameAt;
    private int frameIndex;

    private const int CellSize = 96;
    private const float PixelsPerUnit = 48f;

    public static void Spawn(string resourcePath, Vector3 position, float scale, float animationFrameDuration, int sortingOrder)
    {
        GameObject effectObject = new("TransitionStripAnimator");
        effectObject.transform.position = position;
        effectObject.transform.localScale = Vector3.one * scale;

        TransitionStripAnimator animator = effectObject.AddComponent<TransitionStripAnimator>();
        animator.Initialize(resourcePath, animationFrameDuration, sortingOrder);
    }

    private void Initialize(string resourcePath, float animationFrameDuration, int sortingOrder)
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = sortingOrder;
        frameDuration = animationFrameDuration;
        frames = LoadFrames(resourcePath);
        frameIndex = 0;
        spriteRenderer.sprite = frames.Length > 0 ? frames[0] : PrimitiveSpriteLibrary.SquareSprite;
        nextFrameAt = Time.time + frameDuration;
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time < nextFrameAt)
        {
            return;
        }

        frameIndex++;
        if (frameIndex >= frames.Length)
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer.sprite = frames[frameIndex];
        nextFrameAt = Time.time + frameDuration;
    }

    private static Sprite[] LoadFrames(string resourcePath)
    {
        if (CachedFrames.TryGetValue(resourcePath, out Sprite[] cached))
        {
            return cached;
        }

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            Debug.LogWarning($"Transition strip not found at Resources/{resourcePath}.");
            Sprite[] fallback = { PrimitiveSpriteLibrary.SquareSprite };
            CachedFrames[resourcePath] = fallback;
            return fallback;
        }

        texture.filterMode = FilterMode.Point;
        int frameCount = Mathf.Max(1, texture.width / CellSize);
        Sprite[] loadedFrames = new Sprite[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            Rect rect = new(i * CellSize, 0f, CellSize, CellSize);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit);
            sprite.name = $"{resourcePath}_{i}";
            loadedFrames[i] = sprite;
        }

        CachedFrames[resourcePath] = loadedFrames;
        return loadedFrames;
    }
}
