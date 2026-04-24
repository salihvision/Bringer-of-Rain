using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    private static SimpleCameraFollow instance;

    private Transform target;
    private Rigidbody2D targetBody;
    private Vector3 offset;
    private float followSpeed;
    private Vector2 xLimits;
    private bool clampX;
    private float lookAheadX;

    private float shakeIntensity;
    private float shakeDecayRate;
    private float hitstopEndsAt;
    private Vector3 appliedShakeOffset;

    public void Configure(Transform followTarget, Vector3 followOffset, float smoothing, Vector2 horizontalLimits)
    {
        target = followTarget;
        targetBody = followTarget != null ? followTarget.GetComponent<Rigidbody2D>() : null;
        offset = followOffset;
        followSpeed = smoothing;
        xLimits = horizontalLimits;
        clampX = true;
    }

    public static void SetHorizontalLimits(Vector2 limits)
    {
        if (instance == null)
        {
            return;
        }

        instance.xLimits = limits;
    }

    public static void RequestShake(float intensity, float duration)
    {
        if (instance == null)
        {
            return;
        }

        if (intensity > instance.shakeIntensity)
        {
            instance.shakeIntensity = intensity;
        }
        instance.shakeDecayRate = Mathf.Max(instance.shakeDecayRate, intensity / Mathf.Max(duration, 0.01f));
    }

    public static void RequestHitstop(float duration)
    {
        if (instance == null)
        {
            return;
        }

        float end = Time.unscaledTime + duration;
        if (end > instance.hitstopEndsAt)
        {
            instance.hitstopEndsAt = end;
            Time.timeScale = 0f;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }
        }
    }

    private void Update()
    {
        if (hitstopEndsAt > 0f && Time.unscaledTime >= hitstopEndsAt)
        {
            hitstopEndsAt = 0f;
            Time.timeScale = 1f;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = target.position + offset;
        if (targetBody != null)
        {
            lookAheadX = Mathf.Lerp(lookAheadX, Mathf.Clamp(targetBody.linearVelocity.x * 0.18f, -1.2f, 1.2f), 1f - Mathf.Exp(-7f * Time.deltaTime));
            desired.x += lookAheadX;
            desired.y += Mathf.Clamp(targetBody.linearVelocity.y * 0.05f, -0.5f, 0.5f);
        }

        desired.z = offset.z;

        if (clampX)
        {
            desired.x = Mathf.Clamp(desired.x, xLimits.x, xLimits.y);
        }

        Vector3 basePosition = transform.position - appliedShakeOffset;
        basePosition = Vector3.Lerp(basePosition, desired, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));

        appliedShakeOffset = Vector3.zero;
        if (shakeIntensity > 0f)
        {
            Vector2 shakeOffset = Random.insideUnitCircle * shakeIntensity;
            appliedShakeOffset = new Vector3(shakeOffset.x, shakeOffset.y, 0f);
            shakeIntensity = Mathf.MoveTowards(shakeIntensity, 0f, shakeDecayRate * Time.unscaledDeltaTime);
            if (shakeIntensity <= 0f)
            {
                shakeDecayRate = 0f;
            }
        }

        transform.position = basePosition + appliedShakeOffset;
    }
}
