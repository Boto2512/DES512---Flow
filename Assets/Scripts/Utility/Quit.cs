using UnityEngine;
using UnityEngine.SceneManagement;

public class Quit : MonoBehaviour
{
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) { QuitGame(); }
    }

    public void QuitGame() {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex == 0) { Application.Quit(); }
        else { SceneManager.LoadScene(0); }
    }
}
