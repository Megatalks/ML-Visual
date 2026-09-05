using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject zombiePrefab;
    public GameObject malePrefab;
    public GameObject femalePrefab;
    public GameObject carPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Spawn Probabilities")]
    [Range(0f, 1f)] public float zombieProbability = 0.35f;
    [Range(0f, 1f)] public float carProbability = 0.20f;

    [Header("Spawn Distance (Units)")]
    [Tooltip("Minimum distance covered by player between spawns.")]
    public float minSpawnDistance = 15f;
    [Tooltip("Maximum distance covered by player between spawns.")]
    public float maxSpawnDistance = 30f;

    [Header("Fallback Settings")]
    [Tooltip("Minimum speed used to prevent divide-by-zero or infinite delay when stationary.")]
    public float minimumSpeedCap = 5f;

    [Header("Car Relative Speed Limits")]
    public float minCarRelativeSpeed = -15f;
    public float maxCarRelativeSpeed = 15f;

    private int lastLane = -1;
    private PlayerMovement playerMovement;
    private GlobalWorldCurver worldCurver;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        }

        worldCurver = Object.FindFirstObjectByType<GlobalWorldCurver>();
        ScheduleNextSpawn();
    }

    private void SpawnObject()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (zombiePrefab == null || malePrefab == null || femalePrefab == null || carPrefab == null) return;

        int randomLane;
        do
        {
            randomLane = Random.Range(0, spawnPoints.Length);
        }
        while (spawnPoints.Length > 1 && randomLane == lastLane);

        lastLane = randomLane;
        Transform selectedSpawnPoint = spawnPoints[randomLane];

        float randomValue = Random.value;
        GameObject selectedPrefab;
        bool isCar = false;

        if (randomValue < zombieProbability)
        {
            selectedPrefab = zombiePrefab;
        }
        else if (randomValue < zombieProbability + carProbability)
        {
            selectedPrefab = carPrefab;
            isCar = true;
        }
        else
        {
            selectedPrefab = (Random.value < 0.5f) ? malePrefab : femalePrefab;
        }

        GameObject spawnedObj = Instantiate(selectedPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);

        if (worldCurver != null)
        {
            worldCurver.ApplyCurveToTarget(spawnedObj);
        }

        MovingObstacle movement = spawnedObj.AddComponent<MovingObstacle>();
        float speedVariance = isCar ? Random.Range(minCarRelativeSpeed, maxCarRelativeSpeed) : 0f;
        movement.Initialize(playerMovement, speedVariance);

        ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        // Get target distance gap
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

        // Fetch player's current speed, clamped above minimumSpeedCap to avoid division by zero
        float currentSpeed = minimumSpeedCap;
        if (playerMovement != null)
        {
            currentSpeed = Mathf.Max(playerMovement.CurrentForwardSpeed, minimumSpeedCap);
        }

        // Time = Distance / Speed
        float delay = randomDistance / currentSpeed;

        Invoke(nameof(SpawnObject), delay);
    }

    // --- NESTED COMPONENT FOR INDIVIDUAL MOVEMENT & VEHICLE CRASH BOUNCING ---
    public class MovingObstacle : MonoBehaviour
    {
        private PlayerMovement pm;
        private float carRelativeSpeed = 0f;
        private float destroyZDepth = -50f;

        private bool isCrashed = false;
        private Vector3 flyVelocity;
        private float rotationSpinSpeed;

        public bool IsCrashed => isCrashed;
        public Vector3 FlyVelocity => flyVelocity;

        public void Initialize(PlayerMovement playerMovementScript, float individualSpeedVariance = 0f)
        {
            pm = playerMovementScript;
            carRelativeSpeed = individualSpeedVariance;
        }

        public void TriggerCrashBounce(float playerImpactSpeed)
        {
            if (isCrashed) return;
            isCrashed = true;

            float launchX = Random.Range(-1f, 1f) * 20f;
            float launchZ = playerImpactSpeed * 1.4f;
            flyVelocity = new Vector3(launchX, 0f, launchZ);

            rotationSpinSpeed = Random.Range(-400f, 400f);
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCrashed && other.gameObject.CompareTag("Car"))
            {
                MovingObstacle otherCar = other.gameObject.GetComponent<MovingObstacle>();
                if (otherCar != null && !otherCar.IsCrashed)
                {
                    Debug.Log(gameObject.name + " triggered a chain reaction pile-up on " + other.gameObject.name);

                    float cascadingSpeed = Mathf.Max(flyVelocity.magnitude, 25f);
                    otherCar.TriggerCrashBounce(cascadingSpeed);
                }
            }
        }

        void Update()
        {
            if (isCrashed)
            {
                transform.position += flyVelocity * Time.deltaTime;
                transform.Rotate(0f, rotationSpinSpeed * Time.deltaTime, 0f, Space.Self);

                flyVelocity = Vector3.MoveTowards(flyVelocity, Vector3.zero, 15f * Time.deltaTime);
                rotationSpinSpeed = Mathf.MoveTowards(rotationSpinSpeed, 0f, 200f * Time.deltaTime);

                float hitCarX = transform.position.x;
                if (hitCarX >= 16f)
                {
                    hitCarX = 16f;
                    flyVelocity.x = -Mathf.Abs(flyVelocity.x);
                }
                else if (hitCarX <= -16f)
                {
                    hitCarX = -16f;
                    flyVelocity.x = Mathf.Abs(flyVelocity.x);
                }
                transform.position = new Vector3(hitCarX, transform.position.y, transform.position.z);

                return;
            }

            if (pm == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    pm = playerObj.GetComponent<PlayerMovement>();
                }
                if (pm == null) return;
            }

            float finalBackwardSpeed = pm.CurrentForwardSpeed - carRelativeSpeed;
            transform.position += new Vector3(0, 0, -finalBackwardSpeed * Time.deltaTime);

            if (transform.position.z < destroyZDepth)
            {
                Destroy(gameObject);
            }
        }
    }
}