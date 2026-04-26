using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemySpriteAnimator : MonoBehaviour
{
    private static readonly Dictionary<string, Sprite[]> CachedAnimations = new();

    private EnemyPatrol enemy;
    private SpriteRenderer spriteRenderer;

    private string resourceFolder = "Enemies/MaskDude";
    private string currentClip;
    private Sprite[] activeFrames;
    private float nextFrameAt;
    private float frameDuration;
    private int frameIndex;

    private const int CellSize = 32;
    private const float PixelsPerUnit = 24f;

    public void Configure(string folderName)
    {
        if (!string.IsNullOrWhiteSpace(folderName))
        {
            resourceFolder = $"Enemies/{folderName}";
        }
    }

    private void Awake()
    {
        enemy = GetComponent<EnemyPatrol>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetClip("Idle", true);
    }

    private void Update()
    {
        string nextClip = ResolveClip();
        bool restart = nextClip != currentClip;
        SetClip(nextClip, restart);

        if (activeFrames == null || activeFrames.Length == 0)
        {
            return;
        }

        if (Time.time >= nextFrameAt)
        {
            frameIndex = (frameIndex + 1) % activeFrames.Length;
            spriteRenderer.sprite = activeFrames[frameIndex];
            nextFrameAt = Time.time + frameDuration;
        }
    }

    private string ResolveClip()
    {
        if (enemy.IsStunned)
        {
            return "Hit";
        }

        if (enemy.IsWindingUp)
        {
            return "Idle";
        }

        if (enemy.IsAlert || enemy.IsMoving)
        {
            return "Run";
        }

        return "Idle";
    }

    private void SetClip(string clipName, bool forceRestart)
    {
        if (!forceRestart && clipName == currentClip)
        {
            return;
        }

        currentClip = clipName;
        activeFrames = LoadFrames(clipName);
        frameDuration = clipName switch
        {
            "Hit" => 0.07f,
            "Run" => enemy.IsDashing ? 0.05f : 0.09f,
            _ => enemy.IsWindingUp ? 0.12f : 0.18f
        };

        frameIndex = 0;
        spriteRenderer.sprite = activeFrames.Length > 0 ? activeFrames[0] : PrimitiveSpriteLibrary.SquareSprite;
        nextFrameAt = Time.time + frameDuration;
    }

    private Sprite[] LoadFrames(string clipName)
    {
        string cacheKey = $"{resourceFolder}/{clipName}";
        if (CachedAnimations.TryGetValue(cacheKey, out Sprite[] frames))
        {
            return frames;
        }

        Texture2D texture = Resources.Load<Texture2D>(cacheKey);
        if (texture == null)
        {
            Debug.LogWarning($"Enemy animation strip not found at Resources/{cacheKey}.");
            frames = new[] { PrimitiveSpriteLibrary.SquareSprite };
            CachedAnimations[cacheKey] = frames;
            return frames;
        }

        texture.filterMode = FilterMode.Point;
        int frameCount = Mathf.Max(1, texture.width / CellSize);
        frames = new Sprite[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            Rect rect = new(i * CellSize, 0f, CellSize, CellSize);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit);
            sprite.name = $"{clipName}_{i}";
            frames[i] = sprite;
        }

        CachedAnimations[cacheKey] = frames;
        return frames;
    }
}
