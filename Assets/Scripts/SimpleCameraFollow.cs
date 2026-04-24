using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;
    private float followSpeed;
    private Vector2 xLimits;
    private bool clampX;

    public void Configure(Transform followTarget, Vector3 followOffset, float smoothing, Vector2 horizontalLimits)
    {
        target = followTarget;
        offset = followOffset;
        followSpeed = smoothing;
        xLimits = horizontalLimits;
        clampX = true;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = target.position + offset;
        desired.z = offset.z;

        if (clampX)
        {
            desired.x = Mathf.Clamp(desired.x, xLimits.x, xLimits.y);
        }

        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
    }
}
