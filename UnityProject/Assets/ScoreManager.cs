using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Score")]
    public float score = 0;

    [Header("Scoring")]
    public float pointsPerSecondDriving = 10f;
    public float speedBonusMultiplier = 2f;

    [Header("UI")]
    public TMP_Text scoreText;

    private PlayerMovement car; // Replace with your car movement script

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        car = FindAnyObjectByType<PlayerMovement>(); // Replace CarController with your script name
    }

    void Update()
    {
        if (car == null) return;

        float speed = car.CurrentForwardSpeed; // Replace with however your speed is stored

        // Gain points while moving
        if (speed > 0.1f)
        {
            score += pointsPerSecondDriving * Time.deltaTime;
        }

        // Extra points for speeding
        if (speed > 20f) // Speed threshold
        {
            score += pointsPerSecondDriving * speedBonusMultiplier * Time.deltaTime;
        }

        UpdateUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Coins: " + Mathf.RoundToInt(score);
    }
}
