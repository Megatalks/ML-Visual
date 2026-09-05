using UnityEngine;
using UnityEngine.SceneManagement;

public class Retry : MonoBehaviour
{
    public void RestartGame()
    {
        // Reloads the currently active scene to reset the level
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}