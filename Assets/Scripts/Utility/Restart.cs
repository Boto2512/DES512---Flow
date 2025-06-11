using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
            Time.timeScale = 1f;
        }
    }
}
