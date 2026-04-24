using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyPatrol : MonoBehaviour, IWaterReactive
{
    private Rigidbody2D body;
    private PlayerController player;
    private SpriteRenderer spriteRenderer;

    private float leftBound;
    private float rightBound;
    private float speed;
    private float direction = 1f;
    private float stunUntil;
    private float knockbackVelocity;
    private float nextTouchTime;

    private int currentHealth;
    private const int MaxHealth = 2;
    private const int ContactDamage = 1;
    private const float ChaseRange = 4.8f;
    private const float VerticalAwareness = 1.75f;
    private const float ChaseMultiplier = 1.75f;
    private const float StunDuration = 0.18f;
    private static readonly Color BaseColor = new(0.53f, 0.2f, 0.15f, 1f);
    private static readonly Color HurtColor = new(0.18f, 0.86f, 1f, 1f);

    public void Configure(float minX, float maxX, float moveSpeed)
    {
        leftBound = Mathf.Min(minX, maxX);
        rightBound = Mathf.Max(minX, maxX);
        speed = moveSpeed;
    }

    public void ReactToWaterBurst(WaterBurstData burst)
    {
        if (currentHealth <= 0)
        {
            return;
        }

        currentHealth -= burst.Damage;
        spriteRenderer.color = HurtColor;
        direction = burst.Direction.x > 0f ? -1f : 1f;
        stunUntil = Time.time + StunDuration;
        knockbackVelocity = burst.Direction.x * 5f;

        if (currentHealth <= 0)
        {
            WaterBurstVisual.Spawn(transform.position, new Vector2(1.1f, 0.9f), burst.Direction.x);
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        body.bodyType = RigidbodyType2D.Kinematic;
        body.freezeRotation = true;

        BoxCollider2D collider2D = GetComponent<BoxCollider2D>();
        collider2D.size = new Vector2(0.95f, 0.85f);

        spriteRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        spriteRenderer.color = BaseColor;
        spriteRenderer.sortingOrder = 9;

        currentHealth = MaxHealth;
    }

    private void Update()
    {
        if (currentHealth <= 0)
        {
            return;
        }

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }

        Vector2 position = body.position;

        if (Time.time < stunUntil)
        {
            position.x += knockbackVelocity * Time.deltaTime;
            knockbackVelocity = Mathf.MoveTowards(knockbackVelocity, 0f, 24f * Time.deltaTime);
        }
        else
        {
            float activeSpeed = speed;
            if (CanChasePlayer())
            {
                float deltaToPlayer = player.transform.position.x - transform.position.x;
                if (Mathf.Abs(deltaToPlayer) > 0.15f)
                {
                    direction = Mathf.Sign(deltaToPlayer);
                }

                activeSpeed *= ChaseMultiplier;
            }

            position.x += direction * activeSpeed * Time.deltaTime;
        }

        if (position.x <= leftBound)
        {
            position.x = leftBound;
            direction = 1f;
        }
        else if (position.x >= rightBound)
        {
            position.x = rightBound;
            direction = -1f;
        }

        body.MovePosition(position);

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (direction > 0f ? 1f : -1f);
        transform.localScale = scale;

        if (spriteRenderer.color != BaseColor)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, BaseColor, Time.deltaTime * 10f);
        }
    }

    private bool CanChasePlayer()
    {
        if (player == null)
        {
            return false;
        }

        Vector3 playerPosition = player.transform.position;
        return Mathf.Abs(playerPosition.y - transform.position.y) <= VerticalAwareness &&
               Mathf.Abs(playerPosition.x - transform.position.x) <= ChaseRange;
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
        if (Time.time < nextTouchTime)
        {
            return;
        }

        if (other.TryGetComponent(out PlayerController player))
        {
            nextTouchTime = Time.time + 0.8f;
            player.TakeDamage(ContactDamage, transform.position);
        }
    }
}
