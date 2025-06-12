using UnityEngine;

public class Quit : MonoBehaviour
{
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) { QuitGame(); }
    }

    public void QuitGame() {
        Application.Quit();
    }
}
