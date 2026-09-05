using UnityEngine;
using Unity.MLAgents;

public class CarCollision : MonoBehaviour
{
    public GameObject RetryButton;
    private PlayerMovement playerMovement;
    private RacingAgent racingAgent;
    public bool IsMoving = true;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        racingAgent = GetComponent<RacingAgent>(); // grab agent component
        IsMoving = true;
        if (RetryButton != null)
        {
            RetryButton.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        // Guard clause to find the root GameObject cleanly in case of child objects
        GameObject hitObject = collision.gameObject;
        if (hitObject == null) return;

        bool isAIPlaying = Academy.Instance.IsCommunicatorOn; // check if python script is connected

        if (hitObject.CompareTag("Human"))
        {
            Debug.Log("Human hit! Forcing destruction.");
            
            if (isAIPlaying && racingAgent != null)
            {
                racingAgent.AddReward(-2.5f); // reward penalty
            }
            else if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(-100);
            }
            Destroy(hitObject);
            return;
        }

        if (hitObject.CompareTag("Zombie"))
        {
            Debug.Log("Zombie hit! Forcing destruction.");
            if (isAIPlaying && racingAgent != null)
            {
                racingAgent.AddReward(1.0f);
            }
            else if (ScoreManager.Instance != null)
            {

                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(50);
                }
            }
            Destroy(hitObject);
            return;
        }

        if (hitObject.CompareTag("Car"))
        {
            // Stop early if the player has already initiated a spinout slide
            if (!IsMoving) return;

            if (isAIPlaying && racingAgent != null)
            {
                racingAgent.AddReward(-5.0f);
                racingAgent.EndEpisode();
                return;
            }


            IsMoving = false;

            if (playerMovement != null)
            {
                playerMovement.TriggerCrashSpinOut();
            }

            CharacterSpawner.MovingObstacle obstacleMovement = hitObject.GetComponent<CharacterSpawner.MovingObstacle>();
            if (obstacleMovement != null)
            {
                float impactSpeed = (playerMovement != null) ? playerMovement.CurrentForwardSpeed : 50f;
                obstacleMovement.TriggerCrashBounce(impactSpeed);
            }

            if (RetryButton != null)
            {
                RetryButton.SetActive(true);
            }
        }
    }
}
