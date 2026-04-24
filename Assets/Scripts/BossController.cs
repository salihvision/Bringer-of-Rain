using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BossController : MonoBehaviour, IWaterReactive
{
    private enum Phase
    {
        One,
        Two,
        Defeated
    }

    private enum AttackState
    {
        Idle,
        SlamWindup,
        Slamming,
        Exposed,
        DashWindup,
        Dashing
    }

    private const int MaxHealthValue = 32;
    private const int Phase2Threshold = 16;
    private const float SlamWindupDuration = 0.7f;
    private const float SlamFallSpeed = 24f;
    private const float SlamRiseSpeed = 6.5f;
    private const float ExposedDuration = 0.7f;
    private const float DashWindupDuration = 0.45f;
    private const float DashSpeed = 13.5f;
    private const float ActionGapPhase1 = 1.3f;
    private const float ActionGapPhase2 = 0.85f;
    private const float SummonInterval = 8f;

    private static readonly Color IdleColor = new(0.78f, 0.42f, 0.24f, 1f);
    private static readonly Color WindupColor = new(1f, 0.94f, 0.32f, 1f);
    private static readonly Color SlamColor = new(1f, 0.55f, 0.32f, 1f);
    private static readonly Color ExposedColor = new(0.62f, 1f, 0.96f, 1f);
    private static readonly Color DashColor = new(1f, 0.32f, 0.32f, 1f);
    private static readonly Color Phase2Color = new(0.55f, 0.22f, 0.62f, 1f);
    private static readonly Color HurtColor = new(0.96f, 1f, 1f, 1f);

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D bodyCollider;
    private PlayerController player;
    private GameStateController gameState;
    private Transform arenaRoot;

    private Phase phase = Phase.One;
    private AttackState state = AttackState.Idle;
    private float stateEndsAt;
    private float nextActionAt;
    private float nextSummonAt;
    private float nextTouchTime;
    private float arenaLeftBound;
    private float arenaRightBound;
    private float floorY;
    private float idleY;
    private float slamLandY;
    private float dashSign;
    private int dashesRemaining;
    private int currentHealth;
    private bool initialized;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => MaxHealthValue;

    public void Configure(GameStateController controller, Transform spawnRoot, float leftBound, float rightBound, float groundY)
    {
        gameState = controller;
        arenaRoot = spawnRoot;
        arenaLeftBound = leftBound;
        arenaRightBound = rightBound;
        floorY = groundY;
        idleY = groundY + 2.0f;
        slamLandY = groundY + 1.5f;

        Vector3 position = transform.position;
        position.y = idleY;
        transform.position = position;

        initialized = true;
        ResetEncounterTimers();
    }

    private void OnEnable()
    {
        if (initialized)
        {
            ResetEncounterTimers();
        }
    }

    private void ResetEncounterTimers()
    {
        state = AttackState.Idle;
        nextActionAt = Time.time + 1.6f;
        nextSummonAt = Time.time + 6.5f;
    }

    public void ReactToWaterBurst(WaterBurstData burst)
    {
        if (phase == Phase.Defeated)
        {
            return;
        }

        if (state != AttackState.Exposed)
        {
            spriteRenderer.color = HurtColor;
            return;
        }

        currentHealth -= burst.Damage;
        SimpleCameraFollow.RequestHitstop(0.07f);
        SimpleCameraFollow.RequestShake(0.22f, 0.28f);
        WaterBurstVisual.Spawn(transform.position, new Vector2(1.1f, 0.85f), burst.Direction.x);
        spriteRenderer.color = HurtColor;

        if (currentHealth <= 0)
        {
            DefeatBoss();
            return;
        }

        if (phase == Phase.One && currentHealth <= Phase2Threshold)
        {
            EnterPhaseTwo();
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<BoxCollider2D>();

        body.bodyType = RigidbodyType2D.Kinematic;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        bodyCollider.size = new Vector2(0.85f, 1.2f);

        spriteRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        spriteRenderer.color = IdleColor;
        spriteRenderer.sortingOrder = 11;

        currentHealth = MaxHealthValue;
    }

    private void Update()
    {
        if (!initialized || phase == Phase.Defeated)
        {
            return;
        }

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }

        switch (state)
        {
            case AttackState.Idle:
                TickIdle();
                break;
            case AttackState.SlamWindup:
                TickSlamWindup();
                break;
            case AttackState.Slamming:
                TickSlamming();
                break;
            case AttackState.Exposed:
                TickExposed();
                break;
            case AttackState.DashWindup:
                TickDashWindup();
                break;
            case AttackState.Dashing:
                TickDashing();
                break;
        }

        if (phase == Phase.Two && Time.time >= nextSummonAt)
        {
            SpawnMiniSentry();
            nextSummonAt = Time.time + SummonInterval;
        }

        spriteRenderer.color = Color.Lerp(spriteRenderer.color, TargetColor(), Time.deltaTime * 11f);

        Vector3 facingScale = transform.localScale;
        float facingSign = Mathf.Abs(facingScale.x);
        if (player != null)
        {
            facingSign *= player.transform.position.x < transform.position.x ? -1f : 1f;
        }
        transform.localScale = new Vector3(facingSign, facingScale.y, facingScale.z);
    }

    private Color TargetColor()
    {
        Color baseColor = phase == Phase.Two ? Phase2Color : IdleColor;
        return state switch
        {
            AttackState.SlamWindup => WindupColor,
            AttackState.Slamming => SlamColor,
            AttackState.Exposed => ExposedColor,
            AttackState.DashWindup => DashColor,
            AttackState.Dashing => DashColor,
            _ => baseColor
        };
    }

    private void TickIdle()
    {
        if (player != null)
        {
            float driftSpeed = phase == Phase.Two ? 3.4f : 2.4f;
            float dx = player.transform.position.x - transform.position.x;
            float step = Mathf.Clamp(dx, -driftSpeed * Time.deltaTime, driftSpeed * Time.deltaTime);
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x + step, arenaLeftBound + 1.4f, arenaRightBound - 1.4f);
            position.y = Mathf.MoveTowards(position.y, idleY, SlamRiseSpeed * Time.deltaTime);
            transform.position = position;
        }

        if (Time.time >= nextActionAt)
        {
            ChooseNextAttack();
        }
    }

    private void ChooseNextAttack()
    {
        bool useDash = phase == Phase.Two && Random.value < 0.45f;
        if (useDash)
        {
            state = AttackState.DashWindup;
            stateEndsAt = Time.time + DashWindupDuration;
            dashSign = player != null ? Mathf.Sign(player.transform.position.x - transform.position.x) : 1f;
            if (dashSign == 0f)
            {
                dashSign = 1f;
            }
            dashesRemaining = 2;

            Vector3 position = transform.position;
            position.y = idleY;
            transform.position = position;
        }
        else
        {
            state = AttackState.SlamWindup;
            float windup = phase == Phase.Two ? SlamWindupDuration * 0.78f : SlamWindupDuration;
            stateEndsAt = Time.time + windup;
        }
    }

    private void TickSlamWindup()
    {
        Vector3 position = transform.position;
        position.y = Mathf.MoveTowards(position.y, idleY + 0.8f, 1.8f * Time.deltaTime);
        transform.position = position;

        if (Time.time >= stateEndsAt)
        {
            state = AttackState.Slamming;
        }
    }

    private void TickSlamming()
    {
        Vector3 position = transform.position;
        position.y = Mathf.MoveTowards(position.y, slamLandY, SlamFallSpeed * Time.deltaTime);
        transform.position = position;

        if (Mathf.Abs(position.y - slamLandY) <= 0.001f)
        {
            BossShockwave.Spawn(new Vector2(transform.position.x, floorY + 0.4f), 1f);
            BossShockwave.Spawn(new Vector2(transform.position.x, floorY + 0.4f), -1f);
            SimpleCameraFollow.RequestShake(0.4f, 0.45f);
            SimpleCameraFollow.RequestHitstop(0.05f);
            state = AttackState.Exposed;
            stateEndsAt = Time.time + ExposedDuration;
        }
    }

    private void TickExposed()
    {
        Vector3 position = transform.position;
        position.y = Mathf.MoveTowards(position.y, idleY, SlamRiseSpeed * 0.45f * Time.deltaTime);
        transform.position = position;

        if (Time.time >= stateEndsAt)
        {
            state = AttackState.Idle;
            nextActionAt = Time.time + (phase == Phase.Two ? ActionGapPhase2 : ActionGapPhase1);
        }
    }

    private void TickDashWindup()
    {
        if (Time.time >= stateEndsAt)
        {
            state = AttackState.Dashing;
        }
    }

    private void TickDashing()
    {
        Vector3 position = transform.position;
        position.x += dashSign * DashSpeed * Time.deltaTime;

        if (position.x <= arenaLeftBound + 1.1f)
        {
            position.x = arenaLeftBound + 1.1f;
            dashSign = 1f;
            dashesRemaining--;
        }
        else if (position.x >= arenaRightBound - 1.1f)
        {
            position.x = arenaRightBound - 1.1f;
            dashSign = -1f;
            dashesRemaining--;
        }

        transform.position = position;

        if (dashesRemaining <= 0)
        {
            state = AttackState.Exposed;
            stateEndsAt = Time.time + ExposedDuration * 0.65f;
        }
    }

    private void EnterPhaseTwo()
    {
        phase = Phase.Two;
        SimpleCameraFollow.RequestShake(0.5f, 0.6f);
        SimpleCameraFollow.RequestHitstop(0.12f);
        WaterBurstVisual.Spawn(transform.position, new Vector2(2.2f, 1.6f), 1f);
        state = AttackState.Idle;
        nextActionAt = Time.time + 1.1f;
        nextSummonAt = Time.time + 4f;
        gameState?.ShowTransientMessage("The warden seethes. Tide answers harder.", 2.4f);
    }

    private void SpawnMiniSentry()
    {
        if (arenaRoot == null)
        {
            return;
        }

        float spawnX = Random.value < 0.5f ? arenaLeftBound + 1.5f : arenaRightBound - 1.5f;
        GameObject mini = new("BossMinion");
        mini.transform.SetParent(arenaRoot, false);
        mini.transform.position = new Vector3(spawnX, floorY + 0.7f, 0f);
        mini.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

        mini.AddComponent<SpriteRenderer>();
        mini.AddComponent<BoxCollider2D>();
        mini.AddComponent<Rigidbody2D>();

        EnemyPatrol enemy = mini.AddComponent<EnemyPatrol>();
        enemy.Configure(arenaLeftBound + 1f, arenaRightBound - 1f, 2.1f, 1.25f);
        EnemySpriteAnimator animator = mini.AddComponent<EnemySpriteAnimator>();
        animator.Configure("MaskDude");

        WaterBurstVisual.Spawn(mini.transform.position, new Vector2(0.9f, 0.7f), 1f);
    }

    private void DefeatBoss()
    {
        phase = Phase.Defeated;
        SimpleCameraFollow.RequestHitstop(0.2f);
        SimpleCameraFollow.RequestShake(0.7f, 0.95f);
        WaterBurstVisual.Spawn(transform.position, new Vector2(2.6f, 1.8f), 1f);
        WaterBurstVisual.Spawn(transform.position, new Vector2(2.6f, 1.8f), -1f);
        gameState?.NotifyBossDefeated();
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (phase == Phase.Defeated || Time.time < nextTouchTime)
        {
            return;
        }

        if (other.TryGetComponent(out PlayerController hitPlayer))
        {
            int damage = state == AttackState.Dashing ? 2 : 1;
            nextTouchTime = Time.time + (state == AttackState.Dashing ? 0.55f : 0.8f);
            hitPlayer.TakeDamage(damage, transform.position);
        }
    }
}
