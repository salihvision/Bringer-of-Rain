using UnityEngine;

public class IceSpearProjectile : MonoBehaviour
{
    private Vector2 direction;
    private LayerMask groundMask;
    private LayerMask targetMask;
    private GameObject source;
    private SpriteRenderer spearRenderer;
    private SpriteRenderer trailRenderer;
    private float speed;
    private float maxRange;
    private float force;
    private float traveled;
    private float nextFrameAt;
    private float embeddedAt;
    private int damage;
    private int frameIndex;
    private bool hasHit;
    private bool isEmbedded;

    private const float HitRadius = 0.17f;
    private const float TerrainEmbedDepth = 0.13f;
    private const float EmbeddedLifetime = 4.25f;
    private const float EmbeddedFadeDuration = 0.7f;
    private const float AnimationFrameDuration = 0.045f;
    private const string SpearSpritePath = "Projectiles/IceSpearRepeatable";
    private const int SpearFrameWidth = 48;
    private const int SpearFrameHeight = 32;
    private const int SpearFrameCount = 10;
    private const float SpearPixelsPerUnit = 30f;
    private const float SpearVisualTipOffset = 0.46f;
    private const float TerrainHitLookAhead = 0.45f;
    private static readonly Color TrailColor = new(0.22f, 0.78f, 1f, 0.36f);

    private static Sprite[] cachedSpearFrames;

    public static void Spawn(
        Vector2 position,
        Vector2 aimDirection,
        int damage,
        float force,
        float speed,
        float maxRange,
        LayerMask groundMask,
        LayerMask targetMask,
        GameObject source)
    {
        GameObject spearObject = new("IceSpearProjectile");
        spearObject.transform.position = new Vector3(position.x, position.y, -0.45f);

        IceSpearProjectile spear = spearObject.AddComponent<IceSpearProjectile>();
        spear.Initialize(aimDirection, damage, force, speed, maxRange, groundMask, targetMask, source);
    }

    private void Initialize(
        Vector2 aimDirection,
        int spearDamage,
        float spearForce,
        float spearSpeed,
        float spearMaxRange,
        LayerMask spearGroundMask,
        LayerMask spearTargetMask,
        GameObject spearSource)
    {
        direction = aimDirection.sqrMagnitude > 0.01f ? aimDirection.normalized : Vector2.right;
        damage = spearDamage;
        force = spearForce;
        speed = spearSpeed;
        maxRange = spearMaxRange;
        groundMask = spearGroundMask;
        targetMask = spearTargetMask;
        source = spearSource;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        BuildVisual();
        WaterSplashAudio.PlayIceSpearThrow(transform.position);
    }

    private void Update()
    {
        if (isEmbedded)
        {
            UpdateEmbeddedArtifact();
            return;
        }

        if (hasHit)
        {
            return;
        }

        AnimateSpearSprite();

        float stepDistance = speed * Time.deltaTime;
        if (stepDistance <= 0f)
        {
            return;
        }

        if (TryResolveHit(stepDistance))
        {
            return;
        }

        transform.position += (Vector3)(direction * stepDistance);
        traveled += stepDistance;
        if (traveled >= maxRange)
        {
            Shatter();
        }
    }

    private bool TryResolveHit(float stepDistance)
    {
        int collisionMask = groundMask.value | targetMask.value;
        RaycastHit2D[] rayHits = Physics2D.RaycastAll(transform.position, direction, stepDistance + TerrainHitLookAhead, collisionMask);
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, HitRadius, direction, stepDistance, collisionMask);

        bool hasCandidate = false;
        bool candidateIsCombatTarget = false;
        bool candidateIsTerrain = false;
        float candidateDistance = float.MaxValue;
        Vector2 candidatePoint = transform.position;
        IWaterReactive candidateReactive = null;

        foreach (RaycastHit2D hit in rayHits)
        {
            ConsiderHit(hit, hit.distance, ResolveHitPoint(hit));
        }

        foreach (RaycastHit2D hit in hits)
        {
            ConsiderHit(hit, hit.distance, ResolveHitPoint(hit));
        }

