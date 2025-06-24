using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    private void Update() {

        Debug.Log($"TimeScale {Time.timeScale}");
        if (Input.GetKeyDown(KeyCode.R)) {
            Time.timeScale = 1f;


            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}
