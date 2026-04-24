using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(SpriteRenderer))]
public class CharacterSpriteAnimator : MonoBehaviour
{
    private static readonly Vector2Int[] IdleFrames =
    {
        new(0, 0),
        new(1, 0),
        new(0, 1),
        new(1, 1)
    };

    private static readonly Vector2Int[] RunFrames =
    {
        new(0, 3),
        new(1, 3),
        new(2, 3),
        new(3, 3),
        new(4, 3),
        new(5, 3),
        new(6, 3),
        new(7, 3)
    };

    private static readonly Vector2Int[] JumpFrames =
    {
        new(0, 2),
        new(1, 2)
    };

    private static readonly Vector2Int[] FallFrames =
    {
        new(2, 2),
        new(3, 2)
    };

    private static readonly Vector2Int[] AttackFrames =
    {
        new(0, 4),
        new(1, 4),
        new(2, 4),
        new(3, 4),
        new(4, 4),
        new(5, 4)
    };

    private static readonly Dictionary<Vector2Int, Sprite> CachedSprites = new();
    private static Texture2D cachedTexture;

    private PlayerController player;
    private SpriteRenderer spriteRenderer;

    private Vector2Int[] activeFrames;
    private float frameTime;
    private float nextFrameAt;
    private int frameIndex;

    private const string ResourcePath = "Character/AnimationSheet_Character";
    private const int CellSize = 32;
    private const float PixelsPerUnit = 18f;

    private enum AnimationState
    {
        Idle,
        Run,
        Jump,
        Fall,
        Attack
    }

    private AnimationState currentState;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cachedTexture = LoadTexture();
    }

    private void Start()
    {
        if (cachedTexture == null)
        {
            enabled = false;
            return;
        }

        cachedTexture.filterMode = FilterMode.Point;
        SetAnimation(AnimationState.Idle, true);
    }

    private void Update()
    {
        if (cachedTexture == null)
        {
            return;
        }

        AnimationState nextState = ResolveState();
        bool forceRestart = nextState != currentState;
        SetAnimation(nextState, forceRestart);

        if (activeFrames == null || activeFrames.Length == 0)
        {
            return;
        }

        if (Time.time >= nextFrameAt)
        {
            frameIndex = (frameIndex + 1) % activeFrames.Length;
            ApplyFrame();
            nextFrameAt = Time.time + frameTime;
        }
    }

    private AnimationState ResolveState()
    {
        if (player.IsAttacking)
        {
            return AnimationState.Attack;
        }

        Vector2 velocity = player.Velocity;
        if (!player.IsGrounded)
        {
            return velocity.y >= 0.2f ? AnimationState.Jump : AnimationState.Fall;
        }

        return Mathf.Abs(velocity.x) > 0.35f ? AnimationState.Run : AnimationState.Idle;
    }

    private void SetAnimation(AnimationState nextState, bool forceRestart)
    {
        if (!forceRestart && nextState == currentState)
        {
            return;
        }

        currentState = nextState;
        activeFrames = nextState switch
        {
            AnimationState.Run => RunFrames,
            AnimationState.Jump => JumpFrames,
            AnimationState.Fall => FallFrames,
            AnimationState.Attack => AttackFrames,
            _ => IdleFrames
        };

        frameTime = nextState switch
        {
            AnimationState.Run => 0.08f,
            AnimationState.Attack => 0.06f,
            AnimationState.Jump => 0.13f,
            AnimationState.Fall => 0.13f,
            _ => 0.22f
        };

        frameIndex = 0;
        ApplyFrame();
        nextFrameAt = Time.time + frameTime;
    }

    private void ApplyFrame()
    {
        spriteRenderer.sprite = GetSprite(activeFrames[frameIndex]);
    }

    private static Texture2D LoadTexture()
    {
        if (cachedTexture != null)
        {
            return cachedTexture;
        }

        Texture2D texture = Resources.Load<Texture2D>(ResourcePath);
        if (texture == null)
        {
            Debug.LogWarning($"Character sprite sheet not found at Resources/{ResourcePath}. Using fallback sprite.");
        }

        return texture;
    }

    private static Sprite GetSprite(Vector2Int cell)
    {
        if (CachedSprites.TryGetValue(cell, out Sprite sprite))
        {
            return sprite;
        }

        if (cachedTexture == null)
        {
            return PrimitiveSpriteLibrary.SquareSprite;
        }

        Rect rect = new(cell.x * CellSize, cachedTexture.height - ((cell.y + 1) * CellSize), CellSize, CellSize);
        sprite = Sprite.Create(cachedTexture, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit);
        sprite.name = $"Character_{cell.x}_{cell.y}";
        CachedSprites[cell] = sprite;
        return sprite;
    }
}