        void ConsiderHit(RaycastHit2D hit, float hitDistance, Vector2 hitPoint)
        {
            if (hit.collider == null)
            {
                return;
            }

            GameObject targetObject = hit.collider.attachedRigidbody != null
                ? hit.collider.attachedRigidbody.gameObject
                : hit.collider.gameObject;

            if (targetObject == source)
            {
                return;
            }

            bool isCombatTarget = TryGetCombatReactive(targetObject, out IWaterReactive reactive);
            bool isSolidGround = !hit.collider.isTrigger && IsInLayerMask(hit.collider.gameObject.layer, groundMask);
            if (!isCombatTarget && !isSolidGround)
            {
                return;
            }

            if (!isSolidGround && hitDistance > stepDistance)
            {
                return;
            }

            if (hitDistance < candidateDistance)
            {
                hasCandidate = true;
                candidateIsCombatTarget = isCombatTarget;
                candidateIsTerrain = isSolidGround && !isCombatTarget;
                candidateReactive = reactive;
                candidateDistance = hitDistance;
                candidatePoint = hitPoint;
            }
        }

        if (!hasCandidate)
        {
            return false;
        }

        if (candidateIsCombatTarget && candidateReactive != null)
        {
            transform.position = new Vector3(candidatePoint.x, candidatePoint.y, transform.position.z);
            WaterBurstData burst = new(candidatePoint, direction, damage, force, source);
            candidateReactive.ReactToWaterBurst(burst);
            GameAudioController.Play(AudioCue.BurstHit);
            SimpleCameraFollow.RequestHitstop(0.045f);
            SimpleCameraFollow.RequestShake(0.16f, 0.2f);
            Shatter();
            return true;
        }

        if (candidateIsTerrain)
        {
            EmbedInTerrain(candidatePoint);
            return true;
        }

