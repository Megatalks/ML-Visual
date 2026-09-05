using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Forward Speed Limits")]
    public float baseForwardSpeed = 50f;
    public float maxForwardSpeed = 70f;
    public float minForwardSpeed = 30f;

    [Header("Acceleration & Braking")]
    public float accelerationRate = 10f;
    public float gradualBrakeRate = 4.0f;
    public float driftBrakeRate = 1.5f;
    public float passiveDecelRate = 2f;

    [Header("Drift Settings (Tuned Down)")]
    public float maxDriftSpeed = 12f;
    public float driftBuildUp = 12f;
    public float driftRecovery = 6f;
    public float driftForwardSpeedFloor = 40f;

    [Header("Visual Juice")]
    public float maxDriftAngle = 15f;
    public float rotationJerkSpeed = 8f;

    [Header("Soft Wall Bounce")]
    public float bouncePushForce = 4f;

    private float currentForwardSpeed;
    public float CurrentForwardSpeed => currentForwardSpeed;

    private float currentDriftSpeed = 0f;
    public float CurrentDriftSpeed => currentDriftSpeed;

    private float currentVisualAngle = 0f;
    public float CurrentVisualAngle => currentVisualAngle;

    private float driftDuration = 0f;

    // Crash spin-out states
    private bool isSpinningOut = false;
    private float spinLateralVelocityX;

    private float spinRotationSpeedY;

    private CarCollision carCollision;

    void Start()
    {
        currentForwardSpeed = baseForwardSpeed;
        carCollision = GetComponent<CarCollision>();
        if (carCollision != null) carCollision.IsMoving = true;
    }

    public void TriggerCrashSpinOut()
    {
        isSpinningOut = true;
        // Determine a random spin direction and sideways slide speed
        spinLateralVelocityX = Random.Range(0f, 1f) > 0.5f ? 18f : -18f;
        spinRotationSpeedY = Random.Range(0f, 1f) > 0.5f ? 500f : -500f;
    }

    void Update()
    {
        // SPIN OUT LOGIC LOOP
        if (carCollision != null && !carCollision.IsMoving)
        {
            if (isSpinningOut)
            {
                // Maintain forward rolling speed but rapidly slide to a halt
                currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, 0f, 35f * Time.deltaTime);

                // Slide sideways
                transform.position += new Vector3(spinLateralVelocityX * Time.deltaTime, 0f, 0f);
                spinLateralVelocityX = Mathf.MoveTowards(spinLateralVelocityX, 0f, 10f * Time.deltaTime);

                // Spin chassis rotation on Y axis
                currentVisualAngle += spinRotationSpeedY * Time.deltaTime;
                spinRotationSpeedY = Mathf.MoveTowards(spinRotationSpeedY, 0f, 250f * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, currentVisualAngle, 0);

                // Bound player within roadside limits (+-16) during the slide
                float crashX = transform.position.x;
                if (crashX >= 16f) { crashX = 16f; spinLateralVelocityX = -Mathf.Abs(spinLateralVelocityX) * 0.7f; }
                else if (crashX <= -16f) { crashX = -16f; spinLateralVelocityX = Mathf.Abs(spinLateralVelocityX) * 0.7f; }
                transform.position = new Vector3(crashX, transform.position.y, transform.position.z);

                if (currentForwardSpeed <= 0f && spinLateralVelocityX == 0f && spinRotationSpeedY == 0f)
                {
                    isSpinningOut = false;
                }
            }
            return;
        }

        // NORMAL MOVEMENT CALCULATIONS
        float keyHorizontalInput = Input.GetAxisRaw("Horizontal");
        float horizontalInput = keyHorizontalInput != 0 ? keyHorizontalInput :
            (VirtualInput.MoveLeft ? -1f : (VirtualInput.MoveRight ? 1f : 0f));

        float keyVerticalInput = Input.GetAxisRaw("Vertical");
        float verticalInput = keyVerticalInput !=0 ? keyVerticalInput :
            (VirtualInput.Accel ? -1f : (VirtualInput.Brake ? 1f : 0f));

        if (verticalInput > 0)
        {
            currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, maxForwardSpeed, accelerationRate * Time.deltaTime);
        }
        else if (verticalInput < 0)
        {
            currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, minForwardSpeed, gradualBrakeRate * Time.deltaTime);
        }
        else if (horizontalInput != 0)
        {
            currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, driftForwardSpeedFloor, driftBrakeRate * Time.deltaTime);
        }
        else
        {
            currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, baseForwardSpeed, passiveDecelRate * Time.deltaTime);
        }

        if (horizontalInput != 0)
        {
            driftDuration += Time.deltaTime;
            float t = Mathf.Clamp01(driftDuration);
            float sqrtFactor = Mathf.Sqrt(t);
            float targetSpeed = horizontalInput * maxDriftSpeed * sqrtFactor;
            currentDriftSpeed = Mathf.MoveTowards(currentDriftSpeed, targetSpeed, driftBuildUp * Time.deltaTime);
            float targetAngle = horizontalInput * maxDriftAngle * sqrtFactor;
            currentVisualAngle = Mathf.Lerp(currentVisualAngle, targetAngle, rotationJerkSpeed * Time.deltaTime);
        }
        else
        {
            driftDuration = 0f;
            currentDriftSpeed = Mathf.MoveTowards(currentDriftSpeed, 0f, driftRecovery * Time.deltaTime);
            currentVisualAngle = Mathf.Lerp(currentVisualAngle, 0f, driftRecovery * Time.deltaTime);
        }

        float forwardMove = currentForwardSpeed * Time.deltaTime;
        float horizontalMove = currentDriftSpeed * Time.deltaTime;

        transform.Translate(new Vector3(horizontalMove, 0, 0), Space.World);

        float currentX = transform.position.x;
        if (currentX >= 16f)
        {
            currentX = 16f;
            currentDriftSpeed = -bouncePushForce;
            currentVisualAngle = Mathf.Lerp(currentVisualAngle, 0f, Time.deltaTime * 10f);
            driftDuration = 0f;
        }
        else if (currentX <= -16f)
        {
            currentX = -16f;
            currentDriftSpeed = bouncePushForce;
            currentVisualAngle = Mathf.Lerp(currentVisualAngle, 0f, Time.deltaTime * 10f);
            driftDuration = 0f;
        }

        transform.position = new Vector3(currentX, transform.position.y, transform.position.z);
        transform.rotation = Quaternion.Euler(0, currentVisualAngle, 0);
    }
    public void ResetToStart(Vector3 startPos)
    {
        // teleport to start and reset pos
        transform.localPosition = startPos;
        transform.rotation = Quaternion.identity;

        // reset all movement tracking variables
        currentForwardSpeed = baseForwardSpeed;
        currentDriftSpeed = 0f;
        currentVisualAngle = 0f;
        driftDuration = 0f;
        isSpinningOut = false;

        // kill all physics forces
        //Rigidbody rb = GetComponent<Rigidbody>();
        //if (rb != null)
        //{
        //    rb.linearVelocity = Vector3.zero;
        //    rb.angularVelocity = Vector3.zero;
        //}
    }
}


public class RaycastGizmoDetector : MonoBehaviour
{
    public float maxDistance = 10f;

    // This runs automatically in the Scene View
    private void OnDrawGizmos()
    {
        // 1. Setup origin and direction
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        // 2. Perform the raycast to check for hits
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
        {
            // Draw a red line to the hit point if it hits something
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, hit.point);

            // Draw a small sphere at the impact point
            Gizmos.DrawWireSphere(hit.point, 0.2f);
        }
        else
        {
            // Draw a green ray to its maximum distance if it hits nothing
            Gizmos.color = Color.green;
            Gizmos.DrawRay(origin, direction * maxDistance);
        }
    }
}