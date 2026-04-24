using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    private readonly HashSet<int> hitThisBurst = new();
    private readonly Dictionary<Collider2D, float> ignoredOneWayPlatforms = new();
    private readonly List<Collider2D> expiredOneWayPlatforms = new();

    private InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    private GameStateController gameState;
    private Rigidbody2D body;
    private CapsuleCollider2D capsule;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer[] visualRenderers;

    private LayerMask groundMask;
    private LayerMask burstMask;

    private float moveInput;
    private float moveVerticalInput;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private float nextAttackTime;
    private float invulnerableUntil;
    private float flashUntil;
    private float attackPoseUntil;
    private float landingPoseUntil;
    private float hurtPoseUntil;

    private bool facingRight = true;
    private bool inputLocked;
    private bool isGrounded;
    private bool dropInputHeld;
    private bool wasGrounded;

    private Vector3 spawnPoint;
    private Collider2D currentGroundCollider;
    private int attackVariant;
    private int attackSequence;
    private int currentHealth;

    private const float MoveSpeed = 8.45f;
    private const float GroundAcceleration = 58f;
    private const float GroundDeceleration = 66f;
    private const float AirAcceleration = 62f;
    private const float AirDeceleration = 44f;
    private const float AirTurnAcceleration = 88f;
    private const float JumpForce = 15.4f;
    private const float FallMultiplier = 3.1f;
    private const float LowJumpMultiplier = 2.2f;
    private const float CoyoteTime = 0.13f;
    private const float JumpBufferTime = 0.18f;
    private const float GroundCheckDistance = 0.14f;
    private const float DropThroughDuration = 0.3f;
    private const float DropThroughVelocity = 2.6f;
    private const float OneWayGroundTolerance = 0.16f;
    private const float AttackCooldown = 0.28f;
    private const int AttackDamage = 2;
    private const float AttackForce = 20f;
    private const float AttackLungeSpeed = 0.45f;
    private const float AttackLift = 0.2f;
    private const float AttackHitRadius = 0.58f;
    private const int MaxHealthValue = 3;
    private static readonly Vector2 AttackSize = new(3.2f, 0.9f);
    private static readonly Vector2 AttackOffset = new(0.72f, 0.2f);
    private static readonly Vector2[] AttackHitOffsets =
    {
        new(1.05f, 0.08f),
        new(1.85f, 0.2f),
        new(2.7f, 0.28f)
    };
    private static readonly Color DefaultColor = Color.white;
    private static readonly Color HurtColor = new(1f, 0.42f, 0.38f, 1f);

    public int CurrentHealth => currentHealth;
    public int MaxHealth => MaxHealthValue;
    public bool FacingRight => facingRight;
    public bool IsGrounded => isGrounded;
    public bool IsAttacking => Time.time < attackPoseUntil;
    public bool IsHurt => Time.time < hurtPoseUntil;
    public Vector2 Velocity => body != null ? body.linearVelocity : Vector2.zero;
    public float MoveAxis => moveInput;
    public int AttackVariant => attackVariant;
    public int AttackSequence => attackSequence;

    public void Configure(InputActionAsset actionsAsset, LayerMask groundLayerMask, LayerMask burstLayerMask, GameStateController controller, Vector3 initialSpawnPoint)
    {
        inputActions = actionsAsset;
        groundMask = groundLayerMask;
        burstMask = burstLayerMask;
        gameState = controller;
        spawnPoint = initialSpawnPoint;
    }

    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        moveInput = 0f;
        if (locked)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
        }
    }

    public void SetVisualHidden(bool hidden)
    {
        visualRenderers ??= GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in visualRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = !hidden;
            }
        }
    }

    public void RestoreFullHealth()
    {
        currentHealth = MaxHealthValue;
        gameState?.UpdateHealth(currentHealth, MaxHealthValue);
        spriteRenderer.color = DefaultColor;
        RestoreIgnoredPlatformCollisions();
        invulnerableUntil = Time.time + 0.85f;
        flashUntil = 0f;
        attackPoseUntil = 0f;
        landingPoseUntil = 0f;
        hurtPoseUntil = 0f;
    }

    public void TeleportToSpawn()
    {
        RestoreIgnoredPlatformCollisions();
        body.linearVelocity = Vector2.zero;
        body.position = spawnPoint;
        transform.position = spawnPoint;
    }

    public void ForceRespawn()
    {
        gameState?.RespawnPlayer(this);
    }

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (Time.time < invulnerableUntil || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        gameState?.UpdateHealth(currentHealth, MaxHealthValue);

        if (currentHealth <= 0)
        {
            ForceRespawn();
            return;
        }

        invulnerableUntil = Time.time + 1f;
        flashUntil = Time.time + 0.15f;
        attackPoseUntil = 0f;
        hurtPoseUntil = Time.time + 0.22f;

        SimpleCameraFollow.RequestHitstop(0.08f);
        SimpleCameraFollow.RequestShake(0.28f, 0.32f);

        float direction = Mathf.Sign(transform.position.x - sourcePosition.x);
        if (direction == 0f)
        {
            direction = facingRight ? -1f : 1f;
        }

        body.linearVelocity = new Vector2(direction * 6.5f, 7.5f);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        visualRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        body.gravityScale = 3f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        capsule.size = new Vector2(0.95f, 1.8f);
        spriteRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        spriteRenderer.color = DefaultColor;
        spriteRenderer.sortingOrder = 12;

        currentHealth = MaxHealthValue;
    }

    private void Start()
    {
        if (inputActions == null)
        {
            Debug.LogError("PlayerController requires an InputActionAsset reference.");
            enabled = false;
            return;
        }

        moveAction = inputActions.FindAction("Player/Move", true);
        jumpAction = inputActions.FindAction("Player/Jump", true);
        attackAction = inputActions.FindAction("Player/Attack", true);

        moveAction.Enable();
        jumpAction.Enable();
        attackAction.Enable();

        transform.position = spawnPoint;
        body.position = spawnPoint;
        gameState?.UpdateHealth(currentHealth, MaxHealthValue);
    }

    private void OnDisable()
    {
        RestoreIgnoredPlatformCollisions();
        moveAction?.Disable();
        jumpAction?.Disable();
        attackAction?.Disable();
    }

    private void Update()
    {
        if (!enabled)
        {
            return;
        }

        Vector2 rawMove = moveAction.ReadValue<Vector2>();
        moveInput = inputLocked ? 0f : Mathf.Clamp(rawMove.x, -1f, 1f);
        moveVerticalInput = inputLocked ? 0f : Mathf.Clamp(rawMove.y, -1f, 1f);

        ReleaseExpiredOneWayPlatformIgnores();

        RaycastHit2D groundHit = GetGroundHit();
        currentGroundCollider = groundHit.collider;
        isGrounded = currentGroundCollider != null;
        if (isGrounded)
        {
            coyoteCounter = CoyoteTime;

            if (!wasGrounded && body.linearVelocity.y < -7f)
            {
                landingPoseUntil = Time.time + 0.08f;
            }
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        bool dropPressed = !inputLocked && moveVerticalInput < -0.5f && !dropInputHeld;
        dropInputHeld = moveVerticalInput < -0.5f;
        if (dropPressed)
        {
            TryDropThroughOneWayPlatform();
            gameState?.NotifyPlayerActivity();
        }

        if (!inputLocked && jumpAction.WasPressedThisFrame())
        {
            jumpBufferCounter = JumpBufferTime;
            gameState?.NotifyPlayerActivity();
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (!inputLocked && attackAction.WasPressedThisFrame())
        {
            HandleAttack();
            gameState?.NotifyPlayerActivity();
        }

        if (!inputLocked && Mathf.Abs(moveInput) > 0.01f)
        {
            gameState?.NotifyPlayerActivity();
        }

        if (!inputLocked && jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, JumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            landingPoseUntil = 0f;
        }

        HandleFlip();
        HandleFallState();
        UpdateVisualState();

        if (transform.position.y < -18f)
        {
            ForceRespawn();
        }

        wasGrounded = isGrounded;
    }

    private void FixedUpdate()
    {
        float targetVelocity = inputLocked ? 0f : moveInput * MoveSpeed;
        bool reversingInAir =
            !isGrounded &&
            Mathf.Abs(targetVelocity) > 0.01f &&
            Mathf.Abs(body.linearVelocity.x) > 0.01f &&
            Mathf.Sign(targetVelocity) != Mathf.Sign(body.linearVelocity.x);

        float acceleration = Mathf.Abs(targetVelocity) > 0.01f
            ? (isGrounded ? GroundAcceleration : AirAcceleration)
            : (isGrounded ? GroundDeceleration : AirDeceleration);
        if (reversingInAir)
        {
            acceleration = AirTurnAcceleration;
        }

        float horizontalVelocity = Mathf.MoveTowards(body.linearVelocity.x, targetVelocity, acceleration * Time.fixedDeltaTime);
        body.linearVelocity = new Vector2(horizontalVelocity, body.linearVelocity.y);
    }

    private RaycastHit2D GetGroundHit()
    {
        Bounds bounds = capsule.bounds;
        Vector2 origin = new(bounds.center.x, bounds.min.y + 0.1f);
        Vector2 size = new(bounds.size.x * 0.72f, 0.18f);

        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0f, Vector2.down, GroundCheckDistance, groundMask);
        RaycastHit2D bestHit = default;
        float bestDistance = float.MaxValue;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            if (ignoredOneWayPlatforms.ContainsKey(hit.collider))
            {
                continue;
            }

            PlatformEffector2D oneWayEffector = hit.collider.GetComponent<PlatformEffector2D>();
            if (oneWayEffector != null)
            {
                bool fallingOrStill = body.linearVelocity.y <= 0.2f;
                bool aboveSurface = bounds.min.y >= hit.collider.bounds.max.y - OneWayGroundTolerance;
                if (!fallingOrStill || !aboveSurface)
                {
                    continue;
                }
            }

            if (hit.distance < bestDistance)
            {
                bestHit = hit;
                bestDistance = hit.distance;
            }
        }

        return bestHit;
    }

    private void TryDropThroughOneWayPlatform()
    {
        if (currentGroundCollider == null || currentGroundCollider.GetComponent<PlatformEffector2D>() == null)
        {
            return;
        }

        Physics2D.IgnoreCollision(capsule, currentGroundCollider, true);
        ignoredOneWayPlatforms[currentGroundCollider] = Time.time + DropThroughDuration;
        body.position += Vector2.down * 0.08f;
        body.linearVelocity = new Vector2(body.linearVelocity.x, -DropThroughVelocity);
        coyoteCounter = 0f;
        jumpBufferCounter = 0f;
        isGrounded = false;
        currentGroundCollider = null;
    }

    private void ReleaseExpiredOneWayPlatformIgnores()
    {
        expiredOneWayPlatforms.Clear();
        foreach (KeyValuePair<Collider2D, float> ignoredPlatform in ignoredOneWayPlatforms)
        {
            if (Time.time >= ignoredPlatform.Value)
            {
                expiredOneWayPlatforms.Add(ignoredPlatform.Key);
            }
        }

        foreach (Collider2D platformCollider in expiredOneWayPlatforms)
        {
            if (platformCollider != null)
            {
                Physics2D.IgnoreCollision(capsule, platformCollider, false);
            }

            ignoredOneWayPlatforms.Remove(platformCollider);
        }
    }

    private void RestoreIgnoredPlatformCollisions()
    {
        foreach (KeyValuePair<Collider2D, float> ignoredPlatform in ignoredOneWayPlatforms)
        {
            if (ignoredPlatform.Key != null)
            {
                Physics2D.IgnoreCollision(capsule, ignoredPlatform.Key, false);
            }
        }

        ignoredOneWayPlatforms.Clear();
        expiredOneWayPlatforms.Clear();
    }

    private void HandleAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + AttackCooldown;
        attackPoseUntil = Time.time + 0.42f;
        attackVariant = (attackVariant + 1) % 2;
        attackSequence++;

        float direction = facingRight ? 1f : -1f;
        float burstVerticalLift = isGrounded ? AttackLift : 0f;
        float burstHorizontalVelocity = Mathf.Clamp(
            body.linearVelocity.x + direction * AttackLungeSpeed,
            -MoveSpeed - AttackLungeSpeed,
            MoveSpeed + AttackLungeSpeed);

        body.linearVelocity = new Vector2(burstHorizontalVelocity, Mathf.Max(body.linearVelocity.y, burstVerticalLift));

        Vector2 burstOrigin = (Vector2)transform.position + new Vector2(direction * AttackOffset.x, AttackOffset.y);
        hitThisBurst.Clear();

        bool hitSomething = false;
        WaterBurstData burst = new(burstOrigin, new Vector2(direction, 0f), AttackDamage, AttackForce, gameObject);
        foreach (Vector2 hitOffset in AttackHitOffsets)
        {
            Vector2 hitCenter = (Vector2)transform.position + new Vector2(direction * hitOffset.x, hitOffset.y);
            Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, AttackHitRadius, burstMask);

            foreach (Collider2D hit in hits)
            {
                GameObject targetObject = hit.attachedRigidbody != null ? hit.attachedRigidbody.gameObject : hit.gameObject;
                int instanceId = targetObject.GetHashCode();
                if (!hitThisBurst.Add(instanceId))
                {
                    continue;
                }

                MonoBehaviour[] behaviours = targetObject.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (behaviour is IWaterReactive reactive)
                    {
                        reactive.ReactToWaterBurst(burst);
                        hitSomething = true;
                        break;
                    }
                }
            }
        }

        if (hitSomething && !isGrounded)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, Mathf.Max(body.linearVelocity.y, 2.5f));
        }

        if (hitSomething)
        {
            SimpleCameraFollow.RequestHitstop(0.05f);
            SimpleCameraFollow.RequestShake(0.18f, 0.22f);
        }

        WaterBurstVisual.Spawn(burstOrigin, AttackSize, direction);
    }

    private void HandleFlip()
    {
        if (moveInput > 0.01f)
        {
            facingRight = true;
        }
        else if (moveInput < -0.01f)
        {
            facingRight = false;
        }
    }

    private void HandleFallState()
    {
        if (body.linearVelocity.y < 0f)
        {
            body.linearVelocity += Vector2.up * Physics2D.gravity.y * (FallMultiplier - 1f) * Time.deltaTime;
        }
        else if (body.linearVelocity.y > 0f && !jumpAction.IsPressed())
        {
            body.linearVelocity += Vector2.up * Physics2D.gravity.y * (LowJumpMultiplier - 1f) * Time.deltaTime;
        }
    }

    private void UpdateVisualState()
    {
        if (Time.time < flashUntil)
        {
            spriteRenderer.color = HurtColor;
        }
        else
        {
            float pulse = Time.time < invulnerableUntil ? 0.65f + Mathf.PingPong(Time.time * 8f, 0.35f) : 1f;
            spriteRenderer.color = new Color(DefaultColor.r * pulse, DefaultColor.g * pulse, DefaultColor.b * pulse, 1f);
        }

        float runAmount = Mathf.InverseLerp(0f, MoveSpeed, Mathf.Abs(body.linearVelocity.x));
        Vector3 targetScale = Vector3.one;

        if (!isGrounded)
        {
            targetScale.x = 0.92f;
            targetScale.y = 1.08f;
        }
        else
        {
            targetScale.x = 1f + runAmount * 0.06f;
            targetScale.y = 1f - runAmount * 0.05f;
        }

        if (Time.time < landingPoseUntil)
        {
            targetScale.x *= 1.1f;
            targetScale.y *= 0.9f;
        }

        if (Time.time < attackPoseUntil)
        {
            targetScale.x *= 1.12f;
            targetScale.y *= 0.95f;
        }

        float facingSign = facingRight ? 1f : -1f;
        Vector3 desiredScale = new(targetScale.x * facingSign, targetScale.y, 1f);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, 1f - Mathf.Exp(-18f * Time.deltaTime));
    }
}