        Shatter();
        return true;
    }

    private Vector2 ResolveHitPoint(RaycastHit2D hit)
    {
        if (hit.point != Vector2.zero || hit.distance > 0f)
        {
            return hit.point;
        }

        return (Vector2)transform.position + direction * hit.distance;
    }

    private static bool TryGetCombatReactive(GameObject targetObject, out IWaterReactive reactive)
    {
        reactive = null;
        if (targetObject == null)
        {
            return false;
        }

        if (targetObject.TryGetComponent(out EnemyPatrol enemy))
        {
            reactive = enemy;
            return true;
        }

        if (targetObject.TryGetComponent(out BossController boss))
        {
            reactive = boss;
            return true;
        }

        return false;
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private void BuildVisual()
    {
        Sprite[] frames = GetSpearFrames();
        if (frames.Length > 0)
        {
            GameObject trailObject = new("Trail");
            trailObject.transform.SetParent(transform, false);
            trailObject.transform.localPosition = new Vector3(-SpearVisualTipOffset - 0.52f, 0f, 0f);
            trailObject.transform.localScale = new Vector3(0.74f, 0.11f, 1f);
            trailRenderer = trailObject.AddComponent<SpriteRenderer>();
            trailRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
            trailRenderer.color = TrailColor;
            trailRenderer.sortingOrder = 15;

            GameObject spriteObject = new("SpearSprite");
            spriteObject.transform.SetParent(transform, false);
            spriteObject.transform.localPosition = new Vector3(-SpearVisualTipOffset, 0f, 0f);
            spearRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spearRenderer.sprite = frames[0];
            spearRenderer.color = Color.white;
            spearRenderer.sortingOrder = 18;
            nextFrameAt = Time.time + AnimationFrameDuration;
            return;
        }

        CreateFallbackSegment("Trail", new Vector2(-1.02f, 0f), new Vector2(0.82f, 0.12f), TrailColor, 14);
        CreateFallbackSegment("Shaft", new Vector2(-0.62f, 0f), new Vector2(0.92f, 0.09f), new Color(0.62f, 0.95f, 1f, 0.96f), 16);
        CreateFallbackSegment("Core", new Vector2(-0.52f, 0f), new Vector2(0.56f, 0.045f), new Color(0.94f, 1f, 1f, 1f), 17);
        CreateFallbackSegment("Tip", new Vector2(-0.1f, 0f), new Vector2(0.28f, 0.15f), new Color(0.94f, 1f, 1f, 1f), 18);
    }

    private void AnimateSpearSprite()
    {
        Sprite[] frames = cachedSpearFrames;
        if (spearRenderer == null || frames == null || frames.Length == 0 || Time.time < nextFrameAt)
        {
            return;
        }

        frameIndex = (frameIndex + 1) % frames.Length;
        spearRenderer.sprite = frames[frameIndex];
        nextFrameAt = Time.time + AnimationFrameDuration;

        if (trailRenderer != null)
        {
            float alpha = 0.26f + Mathf.PingPong(Time.time * 8f, 0.18f);
            trailRenderer.color = new Color(TrailColor.r, TrailColor.g, TrailColor.b, alpha);
        }
    }

    private static Sprite[] GetSpearFrames()
    {
        if (cachedSpearFrames != null)
        {
            return cachedSpearFrames;
        }

        Texture2D texture = Resources.Load<Texture2D>(SpearSpritePath);
        if (texture == null)
        {
            cachedSpearFrames = System.Array.Empty<Sprite>();
            return cachedSpearFrames;
        }

        texture.filterMode = FilterMode.Point;
        int frameCount = Mathf.Max(1, texture.width / SpearFrameWidth);
        frameCount = Mathf.Min(frameCount, SpearFrameCount);
        cachedSpearFrames = new Sprite[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            Rect frameRect = new(i * SpearFrameWidth, 0f, SpearFrameWidth, SpearFrameHeight);
            Sprite frame = Sprite.Create(texture, frameRect, new Vector2(0.5f, 0.5f), SpearPixelsPerUnit, 0, SpriteMeshType.FullRect);
            frame.name = $"IceSpear_{i:00}";
            cachedSpearFrames[i] = frame;
        }

        return cachedSpearFrames;
    }

    private void CreateFallbackSegment(string objectName, Vector2 localPosition, Vector2 localScale, Color color, int sortingOrder)
    {
        GameObject segment = new(objectName);
        segment.transform.SetParent(transform, false);
        segment.transform.localPosition = localPosition;
        segment.transform.localScale = new Vector3(localScale.x, localScale.y, 1f);

        SpriteRenderer renderer = segment.AddComponent<SpriteRenderer>();
        renderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    private void EmbedInTerrain(Vector2 surfacePoint)
    {
        hasHit = true;
        isEmbedded = true;
        embeddedAt = Time.time;
        Vector2 embeddedTip = surfacePoint + direction * TerrainEmbedDepth;
        transform.position = new Vector3(embeddedTip.x, embeddedTip.y, transform.position.z);
        WaterSplashAudio.PlayIceShatter(transform.position);

        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }
    }

    private void UpdateEmbeddedArtifact()
    {
        float age = Time.time - embeddedAt;
        float fadeProgress = Mathf.InverseLerp(EmbeddedLifetime - EmbeddedFadeDuration, EmbeddedLifetime, age);
        float alpha = Mathf.Lerp(1f, 0f, fadeProgress);

        SetRendererAlpha(spearRenderer, alpha);
        if (spearRenderer == null)
        {
            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
            {
                SetRendererAlpha(renderer, alpha);
            }
        }

        if (age >= EmbeddedLifetime)
        {
            Destroy(gameObject);
        }
    }

    private static void SetRendererAlpha(SpriteRenderer renderer, float alpha)
    {
        if (renderer == null)
        {
            return;
        }

        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }

    private void Shatter()
    {
        if (hasHit)
        {
            return;
        }

        hasHit = true;
        WaterSplashAudio.PlayIceShatter(transform.position);
        SpawnShards(transform.position, direction);
        Destroy(gameObject);
    }

    private static void SpawnShards(Vector3 position, Vector2 impactDirection)
    {
        Vector2 recoil = impactDirection.sqrMagnitude > 0.01f ? -impactDirection.normalized : Vector2.left;
        for (int i = 0; i < 7; i++)
        {
            GameObject shard = new("IceSpearShard");
            shard.transform.position = new Vector3(position.x, position.y, -0.5f);
            shard.transform.localScale = new Vector3(Random.Range(0.08f, 0.2f), Random.Range(0.035f, 0.075f), 1f);

            SpriteRenderer renderer = shard.AddComponent<SpriteRenderer>();
            renderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
            renderer.color = new Color(0.76f, 0.97f, 1f, 0.82f);
            renderer.sortingOrder = 17;

            Vector2 scatter = (recoil + Random.insideUnitCircle * 0.9f).normalized * Random.Range(1.6f, 4.4f);
            IceSpearShard shardMotion = shard.AddComponent<IceSpearShard>();
            shardMotion.Initialize(scatter, Random.Range(-220f, 220f), Random.Range(0.18f, 0.34f));
        }
    }
}

public class IceSpearShard : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector2 velocity;
    private float angularVelocity;
    private float lifetime;
    private float elapsed;

    public void Initialize(Vector2 shardVelocity, float shardAngularVelocity, float shardLifetime)
    {
        velocity = shardVelocity;
        angularVelocity = shardAngularVelocity;
        lifetime = shardLifetime;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, angularVelocity * Time.deltaTime);

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(0.82f, 0f, Mathf.Clamp01(elapsed / lifetime));
            spriteRenderer.color = color;
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
