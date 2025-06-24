using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    private void Update() {

        if (Input.GetKeyDown(KeyCode.R)) {
            Time.timeScale = 1f;


            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}
