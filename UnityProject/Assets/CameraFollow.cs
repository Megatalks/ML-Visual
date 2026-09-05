using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public Vector3 offset = new Vector3(0, 12, -45);

    [Header("Downward Pitch")]
    [Range(0f, 45f)] public float pitchAngle = 15f;

    [Header("Drone Follow Delay")]
    public float forwardSmoothTime = 0.15f;
    public float lateralSmoothTime = 0.3f;

    [Header("Subtle Acceleration Pushback")]
    [Tooltip("Multiplier applied to the active acceleration to determine extra pushback distance.")]
    public float accelerationMultiplier = 0.4f;
    [Tooltip("Strict maximum distance the camera can slide back from its default offset.")]
    public float maxPushbackDistance = 6f;
    [Tooltip("How smoothly the camera slides into the pushback position. Lower values = smoother/slower.")]
    public float pushbackSmoothSpeed = 3f;
    [Tooltip("How fast the camera returns to its base offset when the player stops accelerating.")]
    public float returnToOriginSpeed = 4f;

    private float zVelocity = 0f;
    private float xVelocity = 0f;

    [Header("Drone Hover Noise")]
    public float noiseFrequency = 1.5f;
    public float noiseAmplitudeX = 0.5f;
    public float noiseAmplitudeY = 0.3f;

    [Header("Drone Roll Tilt")]
    public float maxRollTilt = 5f;
    public float rollSmoothSpeed = 5f;

    private float noiseTimer = 0f;
    private PlayerMovement playerMovement;

    private float lastSpeed = 0f;
    private float smoothPushbackOffset = 0f;

    void Start()
    {
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                lastSpeed = playerMovement.CurrentForwardSpeed;
            }
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        float dynamicZOffset = offset.z;

        if (playerMovement != null)
        {
            // 1. Calculate active acceleration
            float currentSpeed = playerMovement.CurrentForwardSpeed;
            float currentAcceleration = (currentSpeed - lastSpeed) / Time.deltaTime;
            lastSpeed = currentSpeed;

            // 2. Process pushback smoothly
            if (currentAcceleration > 0f)
            {
                float targetPushback = currentAcceleration * accelerationMultiplier;
                targetPushback = Mathf.Clamp(targetPushback, 0f, maxPushbackDistance);

                // Use Lerp instead of instant assignment to eliminate any jerkiness
                smoothPushbackOffset = Mathf.Lerp(smoothPushbackOffset, targetPushback, pushbackSmoothSpeed * Time.deltaTime);
            }
            else
            {
                // Smoothly decay back to default position when speed stabilizes or drops
                smoothPushbackOffset = Mathf.MoveTowards(smoothPushbackOffset, 0f, returnToOriginSpeed * Time.deltaTime);
            }

            dynamicZOffset -= smoothPushbackOffset;
        }

        // Apply smooth positions
        float targetZ = player.position.z + dynamicZOffset;
        float smoothedZ = Mathf.SmoothDamp(transform.position.z, targetZ, ref zVelocity, forwardSmoothTime);

        float targetX = player.position.x + offset.x;
        float smoothedX = Mathf.SmoothDamp(transform.position.x, targetX, ref xVelocity, lateralSmoothTime);

        noiseTimer += Time.deltaTime * noiseFrequency;
        float driftX = (Mathf.PerlinNoise(noiseTimer, 0f) * 2f - 1f) * noiseAmplitudeX;
        float driftY = (Mathf.PerlinNoise(0f, noiseTimer) * 2f - 1f) * noiseAmplitudeY;

        transform.position = new Vector3(smoothedX + driftX, offset.y + driftY, smoothedZ);

        float playerYRotation = player.eulerAngles.y;
        if (playerYRotation > 180f) playerYRotation -= 360f;

        float targetRoll = -playerYRotation * (maxRollTilt / 15f);
        float currentRoll = Mathf.LerpAngle(transform.localEulerAngles.z, targetRoll, Time.deltaTime * rollSmoothSpeed);

        transform.rotation = Quaternion.Euler(pitchAngle, 0f, currentRoll);
    }
}
