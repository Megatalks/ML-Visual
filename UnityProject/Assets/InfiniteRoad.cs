using UnityEngine;

public class InfiniteRoad : MonoBehaviour
{
    [Range(0f, 150f)]
    public float roadLength = 20f;
    public int roadIndex;
    [Range(-50f, 50f)]
    public float manualOffset = 50f;

    private Transform player;
    private PlayerMovement playerMovement;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        if (playerMovement == null) return;

        float forwardMove = playerMovement.CurrentForwardSpeed * Time.deltaTime;
        transform.Translate(new Vector3(0, 0, -forwardMove), Space.World);

        if (transform.position.z < -roadLength - manualOffset)
        {

            transform.position += new Vector3(0, 0, roadLength * 6);
        }
    }
}
