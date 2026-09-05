using UnityEngine;

using UnityEngine;

public class MoveTowardsPlayer : MonoBehaviour
{
    public float moveSpeed = 8f;

    void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }
}