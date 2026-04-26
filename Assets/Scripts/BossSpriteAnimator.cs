using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BossController))]
[RequireComponent(typeof(SpriteRenderer))]
public class BossSpriteAnimator : MonoBehaviour
{
    private static readonly Dictionary<string, Sprite[]> CachedAnimations = new();

    private const string ResourceFolder = "Bosses/DemonSlime";
    private const float PixelsPerUnit = 36f;

    private BossController boss;
    private SpriteRenderer spriteRenderer;
    private Sprite[] activeFrames;
    private string currentClip;
    private float nextFrameAt;
    private float frameDuration;
    private int frameIndex;
    private bool currentClipLoops = true;

    private void Awake()
    {
        boss = GetComponent<BossController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetClip("Idle", true);
    }

    private void Update()
    {
        string nextClip = ResolveClip();
        if (nextClip != currentClip)
        {
            SetClip(nextClip, true);
        }

        if (activeFrames == null || activeFrames.Length == 0)
        {
            return;
        }

        if (Time.time >= nextFrameAt)
        {
            if (currentClipLoops)
            {
                frameIndex = (frameIndex + 1) % activeFrames.Length;
            }
            else if (frameIndex < activeFrames.Length - 1)
            {
                frameIndex++;
            }
            spriteRenderer.sprite = activeFrames[frameIndex];
            nextFrameAt = Time.time + frameDuration;
        }
    }

    private string ResolveClip()
    {
        if (boss.IsDefeated)
        {
            return "Death";
        }

        if (boss.IsSlamming)
        {
            return "Ground";
        }

        if (boss.IsExposed)
        {
            return "Hit";
        }

        if (boss.IsDashing)
        {
            return "Run";
        }

        if (boss.IsTelegraphing)
        {
            return "Attack";
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
        currentClipLoops = clipName != "Death";
        activeFrames = LoadFrames(clipName);
        frameDuration = clipName switch
        {
            "Hit" => 0.06f,
            "Ground" => 0.07f,
            "Run" => boss.IsDashing ? 0.05f : 0.07f,
            "Attack" => 0.08f,
            "Death" => 0.075f,
            _ => 0.16f
        };

        frameIndex = 0;
        if (activeFrames.Length > 0)
        {
            spriteRenderer.sprite = activeFrames[0];
        }
        nextFrameAt = Time.time + frameDuration;
    }

    private Sprite[] LoadFrames(string clipName)
    {
        string cacheKey = $"{ResourceFolder}/{clipName}";
        if (CachedAnimations.TryGetValue(cacheKey, out Sprite[] cached))
        {
            return cached;
        }

        Texture2D[] textures = Resources.LoadAll<Texture2D>(cacheKey);
        if (textures == null || textures.Length == 0)
        {
            Debug.LogWarning($"Boss animation frames not found at Resources/{cacheKey}.");
            Sprite[] fallback = { PrimitiveSpriteLibrary.SquareSprite };
            CachedAnimations[cacheKey] = fallback;
            return fallback;
        }

        System.Array.Sort(textures, (a, b) => ExtractTrailingNumber(a.name).CompareTo(ExtractTrailingNumber(b.name)));

        Sprite[] frames = new Sprite[textures.Length];
        for (int i = 0; i < textures.Length; i++)
        {
            Texture2D texture = textures[i];
            texture.filterMode = FilterMode.Point;
            Rect rect = new(0f, 0f, texture.width, texture.height);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit);
            sprite.name = $"Boss_{clipName}_{i}";
            frames[i] = sprite;
        }

        CachedAnimations[cacheKey] = frames;
        return frames;
    }

    private static int ExtractTrailingNumber(string name)
    {
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(name, @"_(\d+)$");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }
}
